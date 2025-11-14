using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using CUE4Parse.UE4.Versions;
using Athena.Models.API.Responses;

namespace Athena.Models;

public class UserSettings
{
    [JsonConverter(typeof(StringEnumConverter))]
    public EGame EngineVersion { get; set; } = EGame.GAME_UE5_7;

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

public class ProfileSettings
{
    public int BattlePassLevel { get; set; } = -1;
    public string ProfileId { get; set; } = "AthenaProfile";
    public string OutputPath { get; set; } = Directories.Output;
    public List<ItemOptions> CustomItemsOptions { get; set; } = [new()];
}

public class CatalogSettings
{
    public string ShopName { get; set; } = "shopv3.json";
    public string OutputPath { get; set; } = Directories.Output;
    public List<BundleOptions> CustomBundlesOptions { get; set; } = [new()];
}

public class ItemOptions
{
    public string ItemId { get; set; } = string.Empty;
    public int Price { get; set; } = -1;

    public string ViolatorTag { get; set; } = null!;
    public string CustomMaterialBackground { get; set; } = null!;
    public CardColor CardColor { get; set; } = new();
}

public class BundleOptions
{
    public string BundleId { get; set; } = string.Empty;
    public int Price { get; set; } = -1;
    public List<string> BundleCosmetics { get; set; } = [];

    public string ViolatorTag { get; set; } = null!;
    public string CustomMaterialBackground { get; set; } = null!;
    public CardColor CardColor { get; set; } = new();
}

public class CardColor
{
    public string Color1 { get; set; } = null!;
    public string Color2 { get; set; } = null!;
    public string Color3 { get; set; } = null!;
}