using System.Diagnostics;
using Newtonsoft.Json;
using Athena.Services;
using Athena.Models.API.Fortnite;

namespace Athena.Models.App;

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
    private static readonly string _file = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Athena", "settingsv2.json"
    );

    public static UserSettings Current = null!;

    // output settings
    public string ProfilesPath { get; set; } = Directories.Output.FullName; // TBD
    public string CatalogPath { get; set; } = Directories.Output.FullName; // TBD

    // models settings
    public ProfileSettings Profiles { get; set; } = new();
    public CatalogSettings Catalog { get; set; } = new();

    // app settings
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

        askProfileId: { }
        askProfilePath: { }
        askCatalogPath: { }

        // @TODO: add discord shit

        SaveSettings(); // save settings for prevent unsaving on application exit
    }

    public static void OpenSettings()
    {
        Process.Start(_file);
    }

    public static void SaveSettings()
    {
        var settings = JsonConvert.SerializeObject(
            Current, Formatting.Indented, Globals.JsonSettings
        );

        File.WriteAllText(_file, settings);
    }
}
