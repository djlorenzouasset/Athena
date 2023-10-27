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
using CUE4Parse.FileProvider;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.MappingsProvider;
using Athena.Rest;
using Athena.Models;
using Athena.Services;

namespace Athena.Managers;

public class Dataminer
{
    private StreamedFileProvider provider;
    private ManifestDownloader manifest;
    private List<VfsEntry> all = new();
    private List<VfsEntry> newEntries = new();
    // these are just used for global logs and function arguments
    private List<string> ioStoreNames = new();
    private string backupName = string.Empty;

    private readonly FGuid zeroGuid = new FGuid(0, 0, 0, 0); // this is just for dont load dynamic keys
    private readonly Regex pakNameRegex = new(@"^pakchunk(\d*)-WindowsClient.utoc"); // we use this just for the bulk function

    private readonly string[] athenaOptions = new[] { 
        "Profile Athena", 
        "ItemShop Catalog"
    };
    private readonly string[] profileOptions = new[] { 
        "All Cosmetics", "New Cosmetics", 
        "New Cosmetics (With Paks)", 
        "Pak Cosmetics", "Paks Bulk", "Back"
    };
    private readonly string[] shopOptions = new[] { 
        "New Cosmetics", "New Cosmetics (With Paks)", 
        "Pak Cosmetics", "Paks Bulk", "Back"
    };

    public Dataminer(string mappingFile)
    {
        provider = new("FortniteLive", true, new VersionContainer(EGame.GAME_UE5_LATEST));
        provider.MappingsContainer = new FileUsmapTypeMappingsProvider(mappingFile);
        manifest = new("http://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/ChunksV4/");
    }

    public async Task LoadAllPaksAsync(RestResponse manifestData)
    {
        await manifest.DownloadManifest(new(manifestData.Content));
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
    }

    public async Task ShowMenu() // this just show the menu :/ 
    {
        Console.Clear();
        Console.Title = $"Athena v{Globals.VERSION}";
        DiscordRichPresence.Update($"In Menu - {all.Count:###,###,###} Loaded Assets.");
        // credits (andre can be removed """"""""")
        AnsiConsole.Write(new Markup($"[63]Athena v{Globals.VERSION}[/]: Made with [124]<3[/] by [99]@djlorenzouasset[/] & [99]@andredotuasset[/] with the help of [99]@unrealhybrid[/]\n"));
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
        }
        // andre is stupid, and we know it
        await SelectMode(model);
    }

    private async Task SelectMode(Model model)
    {
        string[] opts = model == Model.ItemShop ?
            shopOptions : profileOptions;
        string type = model == Model.ProfileAthena ? 
            "Profile Athena" : "ItemShop Catalog";

        var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title($"What type of [62]{type}[/] do want you generate?")
            .PageSize(10)
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
            /*
            case "Add Selected": UPCOMING
                break;
            */
            case "Back":
                // return to menu (hope it works fine)
                await ReturnToMenu(includeRequest: false);
                return;
        }

        await WeLoveCats(model, action);
    }

    private async Task WeLoveCats(Model model, Actions action) // yes ignore the name of this function (andre is a cat)
    {
        var timer = new Stopwatch(); // timer??
        string type = model == Model.ProfileAthena ? 
            "Profile Athena" : "ItemShop Catalog";

        DiscordRichPresence.Update($"Generating {type}.");
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
        /*
        else if (action == Actions.AddOnlySelected)
        {
            UPCOMING: FUNCTION CODE HERE
            return;
        }
        */

        timer.Stop();
        Log.Information("All tasks finished in {tot}ms\n", Math.Round(timer.Elapsed.TotalSeconds, 2));
        await ReturnToMenu();
        return;
    }

    private async Task<bool> GenerateSelectedModel(IEnumerable<VfsEntry> entries, Model model, Actions action)
    {
        int added = 0;
        string type = model == Model.ProfileAthena ? "cosmetics" : "shop assets";

        Func<VfsEntry, bool> finder = model == Model.ProfileAthena
            // cosmetics for the profile
            ? x => x.PathWithoutExtension.StartsWith("FortniteGame/Content/Athena/Items/Cosmetics") ||
              x.PathWithoutExtension.StartsWith("FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics") 
            // itemshop assets
            : x => x.PathWithoutExtension.StartsWith("FortniteGame/Content/Catalog/NewDisplayAssets/") &&
              x.NameWithoutExtension.StartsWith("DAv2");

        entries = entries.Where(finder); // uses the function here for filter assets

        if (entries.Count() == 0)
        {
            if (action == Actions.AddNew) Log.Error("No new {type} found using {backup}.", type, backupName);
            else if (action == Actions.AddEverything) Log.Error("No cosmetics found.");
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
            try
            {
                File.WriteAllText(Path.Join(Config.config.shopDirectory, "shop.json"), shop.Build());
            }
            catch (Exception err) // sometimes the path wont accept characters like . or -
            {
                Log.Warning("An error has occurred while saving the shop: {err}. Saving in default directory (.profiles).", err.Message);
                File.WriteAllText(Path.Join(DirectoryManager.Profiles, "shop.json"), shop.Build());
            }
            Log.Information("Saved shop for {name}.", Config.config.athenaProfileId);
        }
        else
        {
            ProfileBuilder profile = new();
            foreach (var entry in entries)
            {
                try
                {
                    var exports = provider.LoadAllObjects(entry.Path);
                    var variants = Helper.GetAllVariants(exports.First());
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
            try
            {
                File.WriteAllText(Path.Join(Config.config.shopDirectory, "profile_athena.json"), profile.Build());
            }
            catch (Exception err) // sometimes the path wont accept characters like . or -
            {
                Log.Warning("An error has occurred while saving the profile: {err}. Saving in default directory (.profiles).", err.Message);
                File.WriteAllText(Path.Join(DirectoryManager.Profiles, "profile_athena.json"), profile.Build());
            }
            Log.Information("Saved Profile Athena for {name}.", Config.config.athenaProfileId);
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
        await ShowMenu();
    }

    private void LoadAllEntries(Func<IAesVfsReader, bool> readers, bool global = false)
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