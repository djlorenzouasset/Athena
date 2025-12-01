using CUE4Parse.Utils;
using Athena.Utils;
using Athena.Extensions;
using Athena.Models.Catalog;

namespace Athena.Builders;

public class ShopBuilder : BaseBuilder
{
    private readonly ShopModel _shop = new();
    private readonly List<ICatalogEntry> _catalogEntries = [];

    private int _dailyCount = 0;
    private int _featuredCount = 0;
    private int _dailyRowIndex = 1;
    private int _featuredRowIndex = 1;

    private const int MAX_DAILY_ITEMS = 4;
    private const int MAX_FEATURED_ITEMS = 2;

    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public override string Build()
    {
        _shop.Storefronts.Add(new() { Name = "BRWeeklyStorefront", CatalogEntries = _catalogEntries });
        return Serialize(_shop);
    }

    public void AddCatalogEntry(string entry)
    {
        string objectPath = entry.Replace("FortniteGame/Plugins/GameFeatures/OfferCatalog/Content", "/OfferCatalog");
        string objectName = entry.Split("/").Last();
        string assetName = entry.Split("/").Last().Split("DAv2_").Last();

        if (assetName.StartsWith("RMT"))
            return;

        _catalogEntries.Add(ConstructCatalogEntry(objectPath, objectName, assetName));
    }

    private ICatalogEntry ConstructCatalogEntry(string objPath, string objName, string assetName)
    {
        var catalogSettings = AppSettings.Default.CatalogSettings;

        bool bIsBundle = assetName.StartsWith("Bundle", StringComparison.OrdinalIgnoreCase);

        string offerId = $"v2:/{GenerateRandomOfferId()}";
        string sectionId = bIsBundle ? "Featured" : "Daily";
        int rowIndex = bIsBundle ? _featuredRowIndex : _dailyRowIndex;

        string layoutId = $"{sectionId}.{rowIndex}";
        string tileSize = bIsBundle ? "Size_2_x_2" : "Size_1_x_2";
        string newDisplayAssetPath = $"{objPath}.{objName}";
        string displayAssetPath = DAv2ToDA(objName, bIsBundle);

        var meta = new Meta
        {
            NewDisplayAssetPath = newDisplayAssetPath,
            SectionId = sectionId,
            TileSize = tileSize,
            LayoutId = layoutId
        };

        var metaInfo = new List<MetaInfo>
        {
            new() { Key = "NewDisplayAssetPath", Value = newDisplayAssetPath },
            new() { Key = "SectionId", Value = sectionId },
            new() { Key = "TileSize", Value = tileSize },
            new() { Key = "LayoutId", Value = layoutId },
        };

        if (bIsBundle)
        {
            if (_featuredCount % MAX_FEATURED_ITEMS == 0 && _featuredCount != 0)
                _featuredRowIndex++;

            meta.TemplateId = $"DynamicBundle:b_{assetName.SubstringAfter("Featured_")}";

            var bundleEntry = new BundleCatalogEntry
            {
                OfferId = offerId,
                DisplayAssetPath = displayAssetPath,
                Meta = meta,
                MetaInfo = metaInfo
            };

            var settings = catalogSettings.CustomBundlesOptions.FirstOrDefault(
                option => option.BundleId.Equals(objName, StringComparison.OrdinalIgnoreCase));

            int priceToApply = settings?.Price ?? catalogSettings.DefaultBundlesPrice;
            var bundleCosmetics = (settings?.BundleCosmetics is { Count: > 0 } cosmetics)
                ? cosmetics : catalogSettings.DefaultBundleCosmetics;

            if (settings is not null)
                bundleEntry.SetOptions(settings); // set all custom settings
            else
                bundleEntry.SetCardOptions(catalogSettings.DefaultCardOptions); // set only card options by default values

            var itemGrants = new List<string>();
            foreach (var id in bundleCosmetics)
            {
                // construct the item's templateId
                string backendType = AthenaUtils.GetBackendTypeByItemId(id);
                itemGrants.Add($"{backendType}:{id}");
            }

            bundleEntry.SetPrice(priceToApply);
            // pass the price as we have to put it on every item
            bundleEntry.SetCosmetics(priceToApply, itemGrants);

            _featuredCount++;
            return bundleEntry;
        }
        else
        {
            if (_dailyCount % MAX_DAILY_ITEMS == 0 && _dailyCount != 0)
                _dailyRowIndex++;

            if (assetName.StartsWith("Featured", StringComparison.OrdinalIgnoreCase) ||
                assetName.StartsWith("BuildingProp", StringComparison.OrdinalIgnoreCase))
            {
                assetName = assetName.SubstringAfter('_');
            }

            string backendType = AthenaUtils.GetBackendTypeByItemId(assetName);
            string templateId = $"{backendType}:{assetName}";

            meta.TemplateId = templateId;

            var cosmeticEntry = new CosmeticCatalogEntry
            {
                OfferId = offerId,
                DisplayAssetPath = displayAssetPath,
                Meta = meta,
                MetaInfo = metaInfo
            };

            cosmeticEntry.ItemGrants.Add(new ItemGrant { TemplateId = templateId });
            cosmeticEntry.Requirements.Add(new Requirement { RequiredId = templateId });

            var settings = catalogSettings.CustomItemsOptions.FirstOrDefault(
                option => option.ItemId.Equals(objName, StringComparison.OrdinalIgnoreCase));

            int priceToApply = settings?.Price ?? catalogSettings.DefaultItemsPrice;
            cosmeticEntry.SetPrice(priceToApply);

            if (settings is not null)
                cosmeticEntry.SetOptions(settings);
            else
                cosmeticEntry.SetCardOptions(catalogSettings.DefaultCardOptions);

            _dailyCount++;
            return cosmeticEntry;
        }
    }

    private string GenerateRandomOfferId(int length = 45)
    {
        var chars = new char[length];
        var random = new Random();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = CHARS[random.Next(CHARS.Length)];
        }
        return new(chars);
    }

    // format the DAv2 to a DA shop asset 
    private string DAv2ToDA(string DAv2, bool bundle = false)
    {
        string id;
        string assetName;

        if (bundle)
        {
            id = DAv2.Split("Featured_").Last();
            assetName = $"DA_Featured_{id}_Bundle";
        }
        else
        {
            id = DAv2.Split("DAv2_").Last();
            assetName = $"DA_Featured_{id}";
        }

        return $"/OfferCatalog/DisplayAssets/{assetName}.{assetName}";
    }
}
