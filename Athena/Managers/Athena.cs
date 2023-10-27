using RestSharp;
using Serilog.Sinks.SystemConsole.Themes;
using Athena.Rest;
using Athena.Models;
using Athena.Services;

namespace Athena.Managers;

public static class Athena
{
    private static string mapping { get; set; } = string.Empty;
    private static RestResponse manifest { get; set; } = new();

    public static async Task Initialize()
    {
        Console.Title = "Athena: Starting";

        // configure logger for the application
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(DirectoryManager.Logs, $"Athena-Log-{DateTime.Now:dd-MM-yyyy}.txt"))
            .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
            .CreateLogger();

        DirectoryManager.CreateFolders(); // this function creates the needed folders
        Config.LoadSettings(); // create the settings file or load the saved one
        Console.Clear(); // clear the console after input

        // discord rich presence 
        DiscordRichPresence.Initialize();

        await CheckAuth();
        await TryGetAesKeys();
        if (!await TryGetMappings()) return;
        if (!await TryGetManifest()) return;
        await InitializeDataminer();
    }

    private static async Task TryGetAesKeys()
    {
        var aesKeys = await Endpoints.FNCentral.GetAesKeysAsync();
        if (aesKeys is null)
        {
            Log.Warning("AesKeys response was unsuccessful, the program may not work as expected.");
        }
    }

    private static async Task<bool> TryGetMappings()
    {
        var mappings = await Endpoints.FNCentral.GetMappingsAsync();
        if (mappings is null || mappings.Length == 0)
        {
            if (!DirectoryManager.GetSavedMappings(out string mappingPath))
            {
                Log.Error("Mappings response was invalid and no mappings are saved in .mappings directory.");
                return false;
            }

            mapping = Path.Combine(DirectoryManager.MappingsDir, mappingPath);
            Log.Information("Mappings loaded from {path}", mapping);
        }
        else
        {
            var mappingFile = mappings.First();
            mapping = Path.Combine(DirectoryManager.MappingsDir, mappingFile.Filename);
            if (!File.Exists(mapping))
            {
                await Endpoints.FNCentral.DownloadMappingsAsync(mappingFile.Url, mappingFile.Filename);
                Log.Information("Mappings downloaded in {path}", mapping);
            }
        }

        return true;
    }

    private static async Task<bool> TryGetManifest()
    {
        RestResponse? _manifest = await Endpoints.Epic.GetManifestAsync();
        if (_manifest is null || string.IsNullOrEmpty(_manifest.Content))
        {
            Log.Error("Invalid response from Fortnite Manifest.");
            return false;
        }

        manifest = _manifest;
        return true;
    }

    private static async Task CheckAuth()
    {
        if (!await Endpoints.Epic.IsAuthValid())
        {
            var auth = await Endpoints.Epic.CreateAuthAsync();
            Config.config.accessToken = auth.access_token;
            Config.Save();
        }
    }

    private static async Task InitializeDataminer()
    {
        var dataminer = new Dataminer(mapping);
        await dataminer.LoadAllPaksAsync(manifest);
        await dataminer.ShowMenu();
    }
}