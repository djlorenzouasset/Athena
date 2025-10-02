using System.Diagnostics;
using System.Text.RegularExpressions;
using Spectre.Console;
using K4os.Compression.LZ4.Streams;
using CommunityToolkit.HighPerformance;
using EpicManifestParser.Api;
using CUE4Parse.Utils;
using CUE4Parse.FileProvider;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.VirtualFileSystem;
using CUE4Parse.UE4.Objects.Core.Misc;
using Athena.Rest;
using Athena.Models;
using Athena.Services;

namespace Athena.Managers;

public class Dataminer
{
    public static readonly Dataminer Instance = new();

    public StreamedFileProvider Provider = null!;
    private ManifestDownloader _manifestService = null!;

    private string _backupName = string.Empty;
    private string _currentGenerationType = string.Empty;

    private readonly HashSet<VfsEntry> _allEntries = [];
    private readonly HashSet<VfsEntry> _newEntries = [];
    private readonly List<string> _ioStoreNames = [];
    private readonly List<string> _itemsFilter = [];

    private readonly FGuid _zeroGuid = new(0, 0, 0, 0); // this is just for dont load dynamic keys
    private readonly Regex _pakNameRegex = new(@"^pakchunk(\d*)-WindowsClient.utoc", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant); // we use this just for the bulk function

    private const uint _LZ4Magic = 0x184D2204u;
    private const uint _backupMagic = 0x504B4246;

    private readonly string[] _accepted = [ // better filtering coming in v2
        "CID", "Character",
        "BID", "Backpack",
        "EID", "Pickaxe",
        "Glider", "Wrap", "Shoes",
        "MusicPack", "Umbrella",
        "LSID", "LoadingScreen",
        "Contrail", "Trails",
        "PetCarrier", "Spid",
        "Toy", "Emoji", "Emoticon", "Spray",
        "Sparks_", "SparksAura", "SID",
        "ID", "Wheel", "CarBody", "CarSkin", "Body" /* issue #54 */,
        "JBPID_", "JBSID_", // _ is for avoid export errors
        "DefaultContrail", "DefaultGlider", "DefaultPickaxe", // default items (issue #33)

        "BoltonPickaxe", "Dev_Test_Pickaxe", "HalloweenScythe", // pickaxes without prefix (issue #61)
        "HappyPickaxe", "SickleBatPickaxe", "SkiIcePickaxe", "SpikyPickaxe",

        "ChillyFabric", "FounderGlider", "FounderUmbrella", "Gadget_AlienSignalDetector", // more unprefixed cosmetics (issue #69)
        "Gadget_DetectorGadget", "Gadget_DetectorGadget_Ch4S2", "Gadget_HighTechBackpack",
        "Gadget_RealityBloom", "Gadget_SpiritVessel", "PreSeasonGlider", "PreSeasonGlider_Elite",
        "Solo_Umbrella", "Solo_Umbrella_MarkII", "Squad_Umbrella", "Duo_Umbrella"
    ];
    private readonly string[] _classes = [
        // BR
        "AthenaCharacterItemDefinition", "AthenaBackpackItemDefinition",
        "AthenaPickaxeItemDefinition", "AthenaGliderItemDefinition",
        "AthenaPetCarrierItemDefinition", "AthenaToyItemDefinition",
        "AthenaEmojiItemDefinition", "AthenaSprayItemDefinition",
        "AthenaLoadingScreenItemDefinition", "AthenaDanceItemDefinition",
        "AthenaSkyDiveContrailItemDefinition", "AthenaItemWrapDefinition",
        "AthenaMusicPackItemDefinition", "CosmeticShoesItemDefinition",

        // FORTNITE FESTIVAL
        "SparksGuitarItemDefinition", "SparksBassItemDefinition",
        "SparksKeyboardItemDefinition", "SparksDrumItemDefinition",
        "SparksMicItemDefinition", "SparksAuraItemDefinition",
        "SparksSongItemDefinition",

        // ROCKET RACING
        "FortVehicleCosmeticsItemDefinition_Skin",
        "FortVehicleCosmeticsItemDefinition_Body",
        "FortVehicleCosmeticsItemDefinition_Booster",
        "FortVehicleCosmeticsItemDefinition_Wheel",
        "FortVehicleCosmeticsItemDefinition_DriftTrail",

        // LEGO
        "JunoBuildingPropAccountItemDefinition",
        "JunoBuildingSetAccountItemDefinition"
    ];

