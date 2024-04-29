using System.Diagnostics;
using System.Text.RegularExpressions;
using RestSharp;
using GenericReader;
using Spectre.Console;
using K4os.Compression.LZ4.Streams;
using CUE4Parse.UE4.Readers;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.VirtualFileSystem;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Objects.Core.i18N;
using CUE4Parse.GameTypes.FN.Enums;
using CUE4Parse.MappingsProvider;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using Athena.Rest;
using Athena.Models;
using Athena.Services;

namespace Athena.Managers;

public class Dataminer
{
    private readonly FGuid zeroGuid = new FGuid(0, 0, 0, 0); // this is just for dont load dynamic keys
    private readonly Regex pakNameRegex = new(@"^pakchunk(\d*)-WindowsClient.utoc"); // we use this just for the bulk function

    private StreamedFileProvider provider;
    private ManifestDownloader manifest;
    private List<VfsEntry> all = new();
    private List<VfsEntry> newEntries = new();
    private List<string> ioStoreNames = new();
    private List<string> itemsFilter = new();
    private string backupName = string.Empty;
    private string currentGenerationType = string.Empty;

    private Func<VfsEntry, bool> cosmeticsFilter = (
        x => x.PathWithoutExtension.StartsWith("FortniteGame/Content/Athena/Items/Cosmetics", StringComparison.OrdinalIgnoreCase) || /* epic sometimes do funny things */
        x.PathWithoutExtension.StartsWith("FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics", StringComparison.OrdinalIgnoreCase) ||
        x.PathWithoutExtension.StartsWith("FortniteGame/Plugins/GameFeatures/MeshCosmetics/Content") /* THIS DIRECTORY IS BECAUSE CAPER & ALIAS ARE THE ONLY 2 SKINS HERE */ ||
        ((x.PathWithoutExtension.Contains("SparksCosmetics") || x.PathWithoutExtension.Contains("SparksSongTemplates")) &&
        (x.NameWithoutExtension.StartsWith("Sparks_") || x.NameWithoutExtension.StartsWith("SID_") || x.NameWithoutExtension.StartsWith("SparksAura_"))) ||
        (x.PathWithoutExtension.Contains("VehicleCosmetics") && x.NameWithoutExtension.StartsWith("ID_")));

    private Func<VfsEntry, bool> shopAssetsFilter = (
        x => x.PathWithoutExtension.StartsWith("FortniteGame/Content/Catalog/NewDisplayAssets/") &&
        x.NameWithoutExtension.StartsWith("DAv2"));

    private Func<VfsEntry, bool> weaponsFilter = x => x.NameWithoutExtension.StartsWith("WID_", StringComparison.OrdinalIgnoreCase);

    private readonly string[] athenaOptions = [ 
        "Profile Athena", 
        "ItemShop Catalog",
        "Weapons Dump"
    ];
    private readonly string[] profileOptions = [ 
        "All Cosmetics", "New Cosmetics", 
        "New Cosmetics (With Paks)",
        "Custom Cosmetics (by Id)",
        "Pak Cosmetics", "Paks Bulk", 
        "Back"
    ];
    private readonly string[] shopOptions = [
        "New Cosmetics", "New Cosmetics (With Paks)",
        "Custom Cosmetics (by Id)", "Pak Cosmetics", 
        "Paks Bulk", "Back"
    ];
    private readonly string[] weaponsOptions = [
        "All Weapons", "New Weapons", "Back"
    ];

    public Dataminer(string mappingFile)
    {
        provider = new("FortniteLive", true, new VersionContainer(EGame.GAME_UE5_4));
        provider.MappingsContainer = new FileUsmapTypeMappingsProvider(mappingFile);
        manifest = new("http://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/ChunksV4/");
    }

