using Athena.Extensions;
using Athena.Models.App;
using Athena.Utils;
using Newtonsoft.Json;
using Spectre.Console;

namespace Athena.Services;

public class SettingsService
{
    public static UserSettings Current = null!;

    private static readonly DirectoryInfo _directory = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Athena"));
    private static readonly FileInfo _filePath = new(Path.Combine(_directory.FullName, "settingsv2.json"));

    public static void LoadSettings()
    {
        if (!_filePath.Exists)
        {
            CreateSettings();
            return;
        }

        var raw = File.ReadAllText(_filePath.FullName);
        Current = JsonConvert.DeserializeObject<UserSettings>(raw)!;
    }

    public static void CreateSettings()
    {
        Current = new UserSettings();

        var profileName = AthenaUtils.Ask(
            "What [62]name[/] would you use for the [62]Profile[/]? (type [236]d[/] for use the default one):"
        );
        if (profileName != "d")
        {
            Current.Profiles.ProfileId = profileName;
        }

        AskPath(EModelType.ProfileAthena);
        AskPath(EModelType.ItemShopCatalog);
        Current.bUseDiscordRPC = AnsiConsole.Confirm("Do you want to use the Discord Presence?");

        SaveSettings(); // save settings for prevent unsaving on application exit
    }

    public static void SaveSettings()
    {
        var settings = JsonConvert.SerializeObject(
            Current, Formatting.Indented, Globals.JsonSettings
        );

        File.WriteAllText(_filePath.FullName, settings);
    }

    public static void ValidateSettings()
    {
        bool save = false;

        if (string.IsNullOrEmpty(Current.Profiles.OutputPath) ||
            !Directory.Exists(Current.Profiles.OutputPath))
        {
            save = true;
            Log.Warning("Profiles output path is invalid. It has now been set to the default one.");
            Current.Catalog.OutputPath = Directories.Output.FullName;
        }

        if (string.IsNullOrEmpty(Current.Catalog.OutputPath) ||
            !Directory.Exists(Current.Catalog.OutputPath))
        {
            save = true;
            Log.Warning("Item Shop Catalog output path is invalid. It has now been set to the default one.");
            Current.Catalog.OutputPath = Directories.Output.FullName;
        }

        if (Current.bUseCustomMappingFile &&
            (string.IsNullOrEmpty(Current.CustomMappingFile) ||
            !File.Exists(Current.CustomMappingFile)))
        {
            Log.Warning("Custom mapping file is enabled but the mapping path is invalid.");
            Message.Show("Warning: Invalid Settings", "Custom mapping file is enabled but no valid mapping path is set. " +
                "Athena might not work as expected.\n\nIn order to fix this issue, set the property " +
                "'bUseCustomMappingFile' to 'false' in the settings file or set a valid mapping path.",
                Message.MB_OK | Message.MB_ICONWARNING
            );
        }

        if (save)
        {
            SaveSettings();
        }
    }

    public static async Task<bool> CreateAuth()
    {
        var auth = await APIEndpoints.Instance.EpicGames.CreateAuthAsync();
        if (auth is null)
        {
            Log.Error("Failed to create Epic Games Auth code.");
            return false;
        }

        Current.EpicAuth = auth;
        Log.Information("Successfully created Epic Games auth. Expiration {expire_at}", auth.ExpiresAt);
        SaveSettings();
        return true;
    }

    private static void AskPath(EModelType forModel)
    {
        bool errored = false;

        onInvalidPath:
        {
            var path = AthenaUtils.Ask(
                $"Insert the [62]path[/] to use for save the [62]{forModel.GetDescription()}[/] (type [236]d[/] for use the default one):",
                errored ? 2 : 1
            );

            if (path != "d")
            {
                path = path.Replace("\"", "").Trim();
                if (!Directory.Exists(path))
                {
                    errored = true;
                    Log.Error("The directory you inserted doesn't exists.");
                    goto onInvalidPath;
                }

                switch (forModel)
                {
                    case EModelType.ProfileAthena:
                        Current.Profiles.OutputPath = path;
                        break;
                    case EModelType.ItemShopCatalog:
                        Current.Catalog.OutputPath = path;
                        break;
                }

                AthenaUtils.ClearConsoleLines(1);
                Log.Information("{type} path successfully set to {dir}.", forModel, path);
            }
        }
    }
}
