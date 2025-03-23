using System.Diagnostics;
using EpicManifestParser.Api;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.Compression;
using CUE4Parse.FileProvider;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.MappingsProvider;
using Athena.Utils;
using Athena.Services;
using Athena.Models.App;
using Athena.Models.API.Fortnite;

namespace Athena.Core;

public class Dataminer
{
    public static readonly Dataminer Instance = new();

    public StreamedFileProvider Provider = null!;

    private ManifestDownloader _manifestManager = null!;

    public async Task Initialize()
    {
        _manifestManager = new();
        Provider = new("", new(EGame.GAME_UE5_LATEST), StringComparer.OrdinalIgnoreCase);

        Log.Information("Initializing required libraries.");
        await InitializeOodle(); /* lib required by CUE4Parse */
        await InitializeZlib(); /* lib required by EpicManifestParser */
        Log.Information("Initializated all libraries.");

        await LoadEpicManifest();
        await MountArchives();
        await LoadMappings(); // this may break during updates (like exporting shit)
        await LoadAESKeys();
    }

    private async Task InitializeOodle()
    {
        var path = Path.Combine(Directories.Data.FullName, OodleHelper.OODLE_DLL_NAME);
        if (File.Exists(OodleHelper.OODLE_DLL_NAME))
        {
            File.Move(OodleHelper.OODLE_DLL_NAME, path, true);
        }
        else if (!File.Exists(path))
        {
            await OodleHelper.DownloadOodleDllAsync(path);
        }

        OodleHelper.Initialize(path);
    }

    private async Task InitializeZlib()
    {
        var zlibPath = Path.Combine(Directories.Data.FullName, ZlibHelper.DLL_NAME);
        if (!File.Exists(zlibPath))
        {
            await ZlibHelper.DownloadDllAsync(zlibPath);
        }

        ZlibHelper.Initialize(zlibPath);
    }

    private async Task LoadEpicManifest()
    {
        var auth = UserSettings.Current.EpicAuth;
        ManifestInfo? manifest = await APIEndpoints.EpicGames.GetManifestAsync(auth);

        if (manifest is null)
        {
            Log.Error("The manifest API response was invalid.");
            FUtils.ExitThread(1);
        }

        var start = Stopwatch.StartNew();
        await _manifestManager.DownloadManifest(manifest!);

        Log.Information("Downloaded manifest {id} for {version}", _manifestManager.ManifestFileName, 
            _manifestManager.GameVersion, start.Elapsed.Seconds, start.ElapsedMilliseconds);
    }

    private async Task MountArchives()
    {
        Log.Information("Mounting archives for {version}", _manifestManager.GameBuild);
        _manifestManager.LoadManifestArchives();
        await Provider.MountAsync();
    }

    private async Task LoadMappings()
    {
        var mappings = await APIEndpoints.FortniteCentral
            .GetMappingsAsync() ?? Directories.GetSavedMappings();

        if (mappings is null)
        {
            Log.Warning("Mappings API response was invalid.");
            Log.Warning("No local mappings have been found.");
            Log.Warning("The program may not work as expected.");
            return;
        }

        Provider.MappingsContainer = new FileUsmapTypeMappingsProvider(mappings);
        Log.Information("Usmap file loaded from {path}", mappings);
    }

    private async Task LoadAESKeys()
    {
        var aesKeys = await APIEndpoints.FortniteCentral.GetAESKeysAsync();
        if (aesKeys is null)
        {
            Log.Warning("AESKeys API response was invalid.");
            return;
        }

        LoadKey(Globals.ZERO_GUID, new(aesKeys.MainKey));
        LoadKeys(aesKeys.DynamicKeys);
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
}