    public async Task LoadAllPaksAsync(RestResponse manifestData)
    {
        await manifest.DownloadManifest(new(manifestData.Content));
        Log.Information("Downloading manifest for {build}.", manifest.ManifestFile.BuildVersion);

        foreach (var file in manifest.ManifestFile.FileManifests)
        {
            if (!ManifestDownloader.PaksFinder.IsMatch(file.Name) || file.Name.Contains("optional")) continue;
            manifest.LoadFileManifest(file, ref provider);
        }

        await provider.MountAsync();
        LoadKey(); // load the main key if available
        LoadAllKeys();
        LoadAllEntries( // load only normal files, not paks
            x => x.EncryptionKeyGuid == zeroGuid || Endpoints.FNCentral.AesKey.DynamicKeys.Select(
            k => new FGuid(k.Guid)).Contains(x.EncryptionKeyGuid), true);

        provider.LoadLocalization();
    }

    public async Task ShowMenu() /* this just show the menu */ 
    {
        Console.Clear();
        Console.Title = $"Athena v{Globals.VERSION}";
        DiscordRichPresence.Update($"In Menu - {all.Count:###,###,###} Loaded Assets.");
        // this menu is static, and will never be removed
        AnsiConsole.Write(new Markup($"[63]Athena v{Globals.VERSION}[/]: Made with [124]<3[/] by [99]@djlorenzouasset[/] & [99]@andredotuasset[/] with the help of [99]@unrealhybrid[/]\n"));
        AnsiConsole.Write(new Markup($"[63]Need to change your profiles directory?[/] Go in the [99]%appdata%/Athena[/] folder and edit the [99]settings.json[/] file.\n"));
        AnsiConsole.Write(new Markup($"Join the [63]discord server[/]: [99]{Globals.DISCORD}[/]\n\n"));

        // main menu
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What do you want generate?")
                .PageSize(10)
                .AddChoices(athenaOptions)
            );

