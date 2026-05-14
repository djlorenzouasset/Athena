namespace Athena.Models.Catalog;

public class RMTCatalogEntry : ICatalogEntry
{
    public string DevName = "TBD (RMT)";
    public string OfferId = string.Empty;
    public string OfferType = "StaticPrice";
    public List<Price> Prices = [];
    public List<string> Categories = [];
    public int DailyLimit = -1;
    public int WeeklyLimit = -1;
    public int MonthlyLimit = -1;
    public List<object> FulfillmentIds = [];
    public string RefoundType = "Nonrefundable";
    public List<string> AppStoreId = [];
    public List<Requirement> Requirements = [];
    public List<MetaInfo> MetaInfo = [];
    public string CatalogGroup = string.Empty;
    public int CatalogGroupPriority = 0;
    public int SortPriority = -1;
    public string Title = string.Empty;
    public string ShortDescription = string.Empty;
    public string Description = string.Empty;
    public string DisplayAssetPath = string.Empty;
    public List<ItemGrant> ItemGrants = [];
}