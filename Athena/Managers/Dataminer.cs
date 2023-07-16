using CUE4Parse.FileProvider;
using CUE4Parse.MappingsProvider;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse.UE4.Versions;
using CUE4Parse.UE4.VirtualFileSystem;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Readers;
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

    public string? backupName;
    public string? ioStoreName;

    public Dataminer(string mappingFile)
    {
        provider = new("FortniteLive", true, new VersionContainer(EGame.GAME_UE5_3, ETexturePlatform.DesktopMobile));
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

    public async Task AskAction()
    {
        // clear the console from all the logs
        Console.Clear();

        var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What type of [62]Profile Athena[/] want you generate?")
            .PageSize(10)
            .AddChoices(new[]
            {
                "All Cosmetics",
                "New Cosmetics",
                "Pak Cosmetics"
            }
            )
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

        await BrenLovesMe(action);
    }

    // bren_v2 cameo (twitter.com/Bren_v2)
    private async Task BrenLovesMe(Action action)
    {
        if (action == Action.AddEverything)
        {
            LoadAllKeys();
            LoadAllEntries();
            GenerateFromVfsEntries(all, action);
        }
        else if (action == Action.AddNew)
        {
            // load backup file (fmodel backup)
            // and save all entries in the array
            await LoadBackup();
            GenerateFromVfsEntries(newEntries, action);
        }
        else if (action == Action.AddArchive)
        {
            var dynamicPak = AskForPak();
            LoadDynamicKey(dynamicPak);
            ioStoreName = dynamicPak.Name;

            // filter only cosmetics for make the program more fast
            var cosmetics = provider.Files.Values.Where(
                x => x.PathWithoutExtension.StartsWith("FortniteGame/Content/Athena/Items/Cosmetics") ||
                x.PathWithoutExtension.StartsWith("FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics")
            );

            Log.Information("Loading new VfsEntries comparing preloaded files with {name}. This will take few seconds.", ioStoreName);
            foreach (var value in cosmetics)
            {
                if (value is not VfsEntry entry) continue;
                else if (all.Contains(entry)) continue;
                newEntries.Add(entry);
            }
            GenerateFromVfsEntries(newEntries, action);
        }
    }

    private DynamicKey AskForPak()
    {
        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("What [62]Pak[/] want you generate?")
                .PageSize(10)
                .AddChoices(
                    Endpoints.FNCentral.AesKey.DynamicKeys.Select(x => x.Name)
                )
        );

        return Endpoints.FNCentral.AesKey.DynamicKeys.Where(x => x.Name == selected).First();
    }

    private void GenerateFromVfsEntries(List<VfsEntry> entries, Action action)
    {
        int loaded = 0;

        // load all cosmetics in an Array
        var cosmetics = entries.Where(
            x => x.PathWithoutExtension.StartsWith("FortniteGame/Content/Athena/Items/Cosmetics") || 
            x.PathWithoutExtension.StartsWith("FortniteGame/Plugins/GameFeatures/BRCosmetics/Content/Athena/Items/Cosmetics")
        );

        if (cosmetics.Count() == 0)
        {
            if (action == Action.AddNew) Log.Error("No new cosmetics found using {backup}.", backupName);
            else if (action == Action.AddEverything) Log.Error("No cosmetics found.");
            else if (action == Action.AddArchive) Log.Error("No cosmetics found in {ioStoreName}.", ioStoreName);

            Console.ReadKey();
            Environment.Exit(0);
        }

        ProfileBuilder profile = new ProfileBuilder(cosmetics.Count());
        foreach (var cosmetic in cosmetics)
        {
            try
            {
                profile.OnCosmeticAdded(cosmetic.NameWithoutExtension);
                Log.Information("Added VfsEntry \"{name}\"", cosmetic.Name);
                loaded += 1;
            }
            catch
            {
                #if DEBUG
                Log.Error("Skipped VfsEntry \"{name}\"", cosmetic.Name);
                #endif
            }
        }

        // save the profile
        Log.Information("Building profile-athena with {tot} cosmetics.", loaded);
        File.WriteAllText(Path.Join(DirectoryManager.Profiles, "profile_athena.json"), profile.Build());
        Log.Information("Saved profile athena for {name} in profiles folder.", Config.config.athenaProfileId);
    }

    private void LoadAllEntries()
    {
        foreach (var value in provider.Files.Values)
        {
            if (value is not VfsEntry entry) continue;
            all.Add(entry);
        }
        Log.Information("Loaded {tot} VfsEntries.", all.Count);
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

    // backup loading from FModel
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
}