    private readonly Func<VfsEntry, bool> _shopAssetsFilter = x => x.NameWithoutExtension.StartsWith("DAv2") && !x.Name.Contains("MusicPass");
    private readonly Func<VfsEntry, bool> _cosmeticsFilter = (x => (
        x.Path.Contains("Athena/Items/Cosmetics/", StringComparison.OrdinalIgnoreCase) ||
        x.Path.Contains("GameFeatures/MeshCosmetics/", StringComparison.OrdinalIgnoreCase) /* Caper and Alias cosmetics */ ||
        x.Path.Contains("GameFeatures/CosmeticShoes/", StringComparison.OrdinalIgnoreCase) /* Fortnite Shoes */ ||
        
        x.Path.Contains("GameFeatures/SparksCosmetics/", StringComparison.OrdinalIgnoreCase) || 
        x.Path.Contains("GameFeatures/FM/SparksSongTemplates/", StringComparison.OrdinalIgnoreCase) ||
        x.Path.Contains("GameFeatures/FM/SparksCosmetics/", StringComparison.OrdinalIgnoreCase) ||
        x.Path.Contains("GameFeatures/FM/SparksCharacterCommon/", StringComparison.OrdinalIgnoreCase) || //default sparks cosmetics (issue #33)
        x.Path.Contains("GameFeatures/VehicleCosmetics/", StringComparison.OrdinalIgnoreCase) || 
        x.Path.Contains("GameFeatures/Juno/", StringComparison.OrdinalIgnoreCase)) &&
        Instance._accepted.Any(k => x.NameWithoutExtension.StartsWith(k, StringComparison.OrdinalIgnoreCase)));

    private readonly List<string> _athenaOptions = [ 
        "Profile Athena",
        "ItemShop Catalog"
    ];
    private readonly List<string> _profileOptions = [ 
        "All Cosmetics", "New Cosmetics", 
        "New Cosmetics (With Paks)",
        "Custom Cosmetics (by Id)",
        "Pak Cosmetics", "Paks Bulk", 
        "Back"
    ];
    private readonly List<string> _shopOptions = [
        "New Cosmetics", "New Cosmetics (With Paks)",
        "Custom Cosmetics (by Id)", "Pak Cosmetics", 
        "Paks Bulk", "Back"
    ];

    private readonly List<string> _notices = [];

    public async Task Initialize(List<string> notices)
    {
        _notices.AddRange(notices);
        _manifestService = new();
        Provider = new(string.Empty, new VersionContainer(EGame.GAME_UE5_LATEST), StringComparer.OrdinalIgnoreCase);

        Log.Information("Initializing required libraries.");
        await Helper.InitializeOodle(); /* lib required by CUE4Parse */
        await Helper.InitializeZlib(); /* lib required by EpicManifestParser */
        Log.Information("Initializated all libraries.");

        await CheckAuth();
        await LoadManifest();
        await MountArchives();
        await LoadMappings(); /* (may be broken on updates) */
        await LoadAesKeys();
    }

    // this need to be rewrited :skull:
    private static async Task CheckAuth()
    {
        if (!await APIEndpoints.Epic.IsAuthValid())
        {
            // this is pretty funny because we dont even check if the auth response is valid
            var auth = await APIEndpoints.Epic.CreateAuthAsync();
            Config.config.accessToken = auth?.access_token!;
            Config.Save();
        }
    }

    public async Task MountArchives()
    {
        Log.Information("Loading archives for {build} (CL {cl})", 
            _manifestService.GameBuild, _manifestService.CLVersion);

        _manifestService.LoadArchives();
        await Provider.MountAsync();
    }

    private async Task LoadManifest()
    {
        ManifestInfo? manifest = await APIEndpoints.Epic.GetManifestAsync();
        if (manifest is null)
        {
            Log.Error("The manifest response is invalid!");
            Helper.ExitThread(1);
            return;
        }

        await _manifestService.DownloadManifest(manifest);
        Log.Information("Downloaded manifest {id} ({ver})",
            _manifestService.ManifestId, _manifestService.GameBuild);
    }

