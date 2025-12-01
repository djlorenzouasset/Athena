namespace Athena.Models.Catalog;

public class CosmeticCatalogEntry : ICatalogEntry
{
    public string DevName = "[VIRTUAL]1 x TBD for -1 MtxCurrency";
    public string OfferId = string.Empty;
    public List<object> FulfillmentIds = [];
    public int DailyLimit = -1;
    public int WeeklyLimit = -1;
    public int MonthlyLimit = -1;
    public List<string> Categories = [];
    public List<Price> Prices = [new()];
    public Meta Meta = new();
    public string MatchFilter = string.Empty;
    public double FilterWeight = 0.0;
    public List<string> AppStoreId = [];
    public List<Requirement> Requirements = [];
    public string OfferType = "StaticPrice";
    public GiftInfo GiftInfo = new();
    public bool Refundable = true;
    public List<MetaInfo> MetaInfo = [];
    public string DisplayAssetPath = string.Empty;
    public List<ItemGrant> ItemGrants = [];
    public List<object> AdditionalGrants = [];
    public int SortPriority = -2;
    public int CatalogGroupPriority = 0;
}

public class Price
{
    public string CurrencyType = "MtxCurrency";
    public string CurrencySubType = string.Empty;
    public int RegularPrice = 0;
    public int DynamicRegularPrice = 0;
    public int FinalPrice = 0;
    public DateTime SaleExpiration = new(9999, 12, 31, 23, 59, 59);
    public int BasePrice = 0;
}