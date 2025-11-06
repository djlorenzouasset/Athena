using System.Diagnostics;
using EpicManifestParser.Api;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.VirtualFileSystem;
using CUE4Parse.Compression;
using CUE4Parse.FileProvider;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.MappingsProvider;
using Athena.Utils;
using Athena.Services;
using Athena.Models.App;
using Athena.Models.API.Responses;

namespace Athena.Core;

public class Dataminer
{
    public static readonly Dataminer Instance = new();

    public AESKeys? AESKeys = null;

    public ManifestDownloader Manifest = null!;
    public StreamedFileProvider Provider = null!;

    public readonly List<VfsEntry> AllEntries = [];
    public readonly List<VfsEntry> NewEntries = [];

    public async Task Initialize()
    {
        Manifest = new();
        Provider = new("", new(UserSettings.Current.EngineVersion), StringComparer.OrdinalIgnoreCase);

        await InitOodle(); /* lib required by CUE4Parse */
        await InitZlib(); /* lib required by EpicManifestParser */

        Provider.VfsRegistered += (sender, num) =>
        {
            if (sender is not IAesVfsReader reader)
                return;

            Log.Information("Loaded {0} ({1} archives)", reader.Name, num);
        };

        await LoadEpicManifest();
        await MountArchives();
        await LoadMappings(); // this may break during updates (like exporting shit)
        await LoadAESKeys();

        LoadEntries(
            r => r.EncryptionKeyGuid.Equals(Globals.ZERO_GUID) ||
            (AESKeys?.GuidsList.Contains(r.EncryptionKeyGuid) ?? false)
        );
    }

    private async Task InitOodle()
    {
        var path = Path.Combine(Directories.Data.FullName, OodleHelper.OODLE_DLL_NAME);
        if (!File.Exists(path)) await OodleHelper.DownloadOodleDllAsync(path);
        OodleHelper.Initialize(path);
    }

    private async Task InitZlib()
    {
        var zlibPath = Path.Combine(Directories.Data.FullName, ZlibHelper.DLL_NAME);
        if (!File.Exists(zlibPath)) await ZlibHelper.DownloadDllAsync(zlibPath);
        ZlibHelper.Initialize(zlibPath);
    }

    private async Task LoadEpicManifest()
    {
        var auth = UserSettings.Current.EpicAuth;
        ManifestInfo? manifest = await APIEndpoints.Instance.EpicGames.GetManifestAsync(auth);
        if (manifest is null)
        {
            Log.Error("The manifest response was invalid.");
            FUtils.ExitThread(1);
        }

        var start = Stopwatch.StartNew();
        await Manifest.DownloadManifest(manifest!);
        Log.Information("Downloaded manifest for version {0} (Id: {1}) in {2}s ({3}ms)", 
            Manifest.GameVersion, Manifest.ManifestId, 
            Math.Round(start.Elapsed.TotalSeconds, 2), 
            Math.Round(start.Elapsed.TotalMilliseconds));
    }

    private async Task MountArchives()
    {
        Log.Information("Downloading archives for {0}", Manifest.GameBuild);
        Manifest.LoadManifestArchives();
        await Provider.MountAsync();
    }

    private async Task LoadMappings()
    {
        string mapping;
        if (UserSettings.Current.bUseCustomMappingFile)
        {
            if (string.IsNullOrEmpty(UserSettings.Current.CustomMappingFile))
            {
                Log.Warning("Custom mapping file is enabled but no mapping path is set.");
                return;
            }
            else if (!File.Exists(UserSettings.Current.CustomMappingFile))
            {
                Log.Warning("Custom mapping file is enabled but mapping {0} doesn't exist.", UserSettings.Current.CustomMappingFile);
                return;
            }

            mapping = UserSettings.Current.CustomMappingFile;
        }
        else
        {
            var mappings = await APIEndpoints.Instance.Athena
                .GetMappingAsync() ?? Directories.GetSavedMappings();

            if (mappings is null)
            {
                Log.Warning("Mappings API response was invalid and no local mappings have been found.");
                return;
            }

            mapping = mappings;
        }

        Provider.MappingsContainer = new FileUsmapTypeMappingsProvider(mapping);
        Log.Information("Usmap file loaded from {0}", mapping);
    }

    private async Task LoadAESKeys()
    {
        AESKeys = await APIEndpoints.Instance.Dilly.GetAESKeysAsync();
        if (AESKeys is null)
        {
            Log.Warning("AES Keys response was invalid.");
            return;
        }

        LoadKey(Globals.ZERO_GUID, new(AESKeys.MainKey));
        LoadKeys(AESKeys.DynamicKeys);
        Log.Information("Loaded {0} Dynamic Keys.", AESKeys.DynamicKeys.Count);
    }

    public void LoadKeys(List<DynamicKey> dynamicKeys)
    {
        dynamicKeys.ForEach(k => LoadKey(k));
    }

    public void LoadKey(DynamicKey dynamicKey)
    {
        LoadKey(new(dynamicKey.Guid), new(dynamicKey.Key));
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
                if (file is not VfsEntry entry || !entry.IsUePackage)
                    continue;

                // filter only new files (used when customItemsOrOldPaths contains backup files aka old files)
                if (bNew && (customItemsOrOldPaths?.Contains(path) ?? false))
                    continue;

                // this filter is used when the user wants only selected items (by ID)
                if (bIsCustom && (!customItemsOrOldPaths!.Contains(entry.NameWithoutExtension.ToLower())))
                    continue;

                (bNew || bIsCustom ? NewEntries : AllEntries).Add(entry);
                loadedEntries++;
            }
        }

        if (loadedEntries == 0)
        {
            Log.Warning("No VFs have been loaded.");
        }
        else
        {
            Log.Information("Loaded {0} VFs in {1}s ({2}ms)", loadedEntries,
                Math.Round(start.Elapsed.TotalSeconds, 2),
                Math.Round(start.Elapsed.TotalMilliseconds));
        }
    }
}
