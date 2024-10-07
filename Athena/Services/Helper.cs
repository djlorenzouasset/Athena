﻿using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.UObject;

public static class Helper
{
    private static readonly string DEFAULT_VARIANT_NAME = "[PH] VariantName"; // default name for variants name
    private static readonly string DEFAULT_STYLE_NAME = "[PH] StyleName"; // default name for styles name
    private static readonly string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; // characters for the offerId

    public static string GenerateRandomOfferId(int total = 45) // thanks hybrid for the fix :D 
    {
        var chars = new char[total];
        var random = new Random();
        for (int i = 0; i < chars.Length; i++)
        {
            chars[i] = CHARS[random.Next(CHARS.Length)];
        }
        return new(chars);
    }

    public static string DAv2ToDA(string DAv2, bool bundle = false) // format the DAv2 to a DA shop asset 
    {
        if (!bundle)
        {
            string id = DAv2.Split("DAv2_").Last();
            string assetName = $"DA_Featured_{id}";
            return $"/OfferCatalog/DisplayAssets/{assetName}.{assetName}";
        }
        else
        {
            string id = DAv2.Split("Featured_").Last();
            string assetName = $"DA_Featured_{id}_Bundle";
            return $"/OfferCatalog/DisplayAssets/{assetName}.{assetName}";
        }
    }

    public static Dictionary<string, List<string>> GetAllVariants(UObject obj) // get available variants for cosmetics 
    {
        // thanks to Half for allowing me use the function in this project <3
        Dictionary<string, List<string>> variants = new();

        var cosmeticStyles = obj.GetOrDefault("ItemVariants", Array.Empty<UObject>());
        foreach (var style in cosmeticStyles)
        {
            List<string> ownedParts = new();

            var channelTag = style.GetOrDefault<FStructFallback>("VariantChannelTag");
            if (channelTag is null)
                continue;

            string channelName = channelTag.GetOrDefault("TagName", new FName(DEFAULT_VARIANT_NAME)).Text;
            var optionsName = style.ExportType switch
            {
                "FortCosmeticCharacterPartVariant" => "PartOptions",
                "FortCosmeticGameplayTagVariant" => "GenericTagOptions",
                "FortCosmeticMaterialVariant" => "MaterialOptions",
                "FortCosmeticParticleVariant" => "ParticleOptions",
                "FortCosmeticPropertyVariant" => "GenericPropertyOptions",
                "FortCosmeticMeshVariant" => "MeshOptions",
                _ => null
            };

            if (optionsName is null) 
                continue;

            var options = style.Get<FStructFallback[]>(optionsName);
            if (options.Length == 0) 
                continue;

            foreach (var stageTag in options)
            {
                var CustomizationVariantTag = stageTag.Get<FStructFallback>("CustomizationVariantTag");
                if (CustomizationVariantTag is null) 
                    continue;

                string tag = CustomizationVariantTag.GetOrDefault("TagName", new FName(DEFAULT_STYLE_NAME)).Text;
                if (tag is null) 
                    continue;

                if (tag.Contains("Property.Color"))
                {
                    ownedParts.Add(tag.Split("Property.").Last());
                }
                else
                {
                    ownedParts.Add(tag.Split(".").Last());
                }
            }
            variants.Add(channelName.Split('.').Last(), ownedParts);
        }

        return variants;
    }
}