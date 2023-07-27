public class ShopModel
{
    public int refreshIntervalHrs = 1;
    public int dailyPurchaseHrs = 24;
    public DateTime expiration { get; set; } = new DateTime(9999, 12, 31, 23, 59, 59);
    public List<Storefront> storefronts { get; set; } = new();
}

public class Storefront
{
    public string name { get; set; }
    public List<object> catalogEntries { get; set; } = new(); // use <object> for specify that more type of classes can be added
}

public class BundleCatalogEntry
{
    public string devName = "[VIRTUAL]1 x Funny Thing for 100 MtxCurrency";
    public string offerId = "v2:/ab5b9186b4a65fd4543e697aea9e63a7c40e7e7b22ece8f70101c6800f72a7ad"; // we use this as PH
    public List<object> fulfillmentIds = new();
    public int dailyLimit = -1;
    public int weeklyLimit = -1;
    public int monthlyLimit = -1;
    public List<string> categories = new() { "Panel 03" };
    public List<Price> prices = new();
    public DynamicBundleInfo dynamicBundleInfo = new();
    public BundleMeta meta = new();
    public string matchFilter = string.Empty;
    public double filterWeight = 0.0;
    public List<string> appStoreId = new();
    public List<BundleRequirement> requirements = new() { new() };
    public string offerType = "DynamicBundle";
    public GiftInfo giftInfo = new();
    public bool refundable = true;
    public List<MetaInfo> metaInfo { get; set; } = new();
    public string displayAssetPath { get; set; }
    public List<ItemGrant> itemGrants = new() { new() { templateId = "AthenaCharacter:CID_349_Athena_Commando_M_Banana" } };
    public List<object> additionalGrants = new();
    public int sortPriority = -1;
    public int catalogGroupPriority = 0;
}

public class CosmeticCatalogEntry
{
    public string devName = "[VIRTUAL]1 x Funny Thing for 100 MtxCurrency";
    public string offerId = "v2:/f3d84c3ded015ae12a0c8ae3cc60d771a45df0d90f0af5e1cfbd454fa3083c94"; // same as bundles, we use this as PH
    public List<object> fulfillmentIds = new();
    public int dailyLimit = -1;
    public int weeklyLimit = -1;
    public int monthlyLimit = -1;
    public List<string> categories = new() { "Panel 03" };
    public List<Price> prices = new() { new() };
    public Meta meta { get; set; } = new();
    public string matchFilter = string.Empty;
    public double filterWeight = 0.0;
    public List<string> appStoreId = new();
    public List<Requirement> requirements { get; set; } = new();
    public string offerType = "StaticPrice";
    public GiftInfo giftInfo { get; set; } = new();
    public bool refundable = true;
    public List<MetaInfo> metaInfo { get; set; }
    public string displayAssetPath { get; set; }
    public List<ItemGrant> itemGrants { get; set; } = new();
    public List<object> additionalGrants { get; set; } = new();
    public int sortPriority = -2;
    public int catalogGroupPriority = 0;
}

public class Price
{
    public string currencyType = "MtxCurrency";
    public string currencySubType = string.Empty;
    public int regularPrice = 100; // you can change here the in-game price
    public int dynamicRegularPrice = 100;
    public int finalPrice = 100;
    public DateTime saleExpiration = new DateTime(9999, 12, 31, 23, 59, 59);
    public int basePrice = 100;
}

public class DynamicBundleInfo
{
    public int discountedBasePrice = 100; // here you can change the bundle price
    public int regularBasePrice = 0;
    public int floorPrice = 100;
    public string currencyType = "MtxCurrency";
    public string currencySubType = string.Empty;
    public string displayType = "AmountOff";
    public List<object> bundleItems = new();
}

public class BundleMeta // dont touch nothing here
{
    public string NewDisplayAssetPath { get; set; }
    public string SectionId = "Featured";
    public string TileSize = "DoubleWide";
    public string AnalyticOfferGroupId = "3";
}

public class Meta // dont touch nothing here
{
    public string NewDisplayAssetPath { get; set; }
    public string SectionId = "Daily";
    public string TileSize = "Normal";
    public string AnalyticOfferGroupId = "3";
    public string offertag = string.Empty;
    public string ViolatorTag = string.Empty;
    public string ViolatorIntensity = "High";
    public string FirstSeen = string.Empty;
}

public class BundleRequirement
{
    public bool bCanOwnMultiple = false;
    public int regularPrice = 0;
    public int discountedPrice = 0;
    public int alreadyOwnedPriceReduction = 0;
    public Item item = new() { templateId = "AthenaCharacter:CID_349_Athena_Commando_M_Banana", quantity = 10 }; // yes, 10 peelys
}

public class Requirement // dont touch nothing here
{
    public string requirementType = "DenyOnItemOwnership";
    public string requiredId { get; set; }
    public int minQuantity = 1;
}


///////////////////////////
/// DONT TOUCH NOTHING ///
/////////////////////////

public class GiftInfo
{
    public bool bIsEnabled = true;
    public string forcedGiftBoxTemplateId = string.Empty;
    public List<object> purchaseRequirements = new();
    public List<object> giftRecordIds = new();
}

public class MetaInfo
{
    public string key { get; set; }
    public string value { get; set; }
}

public class ItemGrant
{
    public string templateId { get; set; }
    public int quantity = 1;
}

public class Item
{
    public string templateId { get; set; }
    public int quantity { get; set; }
}