    private async Task LoadMappings()
    {
        var mappings = await APIEndpoints.FNCentral.GetMappingsAsync() ?? DirectoryManager.GetSavedMappings();
        if (mappings is null)
        {
            Log.Warning("Invalid response from mappings API. No saved mappings have been found." +
                " The program may have issues!.");
            return;
        }

        Provider.MappingsContainer = new FileUsmapTypeMappingsProvider(mappings);
        Log.Information("Mappings loaded from {path}.", mappings.SubstringAfterLast('\\'));
    }

    private async Task LoadAesKeys()
    {
        var aesKeys = await APIEndpoints.FNCentral.GetAesKeysAsync();
        if (aesKeys is null || APIEndpoints.FNCentral.AesKey is null)
        {
            Log.Warning("AESKeys response is invalid: the program may not work as expected!");
            return;
        }

        LoadKey();
        LoadKeys();
        LoadAllEntries(
            x => x.EncryptionKeyGuid == _zeroGuid || 
            APIEndpoints.FNCentral.AesKey.DynamicKeys.Select(
            k => new FGuid(k.Guid)).Contains(x.EncryptionKeyGuid), true);
    }

    public async Task ShowMenu()
    {
        Console.Clear(); // clear the console or we duplicate the title
        Console.Title = $"Athena v{Globals.VERSION} - FortniteGame v{_manifestService.GameVersion}";
        DiscordRichPresence.Update($"In Menu - FortniteGame v{_manifestService.GameVersion}");
        // this menu is static, and will never be removed
        AnsiConsole.Write(new Markup($"[63]Athena v{Globals.VERSION}[/]: Made with [124]<3[/] by [99]@djlorenzouasset[/] & [99]@andredotuasset[/] with the help of [99]@unrealhybrid[/]\n"));
        AnsiConsole.Write(new Markup($"[63]Need to change your profiles directory?[/] Go in the [underline 99]%appdata%/Athena[/] folder and edit the [99]settings.json[/] file.\n"));
        AnsiConsole.Write(new Markup($"Join the [63]discord server[/] if you need help using Athena: [underline 99]{Globals.DISCORD}[/]\n\n"));

        foreach (var msg in _notices)
        {
            AnsiConsole.Write(new Markup(msg));
        }

        // main menu
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What do you want generate?")
                .AddChoices(_athenaOptions)
        );

        Model model = Model.ProfileAthena; // default
        switch (selected)
        {
        case "Profile Athena":
            break;
        case "ItemShop Catalog":
            model = Model.ItemShop;
            break;
        }

