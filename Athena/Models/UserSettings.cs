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

    // @TODO: settings logic
    public static void LoadSettings() { }

    public static void SaveSettings() { }

    public static void AskSettings() { }
}
