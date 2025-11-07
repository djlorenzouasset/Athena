using Newtonsoft.Json;
using Athena.Models.App;

namespace Athena.Services;

public class ShopBuilder
{
    private const string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; // characters for the offerId

    private readonly ShopModel _shop = new();
    private readonly List<ICatalogEntry> _catalogEntries = [];

    private int _dailyCount = 0;
    private int _featuredCount = 0;
    private int _dailyRowIndex = 1;
    private int _featuredRowIndex = 1;

    public string Build()
    {
        _shop.Storefronts.Add(new() { Name = "BRWeeklyStorefront", CatalogEntries = _catalogEntries });
        return JsonConvert.SerializeObject(_shop, Formatting.Indented);
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
                new() { Key = "NewDisplayAssetPath", Value =$"{objectPath}.{objectName}" },
                new() { Key = "SectionId", Value = "Featured" },
                new() { Key = "TileSize", Value = "Size_2_x_2" },
                new() { Key = "LayoutId", Value = layoutId },
                ..SettingsService.Current.Catalog.CardGradients
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

            string layoutId = $"Daily.{_dailyRowIndex}";

            Meta meta = new()
            {
                NewDisplayAssetPath = $"{objectPath}.{objectName}",
                SectionId = "Daily",
                TileSize = "Size_1_x_2",
                LayoutId = layoutId
            };
            List<MetaInfo> metaInfo = [
                new() { Key = "NewDisplayAssetPath", Value =$"{objectPath}.{objectName}" },
                new() { Key = "SectionId", Value = "Daily" },
                new() { Key = "TileSize", Value = "Size_1_x_2" },
                new() { Key = "LayoutId", Value = layoutId },
                ..SettingsService.Current.Catalog.CardGradients
            ];

            var itemEntry = new CosmeticCatalogEntry
            {
                OfferId = offerId,
                DisplayAssetPath = DAv2ToDA(objectName, true),
                Meta = meta,
                MetaInfo = metaInfo
            };

            _catalogEntries.Add(itemEntry);
            _dailyCount++;
        }
    }

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

    private string DAv2ToDA(string DAv2, bool bundle = false) // format the DAv2 to a DA shop asset 
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
