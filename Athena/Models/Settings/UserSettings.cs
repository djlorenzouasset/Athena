using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CUE4Parse.UE4.Versions;
using Athena.Models.API.Responses;

namespace Athena.Models.Settings;

public class UserSettings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public EGame EngineVersion { get; set; } = EGame.GAME_UE5_8;

    // --- enabled features settings --
    public bool UseDiscordRPC { get; set; } = true;
    public bool ShowChangeLog { get; set; } = false;
    public bool UseCustomMappingFile { get; set; } = false;
    public string CustomMappingFile { get; set; } = null!;

    // --- models settings --
    public ProfileSettings ProfilesSettings { get; set; } = new();
    public CatalogSettings CatalogSettings { get; set; } = new();

    // --- application informations --
    public DateTime LastDonationPopup { get; set; } = DateTime.MinValue;
    public EpicAuth EpicAuth { get; set; } = null!;
    public AESKeys LocalKeys { get; set; } = null!;
}