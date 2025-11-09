using Athena.Models.App;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.UObject;
using Spectre.Console;

namespace Athena.Utils;

public static class AthenaUtils
{
    public static readonly List<CosmeticType> CosmeticTypes = [
        new CosmeticType
        {
            BackendType = "AthenaCharacter",
            ExportTypes = ["AthenaCharacterItemDefinition"],
            ItemsPrefixes = ["CID", "Character"]
        },
        new CosmeticType
        {
            BackendType = "AthenaBackpack",
            ExportTypes = [
                "AthenaBackpackItemDefinition",
                "AthenaPetCarrierItemDefinition"
            ],
            ItemsPrefixes = [
                "BID", 
                "Backpack", 
                "PetCarrier"
            ],
            UnprefixedItems = [
                "Gadget_AlienSignalDetector",
                "Gadget_DetectorGadget",
                "Gadget_DetectorGadget_Ch4S2",
                "Gadget_HighTechBackpack",
                "Gadget_RealityBloom",
                "Gadget_SpiritVessel"
            ]
        },
        new CosmeticType
        {
            BackendType = "AthenaPickaxe",
            ExportTypes = ["AthenaPickaxeItemDefinition"],
            ItemsPrefixes = ["Pickaxe"],
            UnprefixedItems = [
                "DefaultPickaxe",
                "BoltonPickaxe",
                "Dev_Test_Pickaxe",
                "HalloweenScythe",
                "HappyPickaxe",
                "SickleBatPickaxe",
                "SkiIcePickaxe",
                "SpikyPickaxe"
            ]
        },
        new CosmeticType
        {
            BackendType = "AthenaDance",
            ExportTypes = [
                "AthenaDanceItemDefinition",
                "AthenaToyItemDefinition",
                "AthenaEmojiItemDefinition"
            ],
            ItemsPrefixes = [
                "EID",
                "Spray",
                "Spid",
                "Toy", 
                "Emoji",
                "Emoticon"
            ]
        },
        new CosmeticType
        {
            BackendType = "AthenaGlider",
            ExportTypes = ["AthenaGliderItemDefinition"],
            ItemsPrefixes  = [
                "Glider", 
                "Umbrella"
            ],
            UnprefixedItems = [
                "DefaultGlider",
                "Duo_Umbrella",
                "FounderGlider",
                "FounderUmbrella",
                "PreSeasonGlider",
                "PreSeasonGlider_Elite",
                "Solo_Umbrella",
                "Solo_Umbrella_MarkII",
                "Squad_Umbrella"
            ]
        },
        new CosmeticType
        {
            BackendType = "AthenaItemWrap",
            ExportTypes = ["AthenaItemWrapDefinition"],
            ItemsPrefixes = ["Wrap"],
            UnprefixedItems = ["ChillyFabric"]
        },
        new CosmeticType
        {
            BackendType = "AthenaSkyDiveContrail",
            ExportTypes = ["AthenaSkyDiveContrailItemDefinition"],
            ItemsPrefixes = [
                "Contrail", 
                "Trails"
            ],
            UnprefixedItems = ["DefaultContrail"]
        },
        new CosmeticType
        {
            BackendType = "AthenaMusicPack",
            ExportTypes = ["AthenaMusicPackItemDefinition"],
            ItemsPrefixes = ["MusicPack"]
        },
        new CosmeticType
        {
            BackendType = "AthenaLoadingScreen",
            ExportTypes = ["AthenaLoadingScreenItemDefinition"],
            ItemsPrefixes = [
                "LoadingScreen",
                "LSID"
            ]
        },
        new CosmeticType
        {
            BackendType = "CosmeticShoes",
            ExportTypes = ["CosmeticShoesItemDefinition"],
            ItemsPrefixes = ["Shoes"]
        },
        new CosmeticType
        {
            BackendType = "CosmeticMimosa",
            ExportTypes = ["CosmeticCompanionItemDefinition"],
            ItemsPrefixes = ["Companion"],
            UnprefixedItems = ["Mimosa_Random"]
        },
        new CosmeticType
        {
            BackendType = "CosmeticMimosaC",
            ExportTypes = ["CosmeticCompanionReactFXItemDefinition"],
            ItemsPrefixes = ["Companion_ReactFx"]
        },
        new CosmeticType
        { 
            BackendType = "SparksMicrophone",
            ExportTypes = ["SparksMicItemDefinition"],
            ItemsPrefixes = ["Mic"]
        },
        new CosmeticType
        { 
            BackendType = "SparksKeyboard",
            ExportTypes = ["SparksKeyboardItemDefinition"],
            ItemsPrefixes = ["Keytar"]
        },
        new CosmeticType
        { 
            BackendType = "SparksGuitar",
            ExportTypes = ["SparksGuitarItemDefinition"],
            ItemsPrefixes = ["Guitar"]
        },
        new CosmeticType
        { 
            BackendType = "SparksDrums",
            ExportTypes = ["SparksDrumItemDefinition"],
            ItemsPrefixes = [
                "Drum",
                "DrumKit"
            ]
        },
        new CosmeticType
        { 
            BackendType = "SparksBass",
            ExportTypes = ["SparksBassItemDefinition"],
            ItemsPrefixes = ["Bass"]
        },
        new CosmeticType 
        { 
            BackendType = "SparksAura",
            ExportTypes = ["SparksAuraItemDefinition"],
            ItemsPrefixes = ["Aura"]
        },
        new CosmeticType
        {
            BackendType = "SparksSong",
            ExportTypes = ["SparksSongItemDefinition"],
            ItemsPrefixes = ["Sid"]
        },        
        new CosmeticType
        {
            BackendType = "VehicleCosmetics_Body",
            ExportTypes = ["FortVehicleCosmeticsItemDefinition_Body"],
            ItemsPrefixes = [
                "Body",
                "CarBody"
            ]
        },
        new CosmeticType
        {
            BackendType = "VehicleCosmetics_Skin",
            ExportTypes = ["FortVehicleCosmeticsItemDefinition_Skin"],
            ItemsPrefixes = ["CarSkin"]
        },
        new CosmeticType
        {
            BackendType = "VehicleCosmetics_Booster",
            ExportTypes = ["FortVehicleCosmeticsItemDefinition_Booster"],
            ItemsPrefixes = ["Booster"]
        },
        new CosmeticType
        {
            BackendType = "VehicleCosmetics_Wheel",
            ExportTypes = ["FortVehicleCosmeticsItemDefinition_Wheel"],
            ItemsPrefixes = ["Wheel"]
        },
        new CosmeticType
        {
            BackendType = "FortVehicleCosmeticsItemDefinition_DriftTrail",
            ExportTypes = ["SparksSongItemDefinition"],
            ItemsPrefixes = ["DriftTrail"]
        },
        new CosmeticType
        {
            BackendType = "JunoBuildingProp",
            ExportTypes = ["JunoBuildingPropAccountItemDefinition"],
            ItemsPrefixes = ["JBPID"]
        },
        new CosmeticType
        {
            BackendType = "JunoBuildingSet",
            ExportTypes = ["JunoBuildingSetAccountItemDefinition"],
            ItemsPrefixes = ["JBSID"]
        }
    ];

