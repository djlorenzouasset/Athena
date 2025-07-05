using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Spectre.Console;
using Athena.Services;
using Athena.Models.API.Fortnite;
using CUE4Parse.UE4.Versions;
using Athena.Utils;

namespace Athena.Models.App;

public class ProfileSettings
{
    public string ProfileId { get; set; } = "AthenaProfile";
    public int BattlepassLevel { get; set; } = 999999;
    public string OutputPath { get; set; } = Directories.Output.FullName;
}

public class CatalogSettings
{
    public int BundlePrice { get; set; } = -999999;
    public int ItemPrice { get; set; } = -999999;
    public string OutputPath { get; set; } = Directories.Output.FullName;
}

public class UserSettings
{
    private static readonly string _file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
        "Athena", "settingsv2.json"
    );

    public static UserSettings Current = null!;

    [JsonConverter(typeof(StringEnumConverter))]
    public EGame EngineVersion { get; set; } = EGame.GAME_UE5_LATEST;

    public ProfileSettings Profiles { get; set; } = new();
    public CatalogSettings Catalog { get; set; } = new();
    public EpicAuth EpicAuth { get; set; } = null!;
    public bool bUseDiscordRPC { get; set; } = true;
    public bool bShowChangelog { get; set; } = false;

    public static void LoadSettings()
    {
        if (!File.Exists(_file))
        {
            CreateSettings();
            return;
        }

        var raw = File.ReadAllText(_file);
        Current = JsonConvert.DeserializeObject<UserSettings>(raw)!;
    }

    public static void CreateSettings()
    {
        Current = new UserSettings();

        var profileName = FUtils.Ask(
            "What [62]name[/] would you use for the [62]Profile[/]? (type [236]d[/] for use the default one):"
        );
        if (profileName != "d")
        {
            Current.Profiles.ProfileId = profileName;
        }

        Current.AskPath(EModelType.ProfileAthena);
        Current.AskPath(EModelType.ItemShopCatalog);
        Current.bUseDiscordRPC = AnsiConsole.Confirm("Do you want to use the Discord Presence?");

        SaveSettings(); // save settings for prevent unsaving on application exit
    }

    public static void SaveSettings()
    {
        var settings = JsonConvert.SerializeObject(
            Current, Formatting.Indented, Globals.JsonSettings
        );

        File.WriteAllText(_file, settings);
    }

    public static async Task<bool> CreateAuth()
    {
        var auth = await APEndpoints.Instance.EpicGames.CreateAuthAsync();
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

    private void AskPath(EModelType forModel)
    {
        var modelName = 
            forModel is EModelType.ProfileAthena
            ? "Profiles" : "ItemShop";

        onInvalidPath:
        {
            var path = FUtils.Ask(
                $"Insert the [62]path[/] to use for save the [62]{modelName}[/] (type [236]d[/] for use the default one):"
            );

            if (path != "d")
            {
                if (!Directory.Exists(path))
                {
                    Log.Error("The directory you inserted doesn't exists.");
                    goto onInvalidPath;
                }

                switch (forModel)
                {
                    case EModelType.ProfileAthena:
                        Profiles.OutputPath = path;
                        break;
                    case EModelType.ItemShopCatalog:
                        Catalog.OutputPath = path;
                        break;
                }

                Log.Information("{type} path successfully set to {dir}.", modelName, path);
            }
        }
    }
}
