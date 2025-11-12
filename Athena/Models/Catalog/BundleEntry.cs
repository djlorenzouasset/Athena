namespace Athena.Models.Catalog;

public class BundleCatalogEntry : ICatalogEntry
{
    public string DevName = "[VIRTUAL]1 x Funny Thing for -1 MtxCurrency";
    public string OfferId = string.Empty;
    public List<object> FulfillmentIds = [];
    public int DailyLimit = -1;
    public int WeeklyLimit = -1;
    public int MonthlyLimit = -1;
    public List<string> Categories = [];
    public List<Price> Prices = [];
    public DynamicBundleInfo DynamicBundleInfo = new();
    public Meta Meta = new();
    public string MatchFilter = string.Empty;
    public double FilterWeight = 0.0;
    public List<string> AppStoreId = [];
    public List<object> Requirements = [];
    public string OfferType = "DynamicBundle";
    public GiftInfo GiftInfo = new();
    public bool Refundable = true;
    public List<MetaInfo> MetaInfo = [];
    public string DisplayAssetPath = string.Empty;
    public List<ItemGrant> ItemGrants = 
    [
        new() { TemplateId = "AthenaCharacter:CID_349_Athena_Commando_M_Banana" }
    ];
    public List<object> AdditionalGrants = [];
    public int SortPriority = -1;
    public int CatalogGroupPriority = 0;
}

public class DynamicBundleInfo
{
    public int DiscountedBasePrice = Settings.Current.Catalog.BundlePrice;
    public int RegularBasePrice = Settings.Current.Catalog.BundlePrice;
    public int FloorPrice = Settings.Current.Catalog.BundlePrice;
    public string CurrencyType = "MtxCurrency";
    public string CurrencySubType = string.Empty;
    public string DisplayType = "AmountOff";
    public List<BundleRequirement> BundleItems = [new() /* we add an empty one (??) */];
}

public class BundleRequirement
{
    public bool bCanOwnMultiple = true;
    public int RegularPrice = Settings.Current.Catalog.ItemPrice;
    public int DiscountedPrice = Settings.Current.Catalog.ItemPrice;
    public int AlreadyOwnedPriceReduction = Settings.Current.Catalog.ItemPrice;
    public Item Item = new()
    { 
        TemplateId = "AthenaCharacter:CID_349_Athena_Commando_M_Banana", 
        Quantity = 1 
    };
}