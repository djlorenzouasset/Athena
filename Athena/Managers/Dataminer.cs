using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.VirtualFileSystem;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Readers;
using CUE4Parse.FileProvider.Objects;
using K4os.Compression.LZ4.Streams;
using GenericReader;
using Spectre.Console;
using RestSharp;
using Athena.Rest;
using Athena.Models;

namespace Athena.Managers;

public class Dataminer
{
    public ManifestDownloader manifest;
    public StreamedFileProvider provider;
    public List<VfsEntry> all = new();
    public List<VfsEntry> newEntries = new();
    // utils
    public string? backupName;
    public string? ioStoreName;
    // console options
    public string[] athenaOptions = new[] { "All Cosmetics", "New Cosmetics", "Pak Cosmetics" };
    public string[] shopOptions = new[] { "New Cosmetics", "Pak Cosmetics" };

    public Dataminer(string mappingFile)
    {
        provider = new("FortniteLive", true, new VersionContainer(EGame.GAME_UE5_LATEST));
        provider.MappingsContainer = new FileUsmapTypeMappingsProvider(mappingFile);
        manifest = new("http://epicgames-download1.akamaized.net/Builds/Fortnite/CloudDir/ChunksV4/");
    }

    public async Task LoadAllPaks(RestResponse manifestData)
    {
        await manifest.DownloadManifest(new(manifestData.Content));

        foreach (var file in manifest.ManifestFile.FileManifests)
        {
            if (!ManifestDownloader.PaksFinder.IsMatch(file.Name) || file.Name.Contains("optional")) continue;
            manifest.LoadFileManifest(file, ref provider);
        }
        provider.Mount();
        LoadKey();
        Log.Information("Loaded {totalKeys} Dynamic Keys.", Endpoints.FNCentral.AesKey.DynamicKeys.Count);
        Log.Information("Loading VfsEntries.");
        LoadAllEntries();
    }

    public async Task AskGeneration()
    {
        // clear the console from all the logs
        Console.Clear();

        AnsiConsole.Write(new Markup("[63]Athena[/]: Made by [99]@djlorenzouasset[/] with the help of [99]@andredotuasset[/]\n\n"));

        var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What do you want generate?")
            .PageSize(10)
            .AddChoices(new[]
            {
                "Profile-Athena",
                "ItemShop Catalog"
            })
        );

        ToDo toDo = ToDo.AthenaProfile;
        switch (selected)
        {
            case "Profile-Athena":
                break;
            case "ItemShop Catalog":
                toDo = ToDo.ShopGenerator;
                break;
        }

