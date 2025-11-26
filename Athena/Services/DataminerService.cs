using System.Diagnostics;
using EpicManifestParser.Api;
using CUE4Parse.Compression;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.VirtualFileSystem;
using Athena.Models.API.Responses;

namespace Athena.Services;

public class DataminerService
{
    public ManifestService Manifest = null!;
    public StreamedFileProvider Provider = null!;

    public readonly List<GameFile> AllEntries = [];
    public readonly List<GameFile> NewEntries = [];

    public async Task Initialize()
    {
        Log.ForContext("NoConsole", true).Information("UE version: {version}", AppSettings.Default.EngineVersion);

        Manifest = new();
        Provider = new("", new(AppSettings.Default.EngineVersion), StringComparer.OrdinalIgnoreCase);

        await DeleteChunksCache(); // now people can stop complain big .data folder sizes

        await InitOodle();
        await InitZlib();

        Provider.VfsRegistered += (sender, num) =>
        {
            if (sender is not IAesVfsReader reader)
                return;

            Log.Information("Loaded {name} ({archives} archives)", reader.Name, num);
        };

        await LoadEpicManifest();
        await MountArchives();
        await LoadMappings();
        await LoadKeys();

        Provider.PostMount();

        LoadEntries(
            r => r.EncryptionKeyGuid == Globals.ZERO_GUID ||
            !Provider.RequiredKeys.Contains(r.EncryptionKeyGuid)
        );
    }

    private async Task DeleteChunksCache()
    {
        var chunkSettings = AppSettings.Default.ChunksSettings;

        if (!chunkSettings.AutoClearEnabled)
            return;

        var chunks = Directories.GetCachedChunks();
        var maxLifetime = DateTime.Now - TimeSpan.FromDays(chunkSettings.ChunkCacheLifetime);

        foreach (var chunk in chunks)
        {
            if (chunk.LastWriteTime >= maxLifetime)
                continue;

            chunk.Delete();
        }
    }
    
    private async Task InitOodle()
    {
        var path = Path.Combine(Directories.Data, OodleHelper.OODLE_DLL_NAME);
        if (!File.Exists(path)) await OodleHelper.DownloadOodleDllAsync(path);
        OodleHelper.Initialize(path);
    }

    private async Task InitZlib()
    {
        var zlibPath = Path.Combine(Directories.Data, ZlibHelper.DLL_NAME);
        if (!File.Exists(zlibPath)) await ZlibHelper.DownloadDllAsync(zlibPath);
        ZlibHelper.Initialize(zlibPath);
    }

    private async Task LoadEpicManifest()
    {
        var auth = AppSettings.Default.EpicAuth;
        ManifestInfo? manifest = await Api.EpicGames.GetManifestAsync(auth);
        if (manifest is null)
        {
            Log.Error("The manifest response was invalid.");
            App.ExitThread(1);
        }

        var start = Stopwatch.StartNew();
        await Manifest.DownloadManifest(manifest!);
        Log.Information("Downloaded manifest for version {version} (Id: {id}) in {seconds}s ({ms}ms)",
            Manifest.GameVersion, Manifest.ManifestId,
            Math.Round(start.Elapsed.TotalSeconds, 2),
            Math.Round(start.Elapsed.TotalMilliseconds));
    }

    private async Task MountArchives()
    {
        Log.Information("Downloading archives for {gameBuild}", Manifest.GameBuild);
        Manifest.LoadManifestArchives();
        await Provider.MountAsync();
    }

    private async Task LoadMappings()
    {
        string? mapping;
        if (AppSettings.Default.UseCustomMappingFile)
        {
            if (!File.Exists(AppSettings.Default.CustomMappingFile))
            {
                Log.Error("Custom mapping file is enabled but no mapping has been found for {mapping}.", AppSettings.Default.CustomMappingFile);
                return;
            }
            mapping = AppSettings.Default.CustomMappingFile;
        }
        else
        {
            mapping = await GetMappings() ?? Directories.GetSavedMappings();
            if (string.IsNullOrEmpty(mapping))
            {
                Log.Warning("Mappings response was invalid and no local mappings have been found. Athena might not work as expected.");
                return;
            }
        }

        Provider.MappingsContainer = new FileUsmapTypeMappingsProvider(mapping);
        Log.Information("Loaded mappings from {mapping}.", mapping);
    }

