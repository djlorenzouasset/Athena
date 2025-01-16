using Newtonsoft.Json;
using CUE4Parse.Utils;
using Athena.Models;

namespace Athena.Services;

public class ShopBuilder
{
    private readonly ShopModel _shopModel = new();
    private readonly List<object> _catalogEntries = [];
    private readonly List<List<CosmeticCatalogEntry>> _daily = [];
    private readonly List<List<BundleCatalogEntry>> _featured = [];
    private readonly List<MetaInfo> _backgroundColors = [ // change these if you want to change colors
        new() { key = "color1", value = "#424242" },
        new() { key = "color2", value = "#212121" },
        new() { key = "color3", value = "#121212" },
    ];

    public string Build()
    {
        FillEntries(); // yes
        _shopModel.storefronts.Add(new() { name = "BRWeeklyStorefront", catalogEntries = _catalogEntries });
        return JsonConvert.SerializeObject(_shopModel, Formatting.Indented);
    }

    public void AddCatalogEntry(string shopAsset)
    {
        string shopItem = shopAsset.Replace("FortniteGame/Plugins/GameFeatures/OfferCatalog/Content", "/OfferCatalog");
        string backendType = shopAsset.Split("/").Last();
        string assetName = shopAsset.Split("/").Last().Split("DAv2_").Last();
        string offerId = Helper.GenerateRandomOfferId(); // this is because now the game checks it

        if (assetName.StartsWith("RMT"))
            return;

        /* 
        TileSizes (+v30.10):
            - DoubleWide: Size_2_x_2
            - Normal: Size_1_x_2

        There are more but we dont need them atm 
        */

        else if (assetName.StartsWith("Bundle"))
        {
            List<MetaInfo> metaInfo =
            [
                new() { key = "NewDisplayAssetPath", value = shopItem + '.' + backendType },
                new() { key = "SectionId", value = "Featured" },
                new() { key = "TileSize", value = "Size_2_x_2" },
                .._backgroundColors
            ];

            var meta = new Meta
            {
                NewDisplayAssetPath = shopItem + '.' + backendType,
                SectionId = "Featured",
                TileSize = "Size_2_x_2"
            };
            var ret = new BundleCatalogEntry
            {
                offerId = "v2:/" + offerId,
                meta = meta,
                metaInfo = metaInfo,
                displayAssetPath = Helper.DAv2ToDA(backendType, true)
            };
            AddAsset(ret, true);
        }
        else
        {
            if (assetName.StartsWith("Featured") ||  assetName.StartsWith("BuildingProp"))
            {
                assetName = assetName.SubstringAfter('_');
            }

            List<MetaInfo> metaInfo =
            [
                new() { key = "NewDisplayAssetPath", value = shopItem + '.' + backendType },
                new() { key = "SectionId", value = "Daily" },
                new() { key = "TileSize", value = "Size_1_x_2" },
                .._backgroundColors
            ];

            var meta = new Meta
            {
                NewDisplayAssetPath = shopItem + '.' + backendType,
                SectionId = "Daily",
                TileSize = "Size_1_x_2"
            };
            var ret = new CosmeticCatalogEntry
            {
                offerId = "v2:/" + offerId,
                meta = meta,
                metaInfo = metaInfo,
                displayAssetPath = Helper.DAv2ToDA(backendType)
            };

            ret.requirements.Add(new() { requiredId = $"{GetItemType(assetName)}:{assetName}" });
            ret.itemGrants.Add(new() { templateId = $"{GetItemType(assetName)}:{assetName}" });
            AddAsset(ret);
        }
    }

    private void FillEntries()
    {
        _featured.ForEach(x => _catalogEntries.AddRange(x));
        _daily.ForEach(x => _catalogEntries.AddRange(x));
    }

    // this is really scuffed, need to be rewrited in v2
    private void AddAsset(object data, bool isBundle = false)
    {
        if (isBundle)
        {
            if (_featured.Count == 0 || _featured.Last().Count == 2)
            {
                _featured.Add([(BundleCatalogEntry) data]);
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
                _daily.Add([(CosmeticCatalogEntry) data]);
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
        return Helper.GetItemBackendType(dav2);
    }
}