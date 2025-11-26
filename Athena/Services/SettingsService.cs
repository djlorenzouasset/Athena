using Spectre.Console;
using Athena.Utils;
using Athena.Extensions;
using Athena.Models.Settings;

namespace Athena.Services;

public class SettingsService
{
    public UserSettings Default = null!;

    private readonly string _file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
        "settingsV2.json"
    );

    public void LoadSettings()
    {
        if (!File.Exists(_file))
        {
            CreateSettings();
            return;
        }

        var raw = File.ReadAllText(_file);
        Default = CustomJsonSerializer.Deserialize<UserSettings>(raw)!;
    }

    public void SaveSettings()
    {
        var json = CustomJsonSerializer.Serialize(Default!);
        File.WriteAllText(_file, json);
    }

    public void ValidateSettings()
    {
        bool save = false;

        if (string.IsNullOrEmpty(Default.ProfilesSettings.OutputPath) || !Directory.Exists(Default.ProfilesSettings.OutputPath))
        {
            save = true;
            Default.ProfilesSettings.OutputPath = Directories.Output;
            Log.Warning("Profiles output path is invalid. It has now been set to the default one: {path}.", Default.ProfilesSettings.OutputPath);
        }
        if (string.IsNullOrEmpty(Default.CatalogSettings.OutputPath) || !Directory.Exists(Default.CatalogSettings.OutputPath))
        {
            save = true;
            Default.CatalogSettings.OutputPath = Directories.Output;
            Log.Warning("ItemShop Catalog output path is invalid. It has now been set to the default one: {path}.", Default.CatalogSettings.OutputPath);
        }
        if (Default.UseCustomMappingFile && string.IsNullOrEmpty(Default.CustomMappingFile))
        {
            Default.UseCustomMappingFile = false; // temporarily disable custom mapping for the current run
            Log.Warning("Custom mapping file is enabled but the mapping path is invalid. It has been set to 'false' for this run.");
#if !DEBUG
            MessageService.Show("Invalid Settings",
                "Custom mapping file is enabled but no valid mapping path is set.\n\n" +
                "In order to fix this issue, set the property 'bUseCustomMappingFile' to 'false' in the settings file or set a valid mapping path.",
                MessageService.MB_OK | MessageService.MB_ICONWARNING
            );
#endif
        }

        if (save)
        {
            SaveSettings();
        }
    }

    public async Task<bool> CreateAuth()
    {
        var auth = await Api.EpicGames.CreateAuthAsync();
        if (auth is null)
        {
            Log.Error("Failed to create Epic Games Auth code.");
            return false;
        }

        Default!.EpicAuth = auth;
        Log.Information("Successfully created Epic Games auth. Expiration {expiration}", auth.ExpiresAt);
        SaveSettings();
        return true;
    }

    private void CreateSettings()
    {
        Default ??= new UserSettings();

        var profileName = App.Ask(
            "What [62]name[/] would you use for the [62]Profile[/]? (type [236]d[/] for use the default one):"
        );
        if (profileName != "d")
        {
            Default.ProfilesSettings.ProfileId = profileName;
        }

        AskPath(EModelType.ProfileAthena);
        AskPath(EModelType.ItemShopCatalog);
        Default.UseDiscordRPC = AnsiConsole.Confirm("Do you want to use the [62]Discord Presence[/]?");

        SaveSettings(); // save settings for prevent unsaving on application exit
    }

    private void AskPath(EModelType forModel)
    {
        bool errored = false;

        onInvalidPath:
        {
            var path = App.Ask(
                $"Insert the [62]path[/] to use for save the [62]{forModel.DisplayName()}[/] (type [236]d[/] for use the default one):",
                errored ? 2 : 1
            );

            if (path != "d")
            {
                path = path.Replace("\"", "").Trim();
                if (!Directory.Exists(path))
                {
                    errored = true;
                    Log.Error("The directory inserted doesn't exists.");
                    goto onInvalidPath;
                }

                switch (forModel)
                {
                    case EModelType.ProfileAthena:
                        Default!.ProfilesSettings.OutputPath = path;
                        break;
                    case EModelType.ItemShopCatalog:
                        Default!.CatalogSettings.OutputPath = path;
                        break;
                }

                AthenaUtils.ClearConsoleLines(1);
                Log.Information("{type} path successfully set to {dir}.", forModel.DisplayName(), path);
            }
        }
    }
}