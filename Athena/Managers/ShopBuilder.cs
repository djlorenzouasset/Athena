using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Athena.Models;

namespace Athena.Managers;

public class ShopBuilder
{
    private List<object> _catalogEntries;
    private ShopModel _shopModel;

    public ShopBuilder()
    {
        _catalogEntries = new();
        _shopModel = new();
    }

    public override string ToString() => Build();

    public string Build()
    {
        _shopModel.storefronts.Add(new() { name = "BRDailyStorefront", catalogEntries = _catalogEntries });
        var shopJson = JObject.FromObject(_shopModel);

        if (shopJson is null) return string.Empty;

        return shopJson.ToString(Formatting.Indented);
    }

    public void AddCatalogEntry(string shopAssetData)
    {
        var shopItem = shopAssetData.Replace("FortniteGame/Content", "/Game");
        var backendType = shopAssetData.Split("/").Last();
        var assetName = shopAssetData.Split("/").Last().Split("DAv2_").Last();
        
        if (assetName.StartsWith("Bundle"))
        {
           List<MetaInfo> metaBundle = new() {
                new() { key = "NewDisplayAssetPath", value = shopItem + "." + backendType },
                new() { key = "SectionId", value = "Featured"},
                new() { key = "TileSize", value = "DoubleWide"},
                new() { key = "AnalyticOfferGroupId", value = "3" }
            };

            BundleCatalogEntry ret = new();
            ret.meta.NewDisplayAssetPath = shopItem + "." + backendType;
            ret.metaInfo = metaBundle;
            ret.displayAssetPath = Dav2ToDA(backendType);
            _catalogEntries.Add(ret);
        }
        else
        {
            List<MetaInfo> metaCosmetic = new() {
                new() { key = "NewDisplayAssetPath", value = shopItem + "." + backendType },
                new() { key = "SectionId", value = "Daily" },
                new() { key = "offertag", value = string.Empty },
                new() { key = "TileSize", value = "Normal" },
                new() { key = "AnalyticOfferGroupId", value = "4" },
            };

            var ret = new CosmeticCatalogEntry();
            ret.meta.NewDisplayAssetPath = shopItem + "." + backendType;
            ret.displayAssetPath = Dav2ToDA(backendType);
            ret.metaInfo = metaCosmetic;
            ret.requirements.Add(new() { requiredId = $"{GetItemType(assetName)}:{assetName}" } );
            ret.itemGrants.Add(new() { templateId = $"{GetItemType(assetName)}:{assetName}" });
            _catalogEntries.Add(ret);
        }
    }

    private string Dav2ToDA(string dav2)
    {
        string id = dav2.Split("Featured_").Last();
        string assetName = $"DA_Featured_{id}_Bundle";
        return $"/Game/Catalog/DisplayAssets/{assetName}.{assetName}";
    }

    private string GetItemType(string dav2)
    {
        string type = dav2.Split("_").First();

        return (type.ToLower()) switch
        {
            "cid" => "AthenaCharacter",
            "character" => "AthenaCharacter",

            "bid" => "AthenaBackpack",
            "backpak" => "AthenaBackpack",

            "pickaxe" => "AthenaPickaxe",
            "eid" => "AthenaDance",
            "glider" => "AthenaGlider",
            "wrap" => "AthenaItemWrap",
            "musicpack" => "AthenaMusicPack",

            "loadingscreen" => "AthenaLoadingScreen",
            "lsid" => "AthenaLoadingScreen",

            "contrail" => "AthenaSkyDiveContrail",
            "trails" => "AthenaSkyDiveContrail",

            "spray" => "AthenaDance",
            "emoji" => "AthenaDance",

            _ => "TBD"
        };
    }
}