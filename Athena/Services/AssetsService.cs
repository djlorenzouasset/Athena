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
            Prefixes = ["CID", "Character"]
        },
        new ItemEntry
        {
            BackendType = "AthenaBackpack",
            ClassNames = [
                "AthenaBackpackItemDefinition",
                "AthenaPetCarrierItemDefinition"
            ],
            Prefixes = [
                "BID",
                "Backpack",
                "PetCarrier"
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
            Prefixes = ["Pickaxe"],
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
                "EID",
                "Spray",
                "Spid",
                "Toy",
                "Emoji",
                "Emoticon"
            ]
        },
        new ItemEntry
        {
            BackendType = "AthenaGlider",
            ClassNames = ["AthenaGliderItemDefinition"],
            Prefixes  = [
                "Glider",
                "Umbrella"
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
            Prefixes = ["Wrap"],
            IncludeNames = ["ChillyFabric"]
        },
        new ItemEntry
        {
            BackendType = "AthenaSkyDiveContrail",
            ClassNames = ["AthenaSkyDiveContrailItemDefinition"],
            Prefixes = [
                "Contrail",
                "Trails"
            ],
            IncludeNames = ["DefaultContrail"]
        },
        new ItemEntry
        {
            BackendType = "AthenaMusicPack",
            ClassNames = ["AthenaMusicPackItemDefinition"],
            Prefixes = ["MusicPack"]
        },
        new ItemEntry
        {
            BackendType = "AthenaLoadingScreen",
            ClassNames = ["AthenaLoadingScreenItemDefinition"],
            Prefixes = [
                "LoadingScreen",
                "LSID"
            ]
        },
        new ItemEntry
        {
            BackendType = "CosmeticShoes",
            ClassNames = ["CosmeticShoesItemDefinition"],
            Prefixes = ["Shoes"]
        },
        new ItemEntry
        {
            BackendType = "CosmeticMimosa",
            ClassNames = ["CosmeticCompanionItemDefinition"],
            Prefixes = ["Companion"],
            IncludeNames = ["Mimosa_Random"]
        },
        new ItemEntry
        {
            BackendType = "CosmeticMimosaC",
            ClassNames = ["CosmeticCompanionReactFXItemDefinition"],
            Prefixes = ["Companion_ReactFx"]
        },
        new ItemEntry
        {
            BackendType = "SparksMicrophone",
            ClassNames = ["SparksMicItemDefinition"],
            Prefixes = ["Mic"]
        },
        new ItemEntry
        {
            BackendType = "SparksKeyboard",
            ClassNames = ["SparksKeyboardItemDefinition"],
            Prefixes = ["Keytar"]
        },
        new ItemEntry
        {
            BackendType = "SparksGuitar",
            ClassNames = ["SparksGuitarItemDefinition"],
            Prefixes = ["Guitar"]
        },
        new ItemEntry
        {
            BackendType = "SparksDrums",
            ClassNames = ["SparksDrumItemDefinition"],
            Prefixes = [
                "Drum",
                "DrumKit"
            ]
        },
        new ItemEntry
        {
            BackendType = "SparksBass",
            ClassNames = ["SparksBassItemDefinition"],
            Prefixes = ["Bass"]
        },
        new ItemEntry
        {
            BackendType = "SparksAura",
            ClassNames = ["SparksAuraItemDefinition"],
            Prefixes = ["Aura"]
        },
        new ItemEntry
        {
            BackendType = "SparksSong",
            ClassNames = ["SparksSongItemDefinition"],
            Prefixes = ["Sid"]
        },
        new ItemEntry
        {
            BackendType = "VehicleCosmetics_Body",
            ClassNames = ["FortVehicleCosmeticsItemDefinition_Body"],
            Prefixes = [
                "Body",
                "CarBody"
            ]
        },
        new ItemEntry
        {
            BackendType = "VehicleCosmetics_Skin",
            ClassNames = ["FortVehicleCosmeticsItemDefinition_Skin"],
            Prefixes = ["CarSkin"]
        },
        new ItemEntry
        {
            BackendType = "VehicleCosmetics_Booster",
            ClassNames = ["FortVehicleCosmeticsItemDefinition_Booster"],
            Prefixes = ["Booster"]
        },
        new ItemEntry
        {
            BackendType = "VehicleCosmetics_Wheel",
            ClassNames = ["FortVehicleCosmeticsItemDefinition_Wheel"],
            Prefixes = ["Wheel"]
        },
        new ItemEntry
        {
            BackendType = "FortVehicleCosmeticsItemDefinition_DriftTrail",
            ClassNames = ["SparksSongItemDefinition"],
            Prefixes = ["DriftTrail"]
        },
        new ItemEntry
        {
            BackendType = "JunoBuildingProp",
            ClassNames = ["JunoBuildingPropAccountItemDefinition"],
            Prefixes = ["JBPID"]
        },
        new ItemEntry
        {
            BackendType = "JunoBuildingSet",
            ClassNames = ["JunoBuildingSetAccountItemDefinition"],
            Prefixes = ["JBSID"]
        }
    ];

    // TODO: Rework the functions 

    public bool IsValidClass(string exportClass)
        => Items.Any(item => item.ClassNames.Contains(exportClass));

    public bool IsValidItemId(string itemId)
        => Items.Any(item => item.IncludeNames is { } names && names.Contains(itemId));

    public bool IsValidPrefix(string itemId)
        => Items.Any(item => item.Prefixes.Any(p => itemId.StartsWith(p)));

    public string GetBackendTypeByClass(string exportClass)
        => Items.First(item => item.ClassNames.Contains(exportClass)).BackendType;

    public string GetBackendTypeByPrefix(string prefix)
        => Items.First(item => item.Prefixes.Contains(prefix)).BackendType;

    public string GetBackendTypeByIncludedName(string prefix)
        => Items.First(item => item.Prefixes.Contains(prefix)).BackendType;
}
