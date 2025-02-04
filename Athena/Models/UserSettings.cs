using Newtonsoft.Json;
using Athena.Models.API.Fortnite;

namespace Athena.Models;

public class ProfileSettings
{
    public string ProfileId { get; set; } = "AthenaProfile";
    public int BattlepassLevel { get; set; } = 999999;
}

public class CatalogSettings
{
    public int BundlePrice { get; set; } = -999999;
    public int ItemPrice { get; set; } = -999999;
}

public class UserSettings
{
    public static UserSettings Current = null!;

    // output settings
    public string ProfilesPath { get; set; } = ""; // TBD
    public string CatalogPath { get; set; } = ""; // TBD

    // models settings
    public ProfileSettings Profiles { get; set; } = new();
    public CatalogSettings Catalog { get; set; } = new();

    // app settings
    public EpicAuth EpicAuth { get; set; } = null!;
    public bool bUseDiscordRPC { get; set; } = true;
    public bool bShowChangelog { get; set; } = false;

    // @TODO: change settings path on final release
    public static void LoadSettings()
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "debug-settings.json"))) 
        {
            Current = JsonConvert.DeserializeObject<UserSettings>(Path.Combine(Environment.CurrentDirectory, "debug-settings.json"))!;
        }
        else
        {
            CreateSettings();
        }
    }

    public static void SaveSettings()
    {
        var data = JsonConvert.SerializeObject(
            Current, Formatting.Indented, Globals.JsonSettings
        );

        File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "debug-settings.json"), data);
    }
    
    public static void CreateSettings()
    {
        Current = new UserSettings();

        askProfileId: { }
        askProfilePath: { }
        askCatalogPath: { }

        // @TODO: add discord shit

        SaveSettings(); // save settings for prevent unsaving on application exit
    }
}
