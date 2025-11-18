using Athena.Models.Catalog;
using Athena.Models.Settings;

namespace Athena.Extensions;

public static class CatalogEntryExtensions
{
    public static void SetPrice(this ICatalogEntry entry, int price)
    {
        switch (entry)
        {
            case BundleCatalogEntry bundle:
                {
                    var info = new DynamicBundleInfo
                    {
                        DiscountedBasePrice = price,
                        RegularBasePrice = price,
                        FloorPrice = price
                    };
                    bundle.DynamicBundleInfo = info;
                    break;
                }
            case CosmeticCatalogEntry cosmetic:
                {
                    cosmetic.Prices =
                    [
                        new Price
                        {
                            RegularPrice = price,
                            DynamicRegularPrice = price,
                            FinalPrice = price,
                            BasePrice = price
                        }
                    ];
                    break;
                }
        }
    }

    public static void SetCosmetics(this BundleCatalogEntry entry, int price, List<string> templateIds)
    {
        foreach (var templateId in templateIds)
        {
            entry.ItemGrants.Add(new ItemGrant { TemplateId = templateId });
            entry.DynamicBundleInfo.BundleItems.Add(new BundleRequirement
            {
                RegularPrice = price,
                DiscountedPrice = price,
                AlreadyOwnedPriceReduction = price,
                Item = new Item { TemplateId = templateId }
            });
        }
    }

    public static void SetOptions(this ICatalogEntry entry, IOption options)
    {
        entry.SetCardOptions(options.CardOptions);

        if (string.IsNullOrEmpty(options.ViolatorTag))
            return;

        // shared meta & metaInfo
        entry.GetMeta(out var meta, out var metaInfo);

        string violatorTag = options.ViolatorTag;
        meta.ViolatorTag = violatorTag;
        meta.ViolatorIntensity = "High";

        metaInfo.AddRange(
        [
            new MetaInfo { Key = "ViolatorTag", Value = violatorTag },
            new MetaInfo { Key = "ViolatorIntensity", Value = "High" },
        ]);
    }

    public static void SetCardOptions(this ICatalogEntry entry, CardOptions options)
    {
        var defaults = AppSettings.Default.CatalogSettings.DefaultCardOptions;

        string color1 = string.IsNullOrEmpty(options.Color1) ? defaults.Color1 : options.Color1;
        string color2 = string.IsNullOrEmpty(options.Color2) ? defaults.Color2 : options.Color2;
        string color3 = string.IsNullOrEmpty(options.Color3) ? defaults.Color3 : options.Color3;
        string material = string.IsNullOrEmpty(options.Material) ? defaults.Material : options.Material;
        string textBackgroundColor = string.IsNullOrEmpty(options.TextBackgroundColor) ? defaults.TextBackgroundColor : options.TextBackgroundColor;

        // shared meta & metaInfo
        entry.GetMeta(out var meta, out var metaInfo);

        meta.Color1 = color1;
        meta.Color2 = color2;
        meta.Color3 = color3;
        meta.TileBackgroundMaterial = material;
        meta.TextBackgroundColor = textBackgroundColor;

        if (!string.IsNullOrEmpty(color1))
            metaInfo.Add(new MetaInfo { Key = "Color1", Value = color1 });

        if (!string.IsNullOrEmpty(color2))
            metaInfo.Add(new MetaInfo { Key = "Color2", Value = color2 });

        if (!string.IsNullOrEmpty(color3))
            metaInfo.Add(new MetaInfo { Key = "Color3", Value = color3 });

        if (!string.IsNullOrEmpty(textBackgroundColor))
            metaInfo.Add(new MetaInfo { Key = "TextBackgroundColor", Value = textBackgroundColor });

        if (!string.IsNullOrEmpty(material))
            metaInfo.Add(new MetaInfo { Key = "TileBackgroundMaterial", Value = material });
    }

    public static void GetMeta(this ICatalogEntry entry, out Meta meta, out List<MetaInfo> metaInfo)
    {
        switch (entry)
        {
            case BundleCatalogEntry bundle:
                meta = bundle.Meta;
                metaInfo = bundle.MetaInfo;
                break;
            case CosmeticCatalogEntry cosmetic:
                meta = cosmetic.Meta;
                metaInfo = cosmetic.MetaInfo;
                break;
            default: // this will never run
                meta = null!;
                metaInfo = null!;
                return;
        }
    }
}