    private async Task<string?> GetMappings()
    {
        var mapping = await Api.Dilly.GetMappingAsync();
        if (mapping is null) return null;

        var path = Path.Combine(Directories.Mappings, mapping.FileName);
        var file = await Api.DownloadFileAsync(mapping.Url, path, false);
        return file.Exists ? file.FullName : null;
    }

    private async Task LoadKeys()
    {
        var keysReponse = await Api.Dilly.GetAESKeysAsync();
        if (keysReponse is null)
        {
            Log.Warning("AES Keys response was invalid. Trying to load local keys.");
            LoadLocalKeys();
            return;
        }

        AppSettings.Default.LocalKeys = keysReponse;
        AppSettings.SaveSettings();

        LoadKey(Globals.ZERO_GUID, new(keysReponse.MainKey));
        LoadKeysList(keysReponse.DynamicKeys);
        Log.Information("Loaded {total} Dynamic Keys.", keysReponse.DynamicKeys.Count);

        TestMainKey(keysReponse.MainKey); // if this fails, athena is cooked!
    }

    private void LoadLocalKeys()
    {
        if (AppSettings.Default.LocalKeys is null)
        {
            Log.Warning("No local keys found. Athena might not work as expected.");
            return;
        }

        LoadKey(Globals.ZERO_GUID, new FAesKey(AppSettings.Default.LocalKeys.MainKey));
        LoadKeysList(AppSettings.Default.LocalKeys.DynamicKeys);
        Log.Information("Loaded {total} local Dynamic Keys.", AppSettings.Default.LocalKeys.DynamicKeys.Count);
    }

    private void TestMainKey(string key)
    {
        var vf = Provider.MountedVfs.First(r => r.Name.Equals("pakchunk0-WindowsClient.pak"));
        if (!vf.TestAesKey(new FAesKey(key)))
        {
            Log.Warning("Main key is invalid. Athena might not work as expected.");
        }
    }

    public void LoadKeysList(List<DynamicKey> dynamicKeys)
    {
        dynamicKeys.ForEach(k => LoadKey(new FGuid(k.Guid), new FAesKey(k.Key)));
    }

    public void LoadKey(FGuid guid, FAesKey key)
    {
        Provider.SubmitKey(guid, key);
    }

    public void LoadEntries(Func<IAesVfsReader, bool> filter, HashSet<string>? customItemsOrOldPaths = null, bool bNew = false, bool bIsCustom = false)
    {
        int loadedEntries = 0;
        var start = Stopwatch.StartNew();

        // we load only required VFs
        var readers = Provider.MountedVfs.Where(filter);
        foreach (var reader in readers)
        {
            foreach (var (path, file) in reader.Files)
            {
                // entry.IsUePackage so we load only .uasset files
                if (file is not VfsEntry entry || !entry.IsUePackage || entry.NameWithoutExtension.EndsWith(".o"))
                    continue;

                // filter only new files (used when customItemsOrOldPaths contains backup files aka old files)
                if (bNew && (customItemsOrOldPaths?.Contains(path) ?? false))
                    continue;

                // this filter is used when the user wants only selected items (by ID)
                if (bIsCustom && !customItemsOrOldPaths!.Contains(entry.NameWithoutExtension))
                    continue;

                (bNew || bIsCustom ? NewEntries : AllEntries).Add(entry);
                loadedEntries++;
            }
        }

        if (loadedEntries == 0)
        {
            Log.Warning("No VFs have been loaded.");
            return;
        }

        Log.Information("Loaded {0} VFs in {1}s ({2}ms)", loadedEntries,
            Math.Round(start.Elapsed.TotalSeconds, 2),
            Math.Round(start.Elapsed.TotalMilliseconds));
    }
}