using Newtonsoft.Json;

namespace Athena.Models.Catalog;

public class ShopModel
{
    public int RefreshIntervalHrs = 1;
    public int DailyPurchaseHrs = 24;
    public DateTime Expiration = new(9999, 12, 31, 23, 59, 59);
    public List<Storefront> Storefronts = [];
}

public class Storefront
{
    public string Name = "BRWeeklyStorefront"; // default and only one supported
    public List<ICatalogEntry> CatalogEntries = [];
}

public class Meta
{
    public string NewDisplayAssetPath = string.Empty;
    public string SectionId = string.Empty;
    public string LayoutId = string.Empty;
    public string TileSize = string.Empty;
    public string AnalyticOfferGroupId = "AnalyticOfferGroupId";
    public string FirstSeen = "31/12/9999";

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ViolatorTag = null!;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string ViolatorIntensity = null!;
}

public class Requirement
{
    public string RequirementType = "DenyOnItemOwnership";
    public string RequiredId = string.Empty;
    public int MinQuantity = 1;
}

public class GiftInfo
{
    public bool bIsEnabled = true;
    public string ForcedGiftBoxTemplateId = string.Empty;
    public List<object> PurchaseRequirements = [];
    public List<object> GiftRecordIds = [];
}

public class MetaInfo
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public class ItemGrant
{
    public string TemplateId = string.Empty;
    public int Quantity = 1;
}

public class Item
{
    public string TemplateId = string.Empty;
    public int Quantity = 1;
}