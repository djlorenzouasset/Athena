using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.UObject;
using Athena.Models.Profiles;

namespace Athena.Utils;

public static class AthenaUtils
{
    public static void ClearConsoleLines(int numberOfLines)
    {
        int startLine = Console.CursorTop - numberOfLines;
        if (startLine < 0)
        {
            startLine = 0;
        }

        for (int i = 0; i < numberOfLines; i++)
        {
            Console.SetCursorPosition(0, startLine + i);
            Console.Write(new string(' ', Console.WindowWidth));
        }
        Console.SetCursorPosition(0, startLine);
    }

    public static string GetBackendTypeByItemId(string itemId)
    {
        if (Assets.IsValidItemId(itemId))
        {
            return Assets.GetBackendTypeByIncludedName(itemId);
        }

        var instruments = new string[] {
            "Mic", "Keytar", "Guitar",
            "Drum", "DrumKit", "Bass"
        };

        string prefix;
        if (itemId.StartsWith("Companion_ReactFX"))
        {
            prefix = "Companion_ReactFx";
        }
        else if (itemId.StartsWith("SparksAura"))
        {
            prefix = "Aura";
        }
        else if (itemId.StartsWith("Sparks"))
        {
            var parts = itemId.Split('_');
            var last = parts[^1];
            if (instruments.Contains(last, StringComparer.OrdinalIgnoreCase))
            {
                prefix = last;
            }
            else
            {
                prefix = parts.Length > 1 ? parts[1] : itemId;
            }
        }
        else
        {
            prefix = itemId[..itemId.IndexOf('_')];
        }

        if (prefix.Equals("ID", StringComparison.OrdinalIgnoreCase))
            prefix = itemId.Split('_')[1];

        return Assets.GetBackendTypeByPrefix(prefix);
    }

    public static List<Variant> GetCosmeticVariants(UObject obj)
    {
        var cosmeticVariants = new List<Variant>();

        var variants = obj.GetOrDefault("ItemVariants", Array.Empty<UObject>());
        foreach (var variant in variants)
        {
            var ownedParts = new List<string>();

            var optionName = variant.ExportType switch
            {
                "FortCosmeticTextVariant" => "CustomName",
                "FortCosmeticMeshVariant" => "MeshOptions",
                "FortCosmeticMaterialVariant" => "MaterialOptions",
                "FortCosmeticParticleVariant" => "ParticleOptions",
                "FortCosmeticPropertyVariant" => "GenericPropertyOptions",
                // "FortCosmeticRichColorVariant" => "InlineVariant", skip it as we don't know how it works
                "FortCosmeticGameplayTagVariant" => "GenericTagOptions",
                "FortCosmeticMorphTargetVariant" => "MorphTargetOptions",
                "FortCosmeticAdditivePoseVariant" => "AdditivePoseOptions",
                "FortCosmeticCharacterPartVariant" => "PartOptions",
                "FortCosmeticCIDRedirectorVariant" => "CIDRedirectors",
                "FortCosmeticLoadoutTagDrivenVariant" => "Variants",
                "FortCustomizableObjectSprayVariant" => "ActiveSelectionTag",
                "FortCustomizableObjectParameterVariant" => "ParameterOptions",
                _ => null
            };

            if (optionName is null)
                continue;

            if (optionName == "CustomName")
            {
                ownedParts.Add("[PH]CompanionName - Athena");
            }
            else if (optionName == "ActiveSelectionTag")
            {
                var activeSelectionTag = variant.GetOrDefault<FStructFallback>(optionName);
                if (activeSelectionTag is null)
                    continue;

                var tag = activeSelectionTag.GetOrDefault("TagName", new FName("Variant.Tag.TBD")).Text;
                if (tag is null)
                    continue;

                // i dont know how this shit works internally but seems to work (tested)
                ownedParts.Add(tag.Split("Property.").Last() + ".X=ffff0000ffffSD=");
            }
            else
            {
                var options = variant.GetOrDefault<FStructFallback[]>(optionName);
                if (options is not { Length: > 0 })
                    continue;

                foreach (var option in options)
                {
                    var customizationVariantTag = option.GetOrDefault<FStructFallback>("CustomizationVariantTag");
                    if (customizationVariantTag is null) continue;

                    string tag = customizationVariantTag.GetOrDefault("TagName", new FName("Variant.Tag.TBD")).Text;
                    if (tag is null) continue;

                    var specialPropertyTags = new string[]
                    {
                        "Property.Color", "Vehicle.Painted", "Vehicle.Tier",
                        "Property.Outfit", "Property.Theme"
                    };
                    string ownedPart = specialPropertyTags.Any(st => tag.Contains(st, StringComparison.OrdinalIgnoreCase))
                        ? tag.Split("Property.").Last()
                        : tag.Split('.').Last();

                    ownedParts.Add(ownedPart);
                }
            }

            string? channel = null;

            var variantChannelTag = variant.GetOrDefault<FStructFallback>("VariantChannelTag");
            if (variantChannelTag is null && optionName == "Variants")
            {
                // when a cosmetic uses FortCosmeticLoadoutTagDrivenVariant, the first variant of that type
                // doesnt have the VariantChannelTag property. In this case, we set it to "TagDriven"
                // (I observed this in a real game profile).
                channel = "TagDriven";
            }
            else if (variantChannelTag is not null)
            {
                var specialChannelTags = new string[]
                {
                    "Slot", "Vehicle", "TagDriven",
                    "Theme", "Immutable"
                };

                string channelName = variantChannelTag.GetOrDefault("TagName", new FName("Variant.Channel.TBD")).Text;
                channel = specialChannelTags.Any(st => channelName.Contains(st, StringComparison.OrdinalIgnoreCase))
                    ? channelName.Split("Channel.").Last()
                    : channelName.Split('.').Last();
            }

            if (string.IsNullOrEmpty(channel))
            {
                continue;
            }

            cosmeticVariants.Add(new Variant
            {
                Channel = channel,
                Active = ownedParts.FirstOrDefault() ?? "",
                Owned = ownedParts
            });
        }

        return cosmeticVariants;
    }
}