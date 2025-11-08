using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Athena.Services;
using CUE4Parse.UE4.Versions;
using Athena.Models.API.Responses;

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
    public string ShopName { get; set; } = "shopv3.json"; // current shop name used by neonite
    public string OutputPath { get; set; } = Directories.Output.FullName;
    public List<MetaInfo> CardGradients { get; set; } = [
        new MetaInfo { Key = "color1", Value = "#424242" },
        new MetaInfo { Key = "color2", Value = "#212121" },
        new MetaInfo { Key = "color3", Value = "#121212" },
    ];
}

public class UserSettings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public EGame EngineVersion { get; set; } = EGame.GAME_UE5_7;

    public ProfileSettings Profiles { get; set; } = new();
    public CatalogSettings Catalog { get; set; } = new();
    public EpicAuth EpicAuth { get; set; } = null!;
    public bool bUseDiscordRPC { get; set; } = true;
    public bool bShowChangelog { get; set; } = false;
    public bool bUseCustomMappingFile { get; set; } = false;
    public string? CustomMappingFile { get; set; } = null;
    public DateTime? LastDonationPopup { get; set; } = null;
}
