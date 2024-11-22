namespace Athena.Models;

/* REWRITE IN V2 */

public class ShopModel
{
    public int refreshIntervalHrs = 1;
    public int dailyPurchaseHrs = 24;
    public DateTime expiration = new DateTime(9999, 12, 31, 23, 59, 59); // when the world will be nothing, the shop will be still not updated
    public List<Storefront> storefronts = [];
}

public class Storefront
{
    public string name { get; set; }
    public List<object> catalogEntries { get; set; } = []; // use <object> for specify that more type of classes can be added
}

public class BundleCatalogEntry
{
    public string devName = "[VIRTUAL]1 x Funny Thing for -9 MtxCurrency";
    public string offerId { get; set; } = string.Empty; // DO NOT CHANGE
    public List<object> fulfillmentIds = [];
    public int dailyLimit = -1;
    public int weeklyLimit = -1;
    public int monthlyLimit = -1;
    public List<string> categories = []; // this was a string array, now is empty
    public List<Price> prices = [];
    public DynamicBundleInfo dynamicBundleInfo = new();
    public Meta meta { get; set; } = new();
    public string matchFilter = string.Empty;
    public double filterWeight = 0.0;
    public List<string> appStoreId = [];
    public List<object> requirements = [];
    public string offerType = "DynamicBundle";
    public GiftInfo giftInfo = new();
    public bool refundable = true;
    public List<MetaInfo> metaInfo { get; set; } = [];
    public string displayAssetPath { get; set; } = string.Empty;
    public List<ItemGrant> itemGrants = [new() { templateId = "AthenaCharacter:CID_349_Athena_Commando_M_Banana" }];
    public List<object> additionalGrants = [];
    public int sortPriority = -1;
    public int catalogGroupPriority = 0;
}

public class CosmeticCatalogEntry
{
    public string devName = "[VIRTUAL]1 x Funny Thing for -9 MtxCurrency";
    public string offerId { get; set; } = string.Empty; // DO NOT CHANGE
    public List<object> fulfillmentIds = [];
    public int dailyLimit = -1;
    public int weeklyLimit = -1;
    public int monthlyLimit = -1;
    public List<string> categories = [];
    public List<Price> prices = [new()];
    public Meta meta { get; set; } = new();
    public string matchFilter = string.Empty;
    public double filterWeight = 0.0;
    public List<string> appStoreId = [];
    public List<Requirement> requirements = [];
    public string offerType = "StaticPrice";
    public GiftInfo giftInfo = new();
    public bool refundable = true;
    public List<MetaInfo> metaInfo { get; set; } = [];
    public string displayAssetPath { get; set; } = string.Empty;
    public List<ItemGrant> itemGrants = [];
    public List<object> additionalGrants = [];
    public int sortPriority = -2;
    public int catalogGroupPriority = 0;
}

public class Price
{
    public string currencyType = "MtxCurrency";
    public string currencySubType = string.Empty;
    public int regularPrice = -999999; // you can change here the in-game price
    public int dynamicRegularPrice = -999999;
    public int finalPrice = -999999;
    public DateTime saleExpiration = new DateTime(9999, 12, 31, 23, 59, 59);
    public int basePrice = -999999;
}

public class DynamicBundleInfo
{
    public int discountedBasePrice = -999999; // here you can change the bundle price
    public int regularBasePrice = -999999;
    public int floorPrice = -999999;
    public string currencyType = "MtxCurrency";
    public string currencySubType = string.Empty;
    public string displayType = "AmountOff";
    public List<BundleRequirement> bundleItems = [];
}

public class Meta // dont touch nothing here
{
    public string NewDisplayAssetPath { get; set; } = string.Empty;
    public string SectionId { get; set; } = string.Empty;
    public string LayoutId { get; set; } = string.Empty; // THIS FUCKING SHIT IS THE BIGGEST PROBLEM
    public string TileSize { get; set; } = string.Empty;
    public string AnalyticOfferGroupId = "ISTHISNEEDED??";
    public string FirstSeen = "31/12/9999"; // this is here for some reasons??

    // epic now use API colors (thanks @ShiinaBR for the colors <3)
    public string Color1 = "#424242";
    public string Color2 = "#212121";
    public string Color3 = "#121212";
}

public class BundleRequirement
{
    public bool bCanOwnMultiple = true;
    public int regularPrice = -999999;
    public int discountedPrice = -999999;
    public int alreadyOwnedPriceReduction = -999999;
    public Item item = new() { templateId = "AthenaCharacter:CID_349_Athena_Commando_M_Banana", quantity = 1 };
}

public class Requirement // dont touch nothing here
{
    public string requirementType = "DenyOnItemOwnership";
    public string requiredId { get; set; }
    public int minQuantity = 1;
}

public class GiftInfo
{
    public bool bIsEnabled = true;
    public string forcedGiftBoxTemplateId = string.Empty;
    public List<object> purchaseRequirements = []; // we dont know what are these, this is why <object>
    public List<object> giftRecordIds = [];
}

public class MetaInfo
{
    public string key { get; set; }
    public string value { get; set; }
}

public class ItemGrant
{
    public string templateId { get; set; }
    public int quantity = 10;
}

public class Item
{
    public string templateId { get; set; }
    public int quantity { get; set; }
}