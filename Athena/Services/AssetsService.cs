using Athena.Models;

namespace Athena.Services;

public class AssetsService
{
    public readonly List<ItemEntry> Items = 
    [
        new ItemEntry
        {
            BackendType = "AthenaCharacter",
            ClassNames = ["AthenaCharacterItemDefinition"],
            Prefixes = [
                "CID_", 
                "Character_"
            ]
        },
        new ItemEntry
        {
            BackendType = "AthenaBackpack",
            ClassNames = [
                "AthenaBackpackItemDefinition",
                "AthenaPetCarrierItemDefinition"
            ],
            Prefixes = [
                "BID_",
                "Backpack_",
                "PetCarrier_"
            ],
            IncludeNames = [
                "Gadget_AlienSignalDetector",
                "Gadget_DetectorGadget",
                "Gadget_DetectorGadget_Ch4S2",
                "Gadget_HighTechBackpack",
                "Gadget_RealityBloom",
                "Gadget_SpiritVessel"
            ]
        },
        new ItemEntry
        {
            BackendType = "AthenaPickaxe",
            ClassNames = ["AthenaPickaxeItemDefinition"],
            Prefixes = ["Pickaxe_"],
            IncludeNames = [
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
        new ItemEntry
        {
            BackendType = "AthenaDance",
            ClassNames = [
                "AthenaDanceItemDefinition",
                "AthenaToyItemDefinition",
                "AthenaEmojiItemDefinition"
            ],
            Prefixes = [
                "EID_",
                "Spray_",
                "Spid_",
                "Toy_",
                "Emoji_",
                "Emoticon_"
            ]
        },
        new ItemEntry
        {
            BackendType = "AthenaGlider",
            ClassNames = ["AthenaGliderItemDefinition"],
            Prefixes  = [
                "Glider_",
                "Umbrella_"
            ],
            IncludeNames = [
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
        new ItemEntry
        {
            BackendType = "AthenaItemWrap",
            ClassNames = ["AthenaItemWrapDefinition"],
            Prefixes = ["Wrap_"],
            IncludeNames = ["ChillyFabric"]
        },
        new ItemEntry
        {
            BackendType = "AthenaSkyDiveContrail",
            ClassNames = ["AthenaSkyDiveContrailItemDefinition"],
            Prefixes = [
                "Contrail_",
                "Trails_"
            ],
            IncludeNames = ["DefaultContrail"]
        },
        new ItemEntry
        {
            BackendType = "AthenaMusicPack",
            ClassNames = ["AthenaMusicPackItemDefinition"],
            Prefixes = ["MusicPack_"]
        },
        new ItemEntry
        {
            BackendType = "AthenaLoadingScreen",
            ClassNames = ["AthenaLoadingScreenItemDefinition"],
            Prefixes = [
                "LoadingScreen_",
                "LSID_"
            ]
        },
        new ItemEntry
        {
            BackendType = "CosmeticShoes",
            ClassNames = ["CosmeticShoesItemDefinition"],
            Prefixes = ["Shoes_"]
        },
        new ItemEntry
        {
            BackendType = "CosmeticMimosa",
            ClassNames = ["CosmeticCompanionItemDefinition"],
            Prefixes = ["Companion_"],
            IncludeNames = ["Mimosa_Random"]
        },
        new ItemEntry
        {
            BackendType = "CosmeticMimosaC",
            ClassNames = ["CosmeticCompanionReactFXItemDefinition"],
            Prefixes = ["Companion_ReactFx_"]
        },
        new ItemEntry
        {
            BackendType = "CosmeticVariantToken",
            ClassNames = [],
            Prefixes = ["VTID_"]
        },
        new ItemEntry
        {
            BackendType = "SparksMicrophone",
            ClassNames = ["SparksMicItemDefinition"],
            Prefixes = ["Mic_"]
        },
        new ItemEntry
        {
            BackendType = "SparksKeyboard",
            ClassNames = ["SparksKeyboardItemDefinition"],
            Prefixes = ["Keytar_"]
        },
        new ItemEntry
        {
            BackendType = "SparksGuitar",
            ClassNames = ["SparksGuitarItemDefinition"],
            Prefixes = ["Guitar_"]
        },
        new ItemEntry
        {
            BackendType = "SparksDrums",
            ClassNames = ["SparksDrumItemDefinition"],
            Prefixes = [
                "Drum_",
                "DrumKit_"
            ]
        },
        new ItemEntry
        {
            BackendType = "SparksBass",
            ClassNames = ["SparksBassItemDefinition"],
            Prefixes = ["Bass_"]
        },
        new ItemEntry
        {
            BackendType = "SparksAura",
            ClassNames = ["SparksAuraItemDefinition"],
            Prefixes = ["Aura_"]
        },
        new ItemEntry
        {
            BackendType = "SparksSong",
            ClassNames = ["SparksSongItemDefinition"],
            Prefixes = ["Sid_"]
        },
        new ItemEntry
        {
            BackendType = "VehicleCosmetics_Body",
            ClassNames = ["FortVehicleCosmeticsItemDefinition_Body"],
            Prefixes = [
                "Body_",
                "CarBody_"
            ]
        },
        new ItemEntry
        {
            BackendType = "VehicleCosmetics_Skin",
            ClassNames = ["FortVehicleCosmeticsItemDefinition_Skin"],
            Prefixes = ["CarSkin_"]
        },
        new ItemEntry
        {
            BackendType = "VehicleCosmetics_Booster",
            ClassNames = ["FortVehicleCosmeticsItemDefinition_Booster"],
            Prefixes = ["Booster_"]
        },
        new ItemEntry
        {
            BackendType = "VehicleCosmetics_Wheel",
            ClassNames = ["FortVehicleCosmeticsItemDefinition_Wheel"],
            Prefixes = ["Wheel_"]
        },
        new ItemEntry
        {
            BackendType = "FortVehicleCosmeticsItemDefinition_DriftTrail",
            ClassNames = ["SparksSongItemDefinition"],
            Prefixes = ["DriftTrail_"]
        },
        new ItemEntry
        {
            BackendType = "JunoBuildingProp",
            ClassNames = ["JunoBuildingPropAccountItemDefinition"],
            Prefixes = ["JBPID_"]
        },
        new ItemEntry
        {
            BackendType = "JunoBuildingSet",
            ClassNames = ["JunoBuildingSetAccountItemDefinition"],
            Prefixes = ["JBSID_"]
        }
    ];

    public bool IsValidClass(string exportClass)
        => Items.Any(item => item.ClassNames.Contains(exportClass));

    public bool IsValidItemId(string itemId)
        => Items.Any(item => item.IncludeNames is { } names && names.Contains(itemId));

    public bool IsValidPrefix(string itemId)
        => Items.Any(item => item.Prefixes.Any(p => itemId.StartsWith(p)));

    public string GetBackendTypeByClass(string exportClass)
        => Items.FirstOrDefault(item => item.ClassNames.Contains(exportClass))?.BackendType ?? "TBD";

    public string GetBackendTypeByPrefix(string prefix)
        => Items.FirstOrDefault(item => item.Prefixes.Contains(prefix))?.BackendType ?? "TBD";

    public string GetBackendTypeByIncludedName(string prefix)
        => Items.FirstOrDefault(item => item.Prefixes.Contains(prefix))?.BackendType ?? "TBD";
}