        await AskAction(toDo);
    }

    private async Task AskAction(ToDo toDo)
    {
        string type = toDo == ToDo.AthenaProfile ? "Profile Athena" : "ItemShop Catalog";
        string[] opts = toDo == ToDo.ShopGenerator ? shopOptions : athenaOptions;

        var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title($"What type of [62]{type}[/] do want you generate?")
            .PageSize(10)
            .AddChoices(opts)
        );

        Action action = Action.AddEverything;
        switch (selected)
        {
            case "All Cosmetics":
                action = Action.AddEverything;
                break;
            case "New Cosmetics":
                action = Action.AddNew;
                break;
            case "Pak Cosmetics":
                action = Action.AddArchive;
                break;
        }

        await WeLoveCats(toDo, action);
    }

    private async Task WeLoveCats(ToDo toDo, Action action)
    {
        if (action == Action.AddEverything)
        {
            LoadAllKeys();
            LoadAllEntries();
            GenerateModelFromEntries(all, action);
        }
        else if (action == Action.AddNew)
        {
            // load backup file (fmodel backup)
            // and save all entries in the array
            await LoadBackup();
            GenerateModelFromEntries(newEntries, action, toDo);
        }
        else if (action == Action.AddArchive)
        {
            var dynamicPak = AskForPak();
            LoadDynamicKey(dynamicPak);
            ioStoreName = dynamicPak.Name;

            // I should improve this step since is very slow :(
            Log.Information("Loading new VfsEntries and comparing preloaded files with {name}. This will take few seconds.", ioStoreName);
            LoadEntriesFromArchive(toDo == ToDo.ShopGenerator);
            GenerateModelFromEntries(newEntries, action, toDo);
        }
    }

    private DynamicKey AskForPak()
    {
        var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What [62]Pak[/] do you want generate?")
            .PageSize(10)
            .AddChoices(
                Endpoints.FNCentral.AesKey.DynamicKeys.Select(x => x.Name)
            )
        );

        return Endpoints.FNCentral.AesKey.DynamicKeys.Where(x => x.Name == selected).First();
    }

    private void GenerateModelFromEntries(List<VfsEntry> entries, Action action, ToDo toDo = ToDo.AthenaProfile)
    {
        int loaded = 0;
        string type = toDo == ToDo.AthenaProfile ? "cosmetics" : "shopAssets";

        if (type == "cosmetics")
        {
            entries = entries.Where(
                x => x.PathWithoutExtension.StartsWith("FortniteGame/Content/Athena/Items/Cosmetics") ||
                x.PathWithoutExtension.StartsWith("FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics")
            ).ToList();
        }
        else
        {
            entries = entries.Where(
                x => x.PathWithoutExtension.StartsWith("FortniteGame/Content/Catalog/NewDisplayAssets/") &&
                x.NameWithoutExtension.StartsWith("DAv2")
            ).ToList();
        }

        // start everything
        if (entries.Count() == 0)
        {
            if (action == Action.AddNew) Log.Error("No new {type} found using {backup}.", type, backupName);
            else if (action == Action.AddEverything) Log.Error("No cosmetics found.");
            else if (action == Action.AddArchive) Log.Error("No {type} found in {ioStoreName}.", type, ioStoreName);

            Console.ReadKey();
            Environment.Exit(0);
        }

        if (toDo == ToDo.ShopGenerator)
        {
            ShopBuilder shop = new();

            foreach (var entry in entries)
            {
                shop.AddCatalogEntry(entry.PathWithoutExtension);
                Log.Information("Added ShopAsset {name}", entry.NameWithoutExtension);
                loaded++;
            }

            Log.Information("Building Shop with {tot} shopAssets", loaded);
            File.WriteAllText(Path.Join(DirectoryManager.Profiles, "shop.json"), shop.Build());
            Log.Information("Saved shop for {name} in profiles folder", Config.config.athenaProfileId);
        }
        else
        {
            ProfileBuilder profile = new(entries.Count());

            foreach (var entry in entries)
            {
                try
                {
                    profile.OnCosmeticAdded(entry.NameWithoutExtension);
                    Log.Information("Added cosmetic \"{name}\".", entry.NameWithoutExtension);
                    loaded++;
                }
                catch
                {
#if DEBUG
                    Log.Error("Skipped entry {name}", entry.Name);
#endif
                }
            }

            // save the profile
            Log.Information("Building profile-athena with {tot} cosmetics.", loaded);
            File.WriteAllText(Path.Join(DirectoryManager.Profiles, "profile_athena.json"), profile.Build());
            Log.Information("Saved profile athena for {name} in profiles folder.", Config.config.athenaProfileId);
        }
    }

    private void LoadAllEntries()
    {
        foreach (var value in provider.Files.Values)
        {
            if (value is not VfsEntry entry || entry.Path.EndsWith(".uexp") || entry.Path.EndsWith(".ubulk") || entry.Path.EndsWith(".uptnl")) continue;
            all.Add(entry);
        }
        Log.Information("Loaded {tot} VfsEntries.", all.Count);
    }

    private void LoadEntriesFromArchive(bool dav2 = false)
    {
        List<GameFile> cat = new();

        if (!dav2)
        {
            cat = provider.Files.Values.Where(
                x => x.PathWithoutExtension.StartsWith("FortniteGame/Content/Athena/Items/Cosmetics") ||
                x.PathWithoutExtension.StartsWith("FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics")
            ).ToList();
        }
        else
        {
            cat = provider.Files.Values.Where(
                x => x.PathWithoutExtension.StartsWith("FortniteGame/Content/Catalog/NewDisplayAssets/") &&
                x.NameWithoutExtension.StartsWith("DAv2")
            ).ToList();
        }

        foreach (var value in cat)
        {
            if (value is not VfsEntry entry) continue;
            else if (all.Contains(entry)) continue;
            newEntries.Add(entry);
        }
    }

    // Backup loading from FModel
    private async Task LoadBackup()
    {
        var backupRequest = await Endpoints.GetBackupAsync();
        var bk = backupRequest?.LastOrDefault();

        if (bk?.FileName != null && !File.Exists(Path.Combine(DirectoryManager.BackupsDir, bk.FileName)))
        {
            await Endpoints.DownloadFileAsync(bk.DownloadUrl, Path.Combine(DirectoryManager.BackupsDir, bk.FileName));
            Log.Information("Backup downloaded as {name}", bk?.FileName);
        }
        else
        {
            Log.Information("Backup pulled form local file {name}", bk?.FileName);
        }

        backupName = bk?.FileName;
        Log.Information("Comparing preloaded files and new files with backup file {name}", backupName);

        await using FileStream fileStream = new FileStream(Path.Combine(DirectoryManager.BackupsDir, bk.FileName), FileMode.Open);
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

        foreach (var (key, value) in provider.Files)
        {
            if (value is not VfsEntry entry || path.ContainsKey(key) || entry.Path.EndsWith(".uexp") || entry.Path.EndsWith(".ubulk") || entry.Path.EndsWith(".uptnl"))
                continue;
            newEntries.Add(entry);
        }
        Log.Information("Loaded {tot} new VfsEntries", newEntries.Count);
    }

    private void LoadAllKeys()
    {
        foreach (var key in Endpoints.FNCentral.AesKey.DynamicKeys)
        {
            LoadDynamicKey(key);
        }
    }

    private void LoadDynamicKey(DynamicKey dynamicKey)
    {
        provider.SubmitKey(new FGuid(dynamicKey.Guid), new FAesKey(dynamicKey.Key));
    }

    private void LoadKey()
    {
        var aes = Endpoints.FNCentral.AesKey;
        provider.SubmitKey(new FGuid(), new FAesKey(aes.MainKey));
    }
}