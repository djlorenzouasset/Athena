using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CUE4Parse.UE4.Versions;
using Athena.Models.Catalog;
using Athena.Models.API.Responses;

namespace Athena.Models;

public class ProfileSettings
{
    public string ProfileId { get; set; } = "AthenaProfile";
    public int BattlepassLevel { get; set; } = -1;
    public string OutputPath { get; set; } = Directories.Output;
}

public class CatalogSettings
{
    public int BundlePrice { get; set; } = -1;
    public int ItemPrice { get; set; } = -1;
    public string ShopName { get; set; } = "shopv3.json"; // current shop name used by neonite
    public string OutputPath { get; set; } = Directories.Output;

    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    public List<MetaInfo> CardGradients { get; set; } = [
        new MetaInfo { Key = "color1", Value = "#424242" },
        new MetaInfo { Key = "color2", Value = "#212121" },
        new MetaInfo { Key = "color3", Value = "#121212" },
    ];
}

public class UserSettings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public EGame EngineVersion { get; set; } = EGame.GAME_UE5_8;
    public ProfileSettings Profiles { get; set; } = new();
    public CatalogSettings Catalog { get; set; } = new();
    public EpicAuth EpicAuth { get; set; } = null!;
    public bool UseDiscordRPC { get; set; } = true;
    public bool ShowChangelog { get; set; } = false;
    public bool UseCustomMappingFile { get; set; } = false;
    public string? CustomMappingFile { get; set; } = null;
    public DateTime LastDonationPopup { get; set; } = DateTime.MinValue;
    public AESKeys? LocalKeys { get; set; } = null;
}
