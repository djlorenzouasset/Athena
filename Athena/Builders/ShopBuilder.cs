using CUE4Parse.Utils;
using Athena.Utils;
using Athena.Extensions;
using Athena.Models.Catalog;

namespace Athena.Builders;

public class ShopBuilder : BaseBuilder
{
    private readonly ShopModel _shop = new();
    private readonly List<ICatalogEntry> _catalogEntries = [];
    private readonly List<ICatalogEntry> _rmtEntries = [];

    private int _dailyCount = 0;
    private int _featuredCount = 0;
    private int _limitedTimeCount = 0;

    private const int MAX_DAILY_ITEMS = 4;
    private const int MAX_FEATURED_ITEMS = 2;
    private const int MAX_LIMITEDTIME_ITEMS = 2;

    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    public override string Build()
    {
        _shop.Storefronts.Add(new() { Name = "BRWeeklyStorefront", CatalogEntries = _catalogEntries });
        _shop.Storefronts.Add(new() { Name = "BRStarterKits", CatalogEntries = _rmtEntries });
        return Serialize(_shop);
    }

    public void AddCatalogEntry(string entry)
    {
        string objectPath = entry.Replace("FortniteGame/Plugins/GameFeatures/OfferCatalog/Content", "/OfferCatalog");
        string objectName = entry.Split("/").Last();
        string assetName = entry.Split("/").Last().Split("DAv2_").Last();

        if (assetName.Contains("BattlePass"))
            return;

        if (assetName.StartsWith("RMT", StringComparison.OrdinalIgnoreCase))
        {
            _rmtEntries.Add(ConstructCatalogEntry(objectPath, objectName, assetName));
            return;
        }

        _catalogEntries.Add(ConstructCatalogEntry(objectPath, objectName, assetName));
    }

    private ICatalogEntry ConstructCatalogEntry(string objPath, string objName, string assetName)
    {
        var catalogSettings = AppSettings.Default.CatalogSettings;

        bool bIsBundle = assetName.StartsWith("Bundle", StringComparison.OrdinalIgnoreCase);
        bool bIsRMT = assetName.StartsWith("RMT", StringComparison.OrdinalIgnoreCase);

        string offerId = bIsRMT ? GenerateRandomOfferId(32).ToUpper() : $"v2:/{GenerateRandomOfferId()}";
        string sectionId = bIsRMT ? "LimitedTime" : (bIsBundle ? "Featured" : "Daily");
        int rowIndex = 
            bIsRMT ? (_limitedTimeCount / MAX_LIMITEDTIME_ITEMS) + 1 : 
            (bIsBundle ? (_featuredCount / MAX_FEATURED_ITEMS) + 1 : (_dailyCount / MAX_DAILY_ITEMS) + 1);

        string layoutId = $"{sectionId}.{rowIndex}";
        string tileSize = bIsBundle || bIsRMT ? "Size_2_x_2" : "Size_1_x_2";
        string newDisplayAssetPath = $"{objPath}.{objName}";
        string displayAssetPath = DAv2ToDA(objName, bIsBundle, bIsRMT);

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

        if (bIsRMT)
        {
            var rmtEntry = new RMTCatalogEntry
            {
                OfferId = offerId,
                DisplayAssetPath = displayAssetPath,
                MetaInfo = metaInfo,
                Requirements = [new() { RequiredId = meta.TemplateId }]
            };

            var settings = catalogSettings.CustomRMTBundlesOptions.FirstOrDefault(
                option => option.RMTId.Equals(objName, StringComparison.OrdinalIgnoreCase));

            if (settings is not null)
                rmtEntry.SetOptions(settings); // set all custom settings

            var itemGrants = new List<string>();
            if (settings?.VBucksGranted > 0)
            {
                // add the VBucks as an item grant if specified
                itemGrants.Add("Currency:MtxPurchased");
                rmtEntry.MetaInfo.Add(new MetaInfo { Key = "MtxQuantity", Value = settings.VBucksGranted.ToString() });
            }

            if (settings?.RMTBundleCosmetics is { Count: > 0 } cosmetics)
            {
                foreach (var id in cosmetics)
                {
                    // construct the item's templateId
                    string backendType = AthenaUtils.GetBackendTypeByItemId(id);
                    itemGrants.Add($"{backendType}:{id}");
                }
            }

            int priceToApply = (int)(settings?.Price ?? -1.0);
            rmtEntry.SetPrice(priceToApply);
            // pass the items and the vbucks if there are
            rmtEntry.SetGrants(itemGrants, settings?.VBucksGranted ?? -1);

            _limitedTimeCount++;
            return rmtEntry;
        }
        else if (bIsBundle)
        {
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
    private string DAv2ToDA(string DAv2, bool bundle = false, bool rmt = false)
    {
        string id;
        string assetName;

        if (bundle)
        {
            id = DAv2.Split("Featured_").Last();
            assetName = $"DA_Featured_{id}_Bundle";
        }
        else if (rmt)
        {
            id = DAv2.Split("RMT_").Last().Replace("_", string.Empty);
            assetName = $"DA_Featured_{id}";
        }
        else
        {
            id = DAv2.Split("DAv2_").Last();
            assetName = $"DA_Featured_{id}";
        }

        return $"/OfferCatalog/DisplayAssets/{assetName}.{assetName}";
    }
}