        Log.ForContext("NoConsole", true).Information("User selected {0}", model);
        await SelectMode(model);
    }

    private async Task SelectMode(Model model)
    {
        List<string> opts = [];

        switch (model)
        {
        case Model.ItemShop:
            opts = _shopOptions;
            _currentGenerationType = "ItemShop Catalog";
            break;

        case Model.ProfileAthena:
            opts = _profileOptions;
            _currentGenerationType = "Profile Athena";
            break;
        }

        var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title($"What type of [1]{_currentGenerationType}[/] do want you generate?")
            .AddChoices(opts)
        );

        Actions action = Actions.AddEverything;
        switch (selected)
        {
        case "All Cosmetics":
            action = Actions.AddEverything;
            break;
        case "New Cosmetics":
            action = Actions.AddNew;
            break;
        case "New Cosmetics (With Paks)":
            action = Actions.AddNewWithArchives;
            break;
        case "Pak Cosmetics":
            action = Actions.AddArchive;
            break;
        case "Paks Bulk":
            action = Actions.BulkArchive;
            break;
        case "Custom Cosmetics (by Id)":
            action = Actions.AddOnlySelected;
            break;
        case "Back":
            await ReturnToMenu(bRequest: false);
            return;
        }

        Log.ForContext("NoConsole", true).Information("User selected {0}", action);
        await ProcessRequest(model, action);
    }

    private async Task ProcessRequest(Model model, Actions action)
    {
        var start = Stopwatch.StartNew();
        DiscordRichPresence.Update($"Generating {_currentGenerationType}.");
        
        if (action == Actions.AddEverything) 
        {
            if (!await GenerateSelectedModel(_allEntries, model, action)) 
                return;
        }
        else if (action == Actions.AddNew)
        {
            await LoadBackupAsync();
            if (!await GenerateSelectedModel(_newEntries, model, action))
                return;
        }
        else if (action == Actions.AddNewWithArchives)
        {
            await LoadBackupAsync(true);
            if (!await GenerateSelectedModel(_newEntries, model, action))
                return;
        }
        else if (action == Actions.AddArchive)
        {
            if (APIEndpoints.FNCentral.AesKey.DynamicKeys.Count == 0)
            {
                Log.Error("There are no available paks to select.");
                await ReturnToMenu(true);
                return;
            }

            var selected = SelectArchive();

            _ioStoreNames.Add(selected.Name);
            LoadAllEntries(x => x.EncryptionKeyGuid == new FGuid(selected.Guid));

            if (!await GenerateSelectedModel(_newEntries, model, action))
                return;
        }
        else if (action == Actions.BulkArchive)
        {
            var dynamicKeys = BulkArchives();
            if (dynamicKeys.Count() == 0)
            {
                Log.Error("No paks found for the input you inserted.");
                await ReturnToMenu(true);
                return;
            }
            else
            {
                _ioStoreNames.AddRange(dynamicKeys.Select(x => x.Name)); // fill the array
                LoadAllEntries(x => dynamicKeys.Select(x => new FGuid(x.Guid)).Contains(x.EncryptionKeyGuid));

                if (!await GenerateSelectedModel(_newEntries, model, action))
                    return;
            }
        }
        else if (action == Actions.AddOnlySelected)
        {
            var assets = RequestSelectedItems(model);
            if (assets.Count() == 0)
            {
                Log.Error("No items found for the input you inserted.");
                await ReturnToMenu(true);
                return;
            }
            else
            {
                _itemsFilter.AddRange(assets.Select(x => x.ToLower()));
                LoadAllEntries(x => APIEndpoints.FNCentral.AesKey.DynamicKeys.Select(
                    k => new FGuid(k.Guid)).Contains(x.EncryptionKeyGuid) || x.EncryptionKeyGuid == _zeroGuid, isCustom: true);

                if (!await GenerateSelectedModel(_newEntries, model, action))
                    return;
            }
        }

        Log.Information("All tasks finished in {tot}s ({ms}ms)\n", start.Elapsed.Seconds, Math.Round(start.Elapsed.TotalMilliseconds));
        await ReturnToMenu();
        return;
    }

    private async Task<bool> GenerateSelectedModel(IEnumerable<VfsEntry> entries, Model model, Actions action)
    {
        int added = 0;
        string type;
        Func<VfsEntry, bool> filter;

        switch (model)
        {
        case Model.ProfileAthena:
            type = "cosmetics";
            filter = _cosmeticsFilter;
            break;
        case Model.ItemShop:
            type = "shop assets";
            filter = _shopAssetsFilter;
            break;
        default:
            Log.Error("An unknown error has occured.");
            await ReturnToMenu(true);
            return false;
        }

        entries = entries.Where(filter); // uses the function here for filter assets
        if (entries.Count() == 0)
        {
            if (action == Actions.AddNew) Log.Error("No new {type} found using {backup}.", type, _backupName);
            else if (action == Actions.AddEverything) Log.Error("No {type} found.", type);
            else if (action == Actions.AddArchive) Log.Error("No {type} found in {ioStoreNames}.", type, string.Join(", ", _ioStoreNames));
            else if (action == Actions.BulkArchive) Log.Error("No {type} found in {ioStoreNames}.", type, string.Join(", ", _ioStoreNames));
            else if (action == Actions.AddOnlySelected) Log.Error("No {type} found for the selected ids/DAv2s.", type);
            await ReturnToMenu(true);
            return false;
        }
        
        if (model == Model.ItemShop)
        {
            ShopBuilder shop = new();
            foreach (var entry in entries)
            {
                shop.AddCatalogEntry(entry.PathWithoutExtension);
                Log.Information("Added ShopAsset {name}.", entry.NameWithoutExtension);
                added++;
            }

            Log.Information("Building Shop with {tot} shopAssets.", added);
            string savePath;
            try
            {
                await File.WriteAllTextAsync(Path.Combine(Config.config.shopDirectory, "shop.json"), shop.Build());
                savePath = Config.config.shopDirectory;
            }
            catch (Exception err)
            {
                Log.Warning("An error has occurred while saving the shop: {err}. Saving in default directory (.profiles).", err.Message);
                await File.WriteAllTextAsync(Path.Combine(DirectoryManager.Profiles, "shop.json"), shop.Build());
                savePath = DirectoryManager.Profiles;
            }
            Log.Information("Saved shop for {name} in {path}.", Config.config.athenaProfileId, savePath);
        }
        else if (model == Model.ProfileAthena)
        {
            ProfileBuilder profile = new();
            foreach (var entry in entries)
            {
                try
                {
                    var export = await Provider.LoadPackageObjectAsync(entry.PathWithoutExtension + '.' + entry.NameWithoutExtension);
                    if (!_classes.Contains(export.ExportType)) continue; // this will prevent issues trust

                    var variants = Helper.GetAllVariants(export);
                    profile.AddCosmetic(entry.NameWithoutExtension, variants);
                    Log.Information("Added \"{name}\" ({type}) with {totVariants} channels variants.", entry.NameWithoutExtension, export.ExportType, variants.Count);
                    added++;
                }
                catch (Exception e)
                {
#if DEBUG
                    Log.Error("Skipped entry {name}: {err}.", entry.Name, e.Message);
#else
                    Log.ForContext("NoConsole", true).Error("Skipped entry {name}: {err}.", entry.Name, e.Message);
#endif
                }
            }

            Log.Information("Building Profile Athena with {tot} cosmetics.", added);
            string savePath;
            try
            {
                await File.WriteAllTextAsync(Path.Combine(Config.config.profileDirectory, "profile_athena.json"), profile.Build());
                savePath = Config.config.profileDirectory;
            }
            catch (Exception err)
            {
                Log.Warning("An error has occurred while saving the profile: {err}. Saving in default directory (.profiles).", err.Message);
                await File.WriteAllTextAsync(Path.Combine(DirectoryManager.Profiles, "profile_athena.json"), profile.Build());
                savePath = DirectoryManager.Profiles;
            }
            Log.Information("Saved Profile Athena for {name} in {path}.", Config.config.athenaProfileId, savePath);
        }

        return true;
    }

    private DynamicKey SelectArchive()
    {
        var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What [1]Pak[/] do you want generate?")
            .AddChoices(
                APIEndpoints.FNCentral.AesKey.DynamicKeys.Select(x => $"{x.Name} ({x.Size.Formatted})")
            )
        );
        return APIEndpoints.FNCentral.AesKey.DynamicKeys.Where(x => x.Name == selected.Split(" ")[0]).First();
    }

    private IEnumerable<DynamicKey> BulkArchives()
    {
        var paks = AnsiConsole.Ask<string>("Insert the [1]numbers[/] of the [1]Paks[/] you want generate separated by [1];[/] (ex: 1000; 1001):");
        var numbers = paks.Split(";").Select(x => x.Trim());
        return APIEndpoints.FNCentral.AesKey.DynamicKeys.Where(x => numbers.Contains(_pakNameRegex.Match(x.Name).Groups[1].ToString()));
    }

    private IEnumerable<string> RequestSelectedItems(Model model)
    {
        string type = model == Model.ProfileAthena ? "cosmetics ids" : "DAv2s";
        var names = AnsiConsole.Ask<string>($"Insert the [1]{type}[/] of the [1]items[/] you want generate separated by [62];[/]:");
        return names.Split(";").Select(x => x.Trim());
    }

    private async Task ReturnToMenu(bool bFromError = false, bool bRequest = true)
    {
        if (!bRequest)
        {
            // return to menu without asking
            await ShowMenu();
            return;
        }
        else if (bFromError && !AnsiConsole.Confirm("Do you want to try again?"))
            return;
        else if (!bFromError && !AnsiConsole.Confirm("Do you want to go back to menu?"))
            return;

        // clear arrays for a new generation
        _newEntries.Clear();
        _ioStoreNames.Clear();
        _itemsFilter.Clear();
        _backupName = string.Empty;
        _currentGenerationType = string.Empty;
        await ShowMenu();
    }

    private void LoadAllEntries(Func<IAesVfsReader, bool> readers, bool global = false, bool isCustom = false)
    {
        var start = Stopwatch.StartNew();

        int total = 0;
        foreach (var reader in Provider.MountedVfs.Where(readers))
        {
            foreach (var value in reader.Files.Values)
            {
                if (value is not VfsEntry entry || !entry.IsUePackage) 
                    continue;

                if (global) _allEntries.Add(entry); // used for filter new files
                else if (isCustom) // used for filter only selected assets
                {
                    if (!_itemsFilter.Contains(entry.NameWithoutExtension.ToLower()))
                        continue;
                    _newEntries.Add(entry);
                }
                else _newEntries.Add(entry);

                total++;
            }
        }

        start.Stop();
        Log.Information("Loaded {tot_assets} VfsEntries in {tot}s ({ms}ms).", total, start.Elapsed.Seconds, Math.Round(start.Elapsed.TotalMilliseconds));
    }

    private async Task<Backup> GetBackup()
    {
        var backups = await APIEndpoints.AthenaEndpoints.GetBackupAsync();
        if (backups is null)
        {
            Log.Error("Invalid response from Backups API.");
            Helper.ExitThread(1);
        }

        var backup = backups!.Last();
        /* save the files if not already saved in the PC */
        var file = Path.Combine(DirectoryManager.BackupsDir, backup.FileName);
        if (!File.Exists(file))
        {
            await APIEndpoints.AthenaEndpoints.DownloadFileAsync(backup.DownloadUrl, file);
            Log.Information("Downloaded {bkp}", backup.FileName);
        }

        return backup;
    }

    private async Task LoadBackupAsync(bool includeArchives = false)
    {
        Func<IAesVfsReader, bool> files = includeArchives 
            ? x => APIEndpoints.FNCentral.AesKey.DynamicKeys.Select(k => new FGuid(k.Guid)).Contains(x.EncryptionKeyGuid) || x.EncryptionKeyGuid == _zeroGuid 
            : x => x.EncryptionKeyGuid == _zeroGuid;

        var backup = await GetBackup();
        var start = Stopwatch.StartNew();

        var file = Path.Combine(DirectoryManager.BackupsDir, backup.FileName);
        await using var fileStream = new FileStream(file, FileMode.Open);
        await using var memoryStream = new MemoryStream();

        if (fileStream.Read<uint>() == _LZ4Magic)
        {
            fileStream.Position -= 4;
            using var compressionStream = LZ4Stream.Decode(fileStream);
            await compressionStream.CopyToAsync(memoryStream);
        }
        else await fileStream.CopyToAsync(memoryStream);

        memoryStream.Position = 0;
        await using var archive = new FStreamArchive(fileStream.Name, memoryStream);

        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var bkMagic = archive.Read<uint>();

        if (bkMagic == _backupMagic)
        {
            var version = archive.Read<EBackupVersion>();
            var count = archive.Read<int>();
            for (var i = 0; i < count; i++)
            {
                archive.Position += sizeof(long) + sizeof(byte);
                var fullPath = archive.ReadString();
                if (version < EBackupVersion.PerfectPath) fullPath = fullPath[1..];
                paths.Add(fullPath);
            }

            Log.Information("Parsed backup {bkp} in {tot}s (version: {ver}).", backup.FileName, start.Elapsed.Seconds, version);
        }
        else
        {
            archive.Position -= sizeof(uint);
            while (archive.Position < archive.Length)
            {
                archive.Position += 29;
                paths.Add(archive.ReadString().ToLower()[1..]);
                archive.Position += 4;
            }

            Log.Information("Parsed backup {bkp} in {tot}s.", backup.FileName, start.Elapsed.Seconds);
        }

        Log.Information("Comparing preloaded files with the selected backup.");
        foreach (var IReader in Provider.MountedVfs.Where(files))
        {
            foreach (var (key, value) in IReader.Files)
            {
                if (value is not VfsEntry entry || paths.Contains(key) || !entry.IsUePackage) 
                    continue;

                _newEntries.Add(entry);
            }
        }

        start.Stop();
        _backupName = backup.FileName.SubstringBeforeLast('.');
        Log.Information("Loaded {num} files using {bkp} in {tot}s", _newEntries.Count, backup.FileName, start.Elapsed.Seconds);
    }

    private void LoadKey() // this just load the main key
    {
        var aes = APIEndpoints.FNCentral.AesKey;
        Provider.SubmitKey(new FGuid(), new FAesKey(aes.MainKey));
    }

    private void LoadKeys() // load all dynamic keys
    {
        foreach (var key in APIEndpoints.FNCentral.AesKey.DynamicKeys)
        {
            LoadDynamicKey(key);
        }
    }

    private void LoadDynamicKey(DynamicKey dynamicKey) // load a single dynamic key
    {
        Provider.SubmitKey(new FGuid(dynamicKey.Guid), new FAesKey(dynamicKey.Key));
    }
}