    public static void ExitThread(int exitCode = 0)
    {
        Log.Information("Press the enter key to close the application");

        ConsoleKeyInfo keyInfo;
        do { keyInfo = Console.ReadKey(true); }
        while (keyInfo.Key != ConsoleKey.Enter);

        Log.ForContext("NoConsole", true).Information(" --------------- Application Closed --------------- ");
        Log.CloseAndFlush();
        Environment.Exit(exitCode);
    }

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

    // this function is used because AnsiConsole.Ask<T>() doesn't handle
    // text changes (like moving the arrows keys backwards/forwards
    public static string Ask(string prompt, int clearLines = 1)
    {
        invalid:
        AnsiConsole.Markup(prompt + " ");
        var textRead = Console.ReadLine();
        if (string.IsNullOrEmpty(textRead))
        {
            ClearConsoleLines(clearLines);
            goto invalid;
        }

        ClearConsoleLines(clearLines);
        return textRead;
    }

    public static string GetBackendType(string exportType)
    {
        return CosmeticTypes
            .FirstOrDefault(ct => ct.ExportTypes.Contains(exportType))
            ?.BackendType ?? "TBD";
    }

    public static string GetBackendTypeByItemId(string itemId)
    {
        if (CosmeticTypes
            .Any(ct => ct.UnprefixedItems != null && ct.UnprefixedItems.Contains(itemId)))
        {
            var upItem = CosmeticTypes
                .First(ct => ct.UnprefixedItems != null && ct.UnprefixedItems.Contains(itemId));
            return upItem.BackendType;
        }

        var instruments = new string[] {
            "Mic", "Keytar", "Guitar",
            "Drum", "Drumkit", "Bass"
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
            if (instruments.Contains(last))
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

        var item = CosmeticTypes
            .FirstOrDefault(ct => ct.ItemsPrefixes.Any(p =>
                p.Equals(prefix, StringComparison.OrdinalIgnoreCase)));

        return item?.BackendType ?? "TBD";
    }

    public static List<ProfileAthena.Variant> GetCosmeticVariants(UObject obj)
    {
        var cosmeticVariants = new List<ProfileAthena.Variant>();

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

            cosmeticVariants.Add(new ProfileAthena.Variant
            {
                Channel = channel,
                Active = ownedParts.FirstOrDefault() ?? "",
                Owned = ownedParts
            });
        }

        return cosmeticVariants;
    }
}