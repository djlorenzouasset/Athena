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
    public List<ItemGrant> ItemGrants = [];
    public List<object> AdditionalGrants = [];
    public int SortPriority = -1;
    public int CatalogGroupPriority = 0;
}

public class DynamicBundleInfo
{
    public int DiscountedBasePrice = 0;
    public int RegularBasePrice = 0;
    public int FloorPrice = 0;
    public string CurrencyType = "MtxCurrency";
    public string CurrencySubType = string.Empty;
    public string DisplayType = "AmountOff";
    public List<BundleRequirement> BundleItems = [];
}

public class BundleRequirement
{
    public bool bCanOwnMultiple = true;
    public int RegularPrice = 0;
    public int DiscountedPrice = 0;
    public int AlreadyOwnedPriceReduction = 0;
    public Item Item = new();
}