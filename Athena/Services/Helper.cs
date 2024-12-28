using System.Runtime.InteropServices;
using CUE4Parse.Utils;
using CUE4Parse.Compression;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Objects;
using CUE4Parse.UE4.Objects.UObject;
using Athena.Managers;
using System.Diagnostics;

namespace Athena.Services;

public static class Helper
{
    private static readonly Dictionary<string, string> _defaultCosmetics = new() { // default items (issue #33)
        { "DefaultPickaxe", "AthenaPickaxe" },
        { "DefaultGlider", "AthenaGlider" }
    };

    private static readonly string DEFAULT_VARIANT_NAME = "[PH] VariantName"; // default name for variants name
    private static readonly string DEFAULT_STYLE_NAME = "[PH] StyleName"; // default name for styles name
    private static readonly string CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; // characters for the offerId

    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hInstance, string lpText, string lpCaption, uint type);

    [DllImport("kernel32.dll")]
    public static extern int GetConsoleWindow();

    public static void ExitThread(int exitCode = 0)
    {
        Log.Information("Press the enter key to close the application");

        ConsoleKeyInfo keyInfo;
        do { keyInfo = Console.ReadKey(true); }
        while (keyInfo.Key != ConsoleKey.Enter);

        Log.Information(" --------------- Application Closed --------------- ");
        Log.CloseAndFlush();
        Environment.Exit(exitCode);
    }

    public static void ExitThreadAfterUpdate(string version)
    {
        Log.Information("Starting Athena updater.");
        string thisInstance = Path.Combine(DirectoryManager.Current, "Athena.exe");
        string newVersion = Path.Combine(DirectoryManager.ChunksDir, "Athena.exe");

        var startInfo = new ProcessStartInfo
        {
            FileName = DirectoryManager.UpdaterFile,
            Arguments = $"\"{thisInstance}\" \"{newVersion}\" \"{version}\"",
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        Process.Start(startInfo);
        Log.Information(" --------------- Application closed after update --------------- ");
        Log.CloseAndFlush();
        Environment.Exit(0);
    }

    public static async Task InitializeOodle()
    {
        var path = Path.Combine(DirectoryManager.ChunksDir, OodleHelper.OODLE_DLL_NAME);
        if (File.Exists(OodleHelper.OODLE_DLL_NAME))
        {
            File.Move(OodleHelper.OODLE_DLL_NAME, path, true);
        }
        else if (!File.Exists(path))
        {
            await OodleHelper.DownloadOodleDllAsync(path);
        }

        OodleHelper.Initialize(path);
    }

    public static async Task InitializeZlib()
    {
        var zlibPath = Path.Combine(DirectoryManager.ChunksDir, ZlibHelper.DLL_NAME);
        if (!File.Exists(zlibPath))
        {
            await ZlibHelper.DownloadDllAsync(zlibPath);
        }

        ZlibHelper.Initialize(zlibPath);
    }

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

    public static string GetItemBackendType(string id)
    {
        string prefix;
        if (_defaultCosmetics.TryGetValue(id, out string? value))
        {
            return value;
        }
        else if (id.StartsWith("Sparks_"))
        {
            prefix = id.SubstringAfter('_').SubstringBefore('_');
        }
        else
        {
            prefix = id.StartsWith("SparksAura") 
                ? "aura" : id.Remove(id.IndexOf('_')); 
        }

        return prefix.ToLower() switch
        {
            /* BR */
            "cid" => "AthenaCharacter",
            "character" => "AthenaCharacter",

            "bid" => "AthenaBackpack",
            "backpack" => "AthenaBackpack",
            "petcarrier" => "AthenaBackpack",

            "eid" => "AthenaDance",
            "spray" => "AthenaDance",
            "spid" => "AthenaDance",
            "emoji" => "AthenaDance",
            "toy" => "AthenaDance",

            "glider" => "AthenaGlider",
            "umbrella" => "AthenaGlider",

            "wrap" => "AthenaItemWrap",
            "musicpack" => "AthenaMusicPack",

            "loadingscreen" => "AthenaLoadingScreen",
            "lsid" => "AthenaLoadingScreen",

            "pickaxe" => "AthenaPickaxe",
            "contrail" => "AthenaSkyDiveContrail",
            "trails" => "AthenaSkyDiveContrail",
            "shoes" => "CosmeticShoes",

            /* FESTIVAL */
            "mic" => "SparksMicrophone",
            "keytar" => "SparksKeyboard",
            "guitar" => "SparksGuitar",
            "drum" => "SparksDrums",
            "bass" => "SparksBass",
            "aura" => "SparksAura",
            "sid" => "SparksSong",

            /* ROCKET RACING */
            "id" => $"VehicleCosmetics_{id.Split('_')[1]}",
            "wheel" => $"VehicleCosmetics_Wheel",
            "carbody" => $"VehicleCosmetics_Body",
            "carskin" => $"VehicleCosmetics_Skin",

            /* LEGO */
            "jbpid" => "JunoBuildingProp",
            "jbsid" => "JunoBuildingSet",

            /* NOT FOUND */
            _ => "TBD"
        };
    }

    public static Dictionary<string, List<string>> GetAllVariants(UObject obj) // get available variants for cosmetics 
    {
        // thanks to Half for allowing me use the function in this project <3
        Dictionary<string, List<string>> variants = [];

        var cosmeticStyles = obj.GetOrDefault("ItemVariants", Array.Empty<UObject>());
        foreach (var style in cosmeticStyles)
        {
            List<string> ownedParts = [];

            var channelTag = style.GetOrDefault<FStructFallback>("VariantChannelTag");
            if (channelTag is null) continue;

            string channelName = channelTag.GetOrDefault("TagName", new FName(DEFAULT_VARIANT_NAME)).Text;
            var optionsName = style.ExportType switch
            {
                "FortCosmeticMeshVariant" => "MeshOptions",
                "FortCosmeticMaterialVariant" => "MaterialOptions",
                "FortCosmeticParticleVariant" => "ParticleOptions",
                "FortCosmeticPropertyVariant" => "GenericPropertyOptions",
                "FortCosmeticRichColorVariant" => "InlineVariant",
                "FortCosmeticGameplayTagVariant" => "GenericTagOptions",
                "FortCosmeticCharacterPartVariant" => "PartOptions",
                "FortCustomizableObjectSprayVariant" => "ActiveSelectionTag",
                "FortCustomizableObjectParameterVariant" => "ParameterOptions",
                _ => null
            };

            if (optionsName is null) 
                continue;

            if (optionsName == "ActiveSelectionTag")
            {
                var activeSectionTag = style.Get<FStructFallback>(optionsName);
                if (activeSectionTag is null) continue;

                var tag = activeSectionTag.GetOrDefault("TagName", new FName(DEFAULT_STYLE_NAME)).Text;
                if (tag is null) continue;

                ownedParts.Add(tag.Split("Property.").Last() + ".X=ffff0000ffffSD="); // ?? idk
            }
            else if (optionsName == "InlineVariant") // ??
            {
                // idk (??) need a revision
            }
            else
            {
                var options = style.Get<FStructFallback[]>(optionsName);
                if (options.Length == 0) 
                    continue;

                foreach (var stageTag in options)
                {
                    var customizationVariantTag = stageTag.Get<FStructFallback>("CustomizationVariantTag");
                    if (customizationVariantTag is null) continue;

                    string tag = customizationVariantTag.GetOrDefault("TagName", new FName(DEFAULT_STYLE_NAME)).Text;
                    if (tag is null) continue;

                    if (tag.Contains("Property.Color") || tag.Contains("Vehicle.Painted"))
                    {
                        ownedParts.Add(tag.Split("Property.").Last());
                    }
                    else
                    {
                        ownedParts.Add(tag.Split(".").Last());
                    }
                }
            }

            string channel;
            if (channelName.Contains("Slot") || channelName.Contains("Vehicle"))
            {
                channel = channelName.Split("Channel.").Last();
            }
            else
            {
                channel = channelName.Split('.').Last();
            }

            variants.Add(channel, ownedParts);
        }

        return variants;
    }
}