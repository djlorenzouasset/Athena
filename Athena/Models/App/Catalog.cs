namespace Athena.Models.App;

public class ShopModel
{
    public int RefreshIntervalHrs { get; set; } = 1;
    public int DailyPurchaseHrs { get; set; } = 24;
    public DateTime Expiration { get; set; } = new(9999, 12, 31, 23, 59, 59);
    public List<Storefront> Storefronts { get; set; } = [];
}

public class Storefront
{
    public string Name { get; set; }
    public List<ICatalogEntry> CatalogEntries { get; set; } = [];
}

public interface ICatalogEntry
{
    string DevName { get; set; }
    string OfferId { get; set; }
    List<object> FulfillmentIds { get; set; }
    int DailyLimit { get; set; }
    int WeeklyLimit { get; set; }
    int MonthlyLimit { get; set; }
    List<string> Categories { get; set; }
    List<Price> Prices { get; set; }
    Meta Meta { get; set; }
    string MatchFilter { get; set; }
    double FilterWeight { get; set; }
    List<string> AppStoreId { get; set; }
    string OfferType { get; set; }
    GiftInfo GiftInfo { get; set; }
    bool Refundable { get; set; }
    List<MetaInfo> MetaInfo { get; set; }
    string DisplayAssetPath { get; set; }
    List<ItemGrant> ItemGrants { get; set; }
    List<object> AdditionalGrants { get; set; }
    int SortPriority { get; set; }
    int CatalogGroupPriority { get; set; }
}

public class BundleCatalogEntry : ICatalogEntry
{
    public string DevName { get; set; } = "[VIRTUAL]1 x Funny Thing for -9 MtxCurrency";
    public string OfferId { get; set; } = string.Empty;
    public List<object> FulfillmentIds { get; set; } = [];
    public int DailyLimit { get; set; } = -1;
    public int WeeklyLimit { get; set; } = -1;
    public int MonthlyLimit { get; set; } = -1;
    public List<string> Categories { get; set; } = [];
    public List<Price> Prices { get; set; } = [];
    public DynamicBundleInfo DynamicBundleInfo { get; set; } = new();
    public Meta Meta { get; set; } = new();
    public string MatchFilter { get; set; } = string.Empty;
    public double FilterWeight { get; set; } = 0.0;
    public List<string> AppStoreId { get; set; } = [];
    public List<object> Requirements { get; set; } = [];
    public string OfferType { get; set; } = "DynamicBundle";
    public GiftInfo GiftInfo { get; set; } = new();
    public bool Refundable { get; set; } = true;
    public List<MetaInfo> MetaInfo { get; set; } = [];
    public string DisplayAssetPath { get; set; } = string.Empty;
    public List<ItemGrant> ItemGrants { get; set; } = [new() { TemplateId = "AthenaCharacter:CID_349_Athena_Commando_M_Banana" }];
    public List<object> AdditionalGrants { get; set; } = [];
    public int SortPriority { get; set; } = -1;
    public int CatalogGroupPriority { get; set; } = 0;
}

public class CosmeticCatalogEntry : ICatalogEntry
{
    public string DevName { get; set; } = "[VIRTUAL]1 x Funny Thing for -9 MtxCurrency";
    public string OfferId { get; set; } = string.Empty;
    public List<object> FulfillmentIds { get; set; } = [];
    public int DailyLimit { get; set; } = -1;
    public int WeeklyLimit { get; set; } = -1;
    public int MonthlyLimit { get; set; } = -1;
    public List<string> Categories { get; set; } = [];
    public List<Price> Prices { get; set; } = [new()];
    public Meta Meta { get; set; } = new();
    public string MatchFilter { get; set; } = string.Empty;
    public double FilterWeight { get; set; } = 0.0;
    public List<string> AppStoreId { get; set; } = [];
    public List<Requirement> Requirements { get; set; } = [];
    public string OfferType { get; set; } = "StaticPrice";
    public GiftInfo GiftInfo { get; set; } = new();
    public bool Refundable { get; set; } = true;
    public List<MetaInfo> MetaInfo { get; set; } = [];
    public string DisplayAssetPath { get; set; } = string.Empty;
    public List<ItemGrant> ItemGrants { get; set; } = [];
    public List<object> AdditionalGrants { get; set; } = [];
    public int SortPriority { get; set; } = -2;
    public int CatalogGroupPriority { get; set; } = 0;
}

public class Price
{
    public string CurrencyType = "MtxCurrency";
    public string CurrencySubType = string.Empty;
    public int RegularPrice = UserSettings.Current.Catalog.ItemPrice;
    public int DynamicRegularPrice = UserSettings.Current.Catalog.ItemPrice;
    public int FinalPrice = UserSettings.Current.Catalog.ItemPrice;
    public DateTime SaleExpiration = new(9999, 12, 31, 23, 59, 59);
    public int BasePrice = UserSettings.Current.Catalog.ItemPrice;
}

public class DynamicBundleInfo
{
    public int DiscountedBasePrice = UserSettings.Current.Catalog.BundlePrice;
    public int RegularBasePrice = UserSettings.Current.Catalog.BundlePrice;
    public int FloorPrice = UserSettings.Current.Catalog.BundlePrice;
    public string CurrencyType = "MtxCurrency";
    public string CurrencySubType = string.Empty;
    public string DisplayType  = "AmountOff";
    public List<BundleRequirement> BundleItems = [new()];
}

public class Meta
{
    public string NewDisplayAssetPath { get; set; }
    public string SectionId { get; set; }
    public string LayoutId { get; set; }
    public string TileSize { get; set; }

    public string AnalyticOfferGroupId = "ISTHISNEEDED??";
    public string FirstSeen = "31/12/9999";
    public string Color1 = "#424242";
    public string Color2 = "#212121";
    public string Color3 = "#121212";
}

public class BundleRequirement
{
    public bool bCanOwnMultiple = true;
    public int RegularPrice = UserSettings.Current.Catalog.ItemPrice;
    public int DiscountedPrice = UserSettings.Current.Catalog.ItemPrice;
    public int AlreadyOwnedPriceReduction = UserSettings.Current.Catalog.ItemPrice;
    public Item Item = new() { TemplateId = "AthenaCharacter:CID_349_Athena_Commando_M_Banana", Quantity = 1 };
}

public class Requirement
{
    public string RequirementType = "DenyOnItemOwnership";
    public string RequiredId { get; set; }
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
    public string Key { get; set; }
    public string Value { get; set; }
}

public class ItemGrant
{
    public string TemplateId { get; set; }
    public int Quantity = 10;
}

public class Item
{
    public string TemplateId { get; set; }
    public int Quantity { get; set; }
}