        Model model = Model.ProfileAthena; // default
        switch (selected)
        {
            case "Profile Athena":
                break;
            case "ItemShop Catalog":
                model = Model.ItemShop;
                break;
            case "Weapons Dump":
                model = Model.WeaponsDump;
                break;
        }
        // andre is stupid, and we know it
        await SelectMode(model);
    }

    private async Task SelectMode(Model model)
    {
        string[] opts = [];

        switch (model)
        {
            case Model.ItemShop:
                opts = shopOptions;
                currentGenerationType = "ItemShop Catalog";
                break;

            case Model.ProfileAthena:
                opts = profileOptions;
                currentGenerationType = "Profile Athena";
                break;

            case Model.WeaponsDump:
                opts = weaponsOptions;
                currentGenerationType = "Weapons Dump";
                break;
        }

        var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title($"What type of [62]{currentGenerationType}[/] do want you generate?")
            .PageSize(10)
            .AddChoices(opts)
        );

        Actions action = Actions.AddEverything;
        switch (selected)
        {
            case "All Cosmetics":
            case "All Weapons":
                action = Actions.AddEverything;
                break;
            case "New Cosmetics":
            case "New Weapons":
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
                // return to menu (hope it works fine)
                await ReturnToMenu(includeRequest: false);
                return;
        }

        await ProcessRequest(model, action);
    }

    private async Task ProcessRequest(Model model, Actions action)
    {
        var timer = new Stopwatch(); // timer?
        DiscordRichPresence.Update($"Generating {currentGenerationType}.");
        timer.Start();
        
        if (action == Actions.AddEverything) 
        {
            if (!await GenerateSelectedModel(all, model, action)) 
                return;
        }
        else if (action == Actions.AddNew)
        {
            await LoadBackupAsync();
            if (!await GenerateSelectedModel(newEntries, model, action))
                return;
        }
        else if (action == Actions.AddNewWithArchives)
        {
            await LoadBackupAsync(true);
            if (!await GenerateSelectedModel(newEntries, model, action))
                return;
        }
        else if (action == Actions.AddArchive)
        {
            var selected = SelectArchive();
            LoadAllEntries(x => x.EncryptionKeyGuid == new FGuid(selected.Guid));

            if (!await GenerateSelectedModel(newEntries, model, action))
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
                ioStoreNames.AddRange(dynamicKeys.Select(x => x.Name)); // fill the array
                LoadAllEntries(x => dynamicKeys.Select(x => new FGuid(x.Guid)).Contains(x.EncryptionKeyGuid));

                if (!await GenerateSelectedModel(newEntries, model, action))
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
                itemsFilter.AddRange(assets);
                LoadAllEntries(x => Endpoints.FNCentral.AesKey.DynamicKeys.Select(
                    k => new FGuid(k.Guid)).Contains(x.EncryptionKeyGuid) || x.EncryptionKeyGuid == zeroGuid, isCustom: true);

                if (!await GenerateSelectedModel(newEntries, model, action))
                    return;
            }
        }

        timer.Stop();
        Log.Information("All tasks finished in {tot}ms\n", Math.Round(timer.Elapsed.TotalSeconds, 2));
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
                filter = cosmeticsFilter;
                break;
            case Model.ItemShop:
                type = "shop assets";
                filter = shopAssetsFilter;
                break;
            case Model.WeaponsDump:
                type = "weapons";
                filter = weaponsFilter;
                break;
            default:
                Log.Error("An unknown error has occured.");
                await ReturnToMenu(true);
                return false;
        }

        entries = entries.Where(filter); // uses the function here for filter assets
        if (entries.Count() == 0)
        {
            if (action == Actions.AddNew) Log.Error("No new {type} found using {backup}.", type, backupName);
            else if (action == Actions.AddEverything) Log.Error("No {type} found.", type);
            else if (action == Actions.AddArchive) Log.Error("No {type} found in {ioStoreNames}.", type, string.Join(", ", ioStoreNames));
            else if (action == Actions.BulkArchive) Log.Error("No {type} found in {ioStoreNames}.", type, string.Join(", ", ioStoreNames));
            await ReturnToMenu(true);
            return false;
        }
        
        if (model == Model.ItemShop)
        {
            ShopBuilder shop = new();
            foreach (var entry in entries)
            {
                shop.AddCatalogEntry(entry.PathWithoutExtension);
                Log.Information("Added ShopAsset {name}", entry.NameWithoutExtension);
                added++;
            }

            Log.Information("Building Shop with {tot} shopAssets", added);
            string savePath;
            try
            {
                await File.WriteAllTextAsync(Path.Join(Config.config.shopDirectory, "shop.json"), shop.Build());
                savePath = Config.config.shopDirectory;
            }
            catch (Exception err) // sometimes the path wont accept characters like . or -
            {
                Log.Warning("An error has occurred while saving the shop: {err}. Saving in default directory (.profiles).", err.Message);
                await File.WriteAllTextAsync(Path.Join(DirectoryManager.Profiles, "shop.json"), shop.Build());
                savePath = DirectoryManager.Profiles;
            }
            Log.Information("Saved shop for {name} in {path}.", Config.config.athenaProfileId, Config.config.shopDirectory);
        }
        else if (model == Model.ProfileAthena)
        {
            ProfileBuilder profile = new();
            foreach (var entry in entries)
            {
                try
                {
                    var exports = await provider.LoadObjectAsync(entry.PathWithoutExtension + '.' + entry.NameWithoutExtension);
                    var variants = Helper.GetAllVariants(exports);
                    profile.AddCosmetic(entry.NameWithoutExtension, variants);
                    Log.Information("Added \"{name}\" with {totVariants} channels variants.", entry.NameWithoutExtension, variants.Count);
                    added++;
                }
                catch (Exception e)
                {
#if DEBUG
                    Log.Error("Skipped entry {name} for {err}.", entry.Name, e.Message);
#endif
                }
            }

            Log.Information("Building Profile Athena with {tot} cosmetics.", added);
            string savePath;
            try
            {
                await File.WriteAllTextAsync(Path.Join(Config.config.profileDirectory, "profile_athena.json"), profile.Build());
                savePath = Config.config.profileDirectory;
            }
            catch (Exception err) // sometimes the path wont accept characters like . or -
            {
                Log.Warning("An error has occurred while saving the profile: {err}. Saving in default directory (.profiles).", err.Message);
                await File.WriteAllTextAsync(Path.Join(DirectoryManager.Profiles, "profile_athena.json"), profile.Build());
                savePath = DirectoryManager.Profiles;
            }
            Log.Information("Saved Profile Athena for {name} in {path}.", Config.config.athenaProfileId, savePath);
        }
        else
        {
            List<string> weapons = new();
            string currentVer = manifest!.ManifestFile!.Version.ToString();
            weapons.Add($"WEAPONS DUMP - {currentVer}\n\n");

            foreach (var entry in entries)
            {
                try
                {
                    var wid = await provider.LoadObjectAsync(entry.PathWithoutExtension + '.' + entry.NameWithoutExtension);
                    if (!(wid.ExportType.Equals("FortWeaponRangedItemDefinition") || 
                        wid.ExportType.Equals("FortWeaponMeleeItemDefinition") || 
                        wid.ExportType.Equals("AthenaGadgetItemDefinition"))) continue;

                    string weaponName = wid.GetOrDefault("ItemName", new FText("NONE")).Text.TrimEnd();
                    string weaponRarity = ParseRarity(wid.GetOrDefault("Rarity", EFortRarity.Common));
                    string weaponId = wid.Name;

                    weapons.Add($"[{weaponName} : {weaponRarity}] {weaponId}");
                    added++;
                    Log.Information("Added [{name} : {rarity}] {id} of type {type}.", weaponName, weaponRarity, weaponId, wid.ExportType);
                }
                catch (Exception e)
                {
#if DEBUG
                    Log.Error("Skipped entry {name} for {err}.", entry.Name, e.Message);
#endif
                }
            }

            Log.Information("Saving Weapons dump with {tot} weapons.", added);
            await File.WriteAllLinesAsync(Path.Join(DirectoryManager.Current, $"weapons_dump_{currentVer}.txt"), weapons);
            Log.Information("Saved Weapons Dump in {path}.", DirectoryManager.Current);
        }

        return true;
    }

    private DynamicKey SelectArchive()
    {
        var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What [62]Pak[/] do you want generate?")
            .PageSize(10)
            .AddChoices(
                Endpoints.FNCentral.AesKey.DynamicKeys.Select(x => $"{x.Name} ({x.Size.Formatted})")
            )
        );
        return Endpoints.FNCentral.AesKey.DynamicKeys.Where(x => x.Name == selected.Split(" ")[0]).First();
    }

    private IEnumerable<DynamicKey> BulkArchives()
    {
        var paks = AnsiConsole.Ask<string>("Insert the [62]numbers[/] of the [62]Paks[/] you want generate separated by [62];[/] (ex: 1000; 1001):");
        var numbers = paks.Split(";").Select(x => x.Trim());
        return Endpoints.FNCentral.AesKey.DynamicKeys.Where(
            x => numbers.Contains(pakNameRegex.Match(x.Name).Groups[1].ToString()));
    }

    private IEnumerable<string> RequestSelectedItems(Model model)
    {
        string type = model == Model.ProfileAthena ? "cosmetics ids" : "DAv2s";
        var names = AnsiConsole.Ask<string>($"Insert the [62]{type}[/] of the [62]items[/] you want generate separated by [62];[/]:");
        return names.Split(";").Select(x => x.Trim());
    }

    private async Task ReturnToMenu(bool fromError = false, bool includeRequest = true)
    {
        if (!includeRequest)
        {
            // return to menu without asking
            await ShowMenu();
            return;
        }
        else if (fromError && !AnsiConsole.Confirm("Do you want to try again?"))
            return;
        else if (!fromError && !AnsiConsole.Confirm("Do you want to go back to menu?"))
            return;

        // clear arrays for a new generation
        newEntries.Clear();
        ioStoreNames.Clear();
        itemsFilter.Clear();
        backupName = string.Empty;
        currentGenerationType = string.Empty;
        await ShowMenu();
    }

    private void LoadAllEntries(Func<IAesVfsReader, bool> readers, bool global = false, bool isCustom = false)
    {
        var timer = new Stopwatch();
        int total = 0;
        timer.Start();

        foreach (var reader in provider.MountedVfs.Where(readers))
        {
            foreach (var value in reader.Files.Values)
            {
                if (value is not VfsEntry entry || entry.Path.EndsWith(".uexp") || 
                    entry.Path.EndsWith(".ubulk") || entry.Path.EndsWith(".uptnl")) continue;

                if (global) all.Add(entry); // used for filter new files
                else if (isCustom) // used for filter only selected assets
                {
                    if (!itemsFilter.Contains(entry.NameWithoutExtension))
                        continue;
                    newEntries.Add(entry);
                }
                else newEntries.Add(entry); // used for load archives
                total++;
            }
        }
        timer.Stop();
        Log.Information("Loaded {tot} VfsEntries in {s}ms.", total, Math.Round(timer.Elapsed.TotalSeconds, 2));
    }

    private async Task LoadBackupAsync(bool includeArchives = false)
    {
        Func<IAesVfsReader, bool> files = includeArchives ?
            x => Endpoints.FNCentral.AesKey.DynamicKeys.Select(k => new FGuid(k.Guid)).Contains(x.EncryptionKeyGuid) || x.EncryptionKeyGuid == zeroGuid :
            x => x.EncryptionKeyGuid == zeroGuid;

        var timer = new Stopwatch();
        var backupRequest = await Endpoints.AthenaEndpoints.GetBackupAsync();
        var bk = backupRequest.Last();

        if (bk.FileName != null && !File.Exists(Path.Combine(DirectoryManager.BackupsDir, bk.FileName)))
        {
            await Endpoints.AthenaEndpoints.DownloadFileAsync(bk.DownloadUrl, Path.Combine(DirectoryManager.BackupsDir, bk.FileName));
            Log.Information("Backup downloaded as {name}", bk.FileName);
        }

        backupName = bk.FileName;
        Log.Information("Comparing preloaded files and new files with backup file {name}", backupName);
        timer.Start();

        await using FileStream fileStream = new FileStream(Path.Combine(DirectoryManager.BackupsDir, backupName), FileMode.Open);
        await using MemoryStream memoryStream = new MemoryStream();
        var reader = new GenericStreamReader(fileStream);

        if (reader.Read<uint>() == 0x184D2204u)
        {
            fileStream.Position -= 4;
            await using LZ4DecoderStream compression = LZ4Stream.Decode(fileStream);
            await compression.CopyToAsync(memoryStream).ConfigureAwait(false);
        }
        else
        {
            await fileStream.CopyToAsync(memoryStream).ConfigureAwait(false);
        }

        memoryStream.Position = 0;
        await using FStreamArchive archive = new FStreamArchive(fileStream.Name, memoryStream);
        var path = new Dictionary<string, int>();

        while (archive.Position < archive.Length)
        {
            archive.Position += 29;
            path[archive.ReadString().ToLower()[1..]] = 0;
            archive.Position += 4;
        }

        foreach (var IReader in provider.MountedVfs.Where(files))
        {
            foreach (var (key, value) in IReader.Files)
            {
                if (value is not VfsEntry entry || path.ContainsKey(key) || entry.Path.EndsWith(".uexp") ||
                    entry.Path.EndsWith(".ubulk") || entry.Path.EndsWith(".uptnl")) continue;

                newEntries.Add(entry); // load entries
            }
        }
        timer.Stop();
        Log.Information("Loaded {tot} new VfsEntries in {s}ms", newEntries.Count, Math.Round(timer.Elapsed.TotalSeconds, 2));
    }

    private string ParseRarity(EFortRarity rarity)
    {

        string r = rarity.GetNameText().Text;
        if (provider.LocalizedResources.ContainsKey("Fort.Rarity"))
        {
            return provider.LocalizedResources["Fort.Rarity"][r];
        }
        return "Common";
    }

    private void LoadKey() // this just load the main key
    {
        var aes = Endpoints.FNCentral.AesKey;
        provider.SubmitKey(new FGuid(), new FAesKey(aes.MainKey));
    }

    private void LoadAllKeys() // load all dynamic keys
    {
        foreach (var key in Endpoints.FNCentral.AesKey.DynamicKeys)
        {
            LoadDynamicKey(key);
        }
    }

    private void LoadDynamicKey(DynamicKey dynamicKey) // load a single dynamic key
    {
        provider.SubmitKey(new FGuid(dynamicKey.Guid), new FAesKey(dynamicKey.Key));
    }
}