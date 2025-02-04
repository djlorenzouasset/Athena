using Newtonsoft.Json;
using Athena.Models.API.Fortnite;

namespace Athena.Models;

public class UserSettings
{
    public static UserSettings Current = null!;

    public string AthenaProfileId { get; set; } = "AthenaProfile";
    public string ProfilesPath { get; set; } = ""; // TBD
    public string CatalogPath { get; set; } = ""; // TBD

    public bool bUseDiscordRPC { get; set; } = true;
    public bool bShowChangelog { get; set; } = false;

    public EpicAuth EpicAuth { get; set; } = null!;

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
