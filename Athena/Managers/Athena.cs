using RestSharp;
using Serilog.Sinks.SystemConsole.Themes;
using Athena.Rest;

namespace Athena.Managers;

public static class Athena
{
    public static async Task Initialize()
    {
        Console.Title = "Athena: Starting";

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(DirectoryManager.Logs, $"Athena-Log-{DateTime.Now:dd-MM-yyyy}.txt"),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
            .CreateLogger();

        DirectoryManager.CreateFolders();
        DiscordRichPresence.Initialize();
        DiscordRichPresence.Update("Loading Assets");
        
        // endpoints requests
        var mappings = await Endpoints.FNCentral.GetMappingsAsync();
        var aesKeys = await Endpoints.FNCentral.GetAesKeysAsync();

        // use this variable for assing the mapping name
        string mapping;

        if (aesKeys is null)
        {
            Log.Warning("AesKeys response was unsuccessful, the program may not work as expected.");
        }

        if (mappings is null || !mappings.FirstOrDefault().IsValid || mappings.Length == 0)
        {
            if (!DirectoryManager.GetSavedMappings(out string mappingPath))
            {
                Log.Error("Mappings response was invalid and no mappings are saved in .mappings directory.");
                Console.ReadKey();
                Environment.Exit(0);
            }
            mapping = Path.Combine(DirectoryManager.MappingsDir, mappingPath);
            Log.Information("Mappings loaded from {path}", mapping);
        }
        else
        {
            var mappingFile = mappings.FirstOrDefault();
            mapping = Path.Combine(DirectoryManager.MappingsDir, mappingFile.Filename);

            if (!File.Exists(mapping))
            {
                await Endpoints.FNCentral.DownloadMappingsAsync(mappingFile.Url, mappingFile.Filename);
                Log.Information("Mappings downloaded in {path}", mapping);
            }
        }

        // get auth
        await Endpoints.Epic.GetAuthAsync();

        // get manifest
        RestResponse? manifest = await Endpoints.Epic.GetManifestAsync();

        if (manifest is null || string.IsNullOrEmpty(manifest.Content))
        {
            Log.Error("Invalid response from Fortnite Manifest.");
            Console.ReadKey();
            Environment.Exit(0);
        }

        await InitializeDataminer(mapping, manifest);
    }

    private static async Task InitializeDataminer(string mappingFile, RestResponse manifest)
    {
        var dataminer = new Dataminer(mappingFile);
        await dataminer.LoadAllPaks(manifest);
        await dataminer.AskGeneration();
    }
}