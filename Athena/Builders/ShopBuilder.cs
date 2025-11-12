using CUE4Parse.Utils;
using Athena.Utils;
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
        string offerId = GenerateRandomOfferId(); // this is because now the game checks it

        if (assetName.StartsWith("RMT"))
            return;

        if (assetName.StartsWith("Bundle"))
        {
            if (_featuredCount % 2 == 0 && _featuredCount != 0)
                _featuredRowIndex++;

            string layoutId = $"Featured.{_featuredRowIndex}";

            Meta meta = new()
            {
                NewDisplayAssetPath = $"{objectPath}.{objectName}",
                SectionId = "Featured",
                TileSize = "Size_2_x_2",
                LayoutId = layoutId
            };
            List<MetaInfo> metaInfo = [
                new() { Key = "NewDisplayAssetPath", Value = $"{objectPath}.{objectName}" },
                new() { Key = "SectionId", Value = "Featured" },
                new() { Key = "TileSize", Value = "Size_2_x_2" },
                new() { Key = "LayoutId", Value = layoutId },
                ..Settings.Current.Catalog.CardGradients
            ];

            var bundleEntry = new BundleCatalogEntry
            {
                OfferId = offerId,
                DisplayAssetPath = DAv2ToDA(objectName, true),
                Meta = meta,
                MetaInfo = metaInfo
            };

            _catalogEntries.Add(bundleEntry);
            _featuredCount++;
        }
        else
        {
            if (_dailyCount % 5 == 0 && _dailyCount != 0)
                _dailyRowIndex++;

            if (assetName.StartsWith("Featured") || assetName.StartsWith("BuildingProp"))
            {
                assetName = assetName.SubstringAfter('_');
            }

            string layoutId = $"Daily.{_dailyRowIndex}";
            string backendType = AthenaUtils.GetBackendTypeByItemId(assetName);
            string itemId = $"{backendType}:{assetName}";

            Meta meta = new()
            {
                NewDisplayAssetPath = $"{objectPath}.{objectName}",
                SectionId = "Daily",
                TileSize = "Size_1_x_2",
                LayoutId = layoutId
            };
            List<MetaInfo> metaInfo = [
                new() { Key = "NewDisplayAssetPath", Value = $"{objectPath}.{objectName}" },
                new() { Key = "SectionId", Value = "Daily" },
                new() { Key = "TileSize", Value = "Size_1_x_2" },
                new() { Key = "LayoutId", Value = layoutId },
                ..Settings.Current.Catalog.CardGradients
            ];

            var itemEntry = new CosmeticCatalogEntry
            {
                OfferId = offerId,
                DisplayAssetPath = DAv2ToDA(objectName, true),
                Meta = meta,
                MetaInfo = metaInfo,
                Requirements = [
                    new() { RequiredId = "DenyOnOwnership" }
                ],
                ItemGrants = [
                    new() { TemplateId = itemId }
                ]
            };

            _catalogEntries.Add(itemEntry);
            _dailyCount++;
        }
    }

    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private string GenerateRandomOfferId(int total = 45)
    {
        var chars = new char[total];
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
