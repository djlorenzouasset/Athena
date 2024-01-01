using Newtonsoft.Json;

namespace Athena.Services;

public class ShopBuilder
{
    private List<object> _catalogEntries;
    private List<List<CosmeticCatalogEntry>> _daily;
    private List<List<BundleCatalogEntry>> _featured;
    private ShopModel _shopModel;

    public ShopBuilder()
    {
        _shopModel = new();
        _catalogEntries = new();
        _daily = new();
        _featured = new();
    }

    public string Build()
    {
        FillEntries(); // yes
        _shopModel.storefronts.Add(new() { name = "BRWeeklyStorefront", catalogEntries = _catalogEntries });
        return JsonConvert.SerializeObject(_shopModel, Formatting.Indented);
    }

    public void AddCatalogEntry(string shopAsset)
    {
        string shopItem = shopAsset.Replace("FortniteGame/Content", "/Game");
        string backendType = shopAsset.Split("/").Last();
        string assetName = shopAsset.Split("/").Last().Split("DAv2_").Last();
        string DAassetName = shopAsset.Split("/").Last().Split("DA_").Last().Split("Featured_").Last();
        string offerId = Helper.GenerateRandomOfferId();

        if (assetName.StartsWith("RMT")) // skip this offer type (in v26.30 they added this offer (??))
            return;

        else if (assetName.StartsWith("Bundle"))
        {
            List<MetaInfo> metaInfo = new()
            {
                new() { key = "NewDisplayAssetPath", value = shopItem + '.' + backendType },
                new() { key = "SectionId", value = "Featured" },
                new() { key = "TileSize", value = "DoubleWide" }
            };
            Meta meta = new()
            {
                NewDisplayAssetPath = shopItem + '.' + backendType,
                SectionId = "Featured",
                TileSize = "DoubleWide"
            };

            BundleCatalogEntry ret = new();
            ret.offerId = "v2:/" + offerId; // DO NOT CHANGE
            ret.meta = meta;
            ret.metaInfo = metaInfo;
            ret.displayAssetPath = Helper.DAv2ToDA(backendType, true);
            AddAsset(ret, true);
        }

        else if(assetName.StartsWith("DA"))
        {
            List<MetaInfo> metaInfo = new()
            {
                new() { key = "NewDisplayAssetPath", value = shopItem + '.' + backendType },
                new() { key = "SectionId", value = "Daily" },
                new() { key = "TileSize", value = "Normal" }
            };
            Meta meta = new()
            {
                NewDisplayAssetPath = shopItem + '.' + backendType,
                SectionId = "Daily",
                TileSize = "Normal"
            };
            CosmeticCatalogEntry ret = new();
            ret.offerId = "v2:/" + offerId; // DO NOT CHANGE
            ret.meta = meta;
            ret.requirements.Add(new() { requiredId = $"{GetItemType(DAassetName)}:{DAassetName}" });
            ret.metaInfo = metaInfo;
            ret.displayAssetPath = $"/Game/Catalog/DisplayAssets/{assetName}.{assetName}";
            ret.itemGrants.Add(new() { templateId = $"{GetItemType(DAassetName)}:{DAassetName}" });
            AddAsset(ret);
        }

        else
        {
            List<MetaInfo> metaInfo = new()
            {
                new() { key = "NewDisplayAssetPath", value = shopItem + '.' + backendType },
                new() { key = "SectionId", value = "Daily" },
                new() { key = "TileSize", value = "Normal" }
            };
            Meta meta = new()
            {
                NewDisplayAssetPath = shopItem + '.' + backendType,
                SectionId = "Daily",
                TileSize = "Normal"
            };

            CosmeticCatalogEntry ret = new();
            ret.offerId = "v2:/" + offerId; // DO NOT CHANGE
            ret.meta = meta;
            ret.requirements.Add(new() { requiredId = $"{GetItemType(assetName)}:{assetName}" });
            ret.metaInfo = metaInfo;
            ret.displayAssetPath = Helper.DAv2ToDA(backendType);
            ret.itemGrants.Add(new() { templateId = $"{GetItemType(assetName)}:{assetName}" });
            AddAsset(ret);
        }
    }

    private void FillEntries()
    {
        _featured.ForEach(x => _catalogEntries.AddRange(x));
        _daily.ForEach(x => _catalogEntries.AddRange(x));
    }

    private void AddAsset(object data, bool isBundle = false) // (should work) (idk) (i hope)
    {
        if (isBundle)
        {
            if (_featured.Count == 0 || _featured.Last().Count == 2)
            {
                _featured.Add(new() { (BundleCatalogEntry) data });
            }
            else
            {
                var current = _featured.Last();
                current.Add((BundleCatalogEntry) data);
            }

            // the new system of layout ids is annoying
            // so we have to do this every new line
            var added = _featured.Last().Last();
            added.meta.LayoutId = $"Featured.{_featured.Count}";
            added.metaInfo.Add(new() { key = "LayoutId", value = $"Featured.{_featured.Count}" });
        }
        else
        {
            if (_daily.Count == 0 || _daily.Last().Count == 5)
            {
                _daily.Add(new() { (CosmeticCatalogEntry) data });
            }
            else
            {
                var current = _daily.Last();
                current.Add((CosmeticCatalogEntry) data);
            }

            // stupid ass fix (should be changed asap)
            var added = _daily.Last().Last();
            added.meta.LayoutId = $"Daily.{_daily.Count}";
            added.metaInfo.Add(new() { key = "LayoutId", value = $"Daily.{_daily.Count}" });
        }
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