using System.Diagnostics;
using Spectre.Console;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.VirtualFileSystem;
using Athena.Utils;
using Athena.Builders;
using Athena.Extensions;
using Athena.Models.API.Responses;

namespace Athena.Core;

public class Generator
{
    private static readonly HashSet<string> _directoriesToFilter =
    [
        "Athena/Items/Cosmetics/",
        "GameFeatures/CosmeticCompanions/",
        "GameFeatures/MeshCosmetics/",
        "GameFeatures/CosmeticShoes/",
        "GameFeatures/SparksCosmetics/",
        "GameFeatures/FM/SparksSongTemplates/",
        "GameFeatures/FM/SparksCosmetics/",
        "GameFeatures/FM/SparksCharacterCommon/",
        "GameFeatures/VehicleCosmetics/",
        "GameFeatures/Juno/"
    ];

    private readonly List<EModelType> _models =
    [
        EModelType.ProfileAthena,
        EModelType.ItemShopCatalog
    ];
    private readonly List<EGenerationType> _generationTypes =
    [
        EGenerationType.AllCosmetics,
        EGenerationType.NewCosmetics,
        EGenerationType.NewCosmeticsAndArchives,
        EGenerationType.ArchiveCosmetics,
        EGenerationType.WaitForArchivesUpdate,
        EGenerationType.SelectedCosmeticsOnly,
        EGenerationType.ReturnToMenu
    ];

    private readonly List<EBackupOption> _backupOptions =
    [
        EBackupOption.Streamed,
        EBackupOption.Local
    ];

    // filters to use when filtering files
    private Func<GameFile, bool> _shopAssetsFilter => (x => x.Name.StartsWith("DAv2", StringComparison.OrdinalIgnoreCase));
    private Func<GameFile, bool> _cosmeticsFilter => (
        x => _directoriesToFilter.Any(p => x.Path.Contains(p, StringComparison.OrdinalIgnoreCase) &&
        (Assets.IsValidItemId(x.NameWithoutExtension) || Assets.IsValidPrefix(x.NameWithoutExtension)))
    );

    private List<IAesVfsReader> _availableArchives = [];

    private string _backupName = string.Empty;

    #region main
    public void LoadAvailableArchives()
    {
        foreach (var vf in UEParser.Provider.MountedVfs)
        {
            if (!vf.Name.EndsWith("utoc") || vf.EncryptionKeyGuid == UEParser.ZERO_GUID)
                continue;

            _availableArchives.Add(vf);
            Log.Information("Loaded {utoc} (FGuid: {guid}) into available archives", vf.Name, vf.EncryptionKeyGuid);
        }
    }

    public async Task ShowMenu()
    {
        while (true)
        {
            // get news every time we go back to menu to show the latest updates
            var news = await Api.Athena.GetNewsAsync();

            Console.Clear(); // clear console from parser logs & api logs

            Console.Title = $"Athena {Globals.Version.DisplayName} - FortniteGame v{UEParser.Manifest.GameVersion}";
            Discord.Update($"In Menu - FortniteGame v{UEParser.Manifest.GameVersion}");

            AnsiConsole.Markup($"Welcome to [12]Athena {Globals.Version.DisplayName}[/]: Made with [124]<3[/] by [12]@djlorenzouasset[/] & [12]@andredotuasset[/] with the help of many others.\n");
            AnsiConsole.Markup($"Join the [12]Discord Server[/] to stay updated on the development: [12]{Globals.DISCORD_URL}[/]\n");
            AnsiConsole.Markup($"Want to change [12]app/shop/profiles settings?[/] Go in the [underline 12]AppData/Roaming/Athena[/] folder and edit the [12]settingsV2.json[/] file.\n");

            if (news is not null && news.GetNews() is { Count: > 0} newsToDisplay)
            {
                Console.WriteLine(""); // spacing
                foreach (var text in newsToDisplay)
                {
                    AnsiConsole.Markup($"{text}\n");
                }
            }

            if (AppSettings.Default.ShowChangeLog && App.ReleaseInfo?.Changelog is not null)
            {
                Console.WriteLine("");
                AnsiConsole.Markup($"{App.ReleaseInfo.Changelog}\n");
                AppSettings.Default.ShowChangeLog = false;
            }

            Console.WriteLine(""); // spacing

            var model = SelectModel();
            Discord.Update(model);

            var generationType = SelectGenerationType(model);

            if (generationType == EGenerationType.ReturnToMenu)
            {
                ResetState();
                continue;
            }

            EReturnResult status = await HandleRequest(model, generationType);
            if (!ReturnToMenu(status is EReturnResult.Error))
                return;
        }
    }

    private async Task<EReturnResult> HandleRequest(EModelType model, EGenerationType generationType)
    {
        Log.ForContext("NoConsole", true).Information("User selected {model} -> {type}", model, generationType); 

        EReturnResult status = EReturnResult.Success;
        switch (generationType)
        {
            case EGenerationType.NewCosmetics:
                status = await HandleNewCosmetics();
                break;

            case EGenerationType.NewCosmeticsAndArchives:
                status = await HandleNewCosmetics(true);
                break;

            case EGenerationType.ArchiveCosmetics:
                status = HandleArchiveCosmetics(model);
                break;

            case EGenerationType.WaitForArchivesUpdate:
                status = await HandleWaitForArchivesUpdate(model);
                break;

            case EGenerationType.SelectedCosmeticsOnly:
                HandleSelectedCosmetics(model);
                break;
        }

        if (status is not EReturnResult.Success)
        {
            return status;
        }

        var start = Stopwatch.StartNew();
        if (await GenerateModel(model, generationType) is EReturnResult.Error)
            return EReturnResult.Error;

        Log.Information("All tasks finished in {sec}s ({ms}ms).",
            Math.Round(start.Elapsed.TotalSeconds, 2),
            Math.Round(start.Elapsed.TotalMilliseconds));

        return EReturnResult.Success;
    }

    private async Task<EReturnResult> GenerateModel(EModelType model, EGenerationType generationType)
    {
        string itemType = model.ItemTypeName().ToLower();
        bool bIsProfile = model is EModelType.ProfileAthena;
        bool bUseAllEntries = generationType is EGenerationType.AllCosmetics;

        var filter = (bIsProfile ? _cosmeticsFilter : _shopAssetsFilter);
        var entries = (bUseAllEntries ? UEParser.AllEntries : UEParser.NewEntries);

        var filtered = entries.Where(filter).ToHashSet();
        if (filtered.Count == 0)
        {
            switch (generationType)
            {
                case EGenerationType.AllCosmetics:
                    Log.Error("No {type} have been found.", itemType);
                    break;
                case EGenerationType.NewCosmetics:
                case EGenerationType.NewCosmeticsAndArchives:
                    Log.Error("No new {type} have been found using backup {backup}.", itemType, _backupName);
                    break;
                case EGenerationType.ArchiveCosmetics:
                case EGenerationType.WaitForArchivesUpdate:
                    Log.Error("No new {type} have been found in the requested paks.", itemType);
                    break;
                case EGenerationType.SelectedCosmeticsOnly:
                    Log.Error("The selected {type} could not be found.", itemType);
                    break;
            }
            return EReturnResult.Error;
        }

        string savePath = bIsProfile
            ? Path.Combine(AppSettings.Default.ProfilesSettings.OutputPath, "profile_athena.json")
            : Path.Combine(AppSettings.Default.CatalogSettings.OutputPath, AppSettings.Default.CatalogSettings.ShopName);

        int added = 0;
        IBuilder builder = bIsProfile ? new ProfileBuilder() : new ShopBuilder();
        if (bIsProfile)
        {
            var profile = (ProfileBuilder)builder;
            await Parallel.ForEachAsync(filtered, async (item, _) =>
            {
                try
                {
                    var export = await UEParser.Provider.SafeLoadPackageObjectAsync(item.PathWithoutExtension);
                    if (export == null || !Assets.IsValidClass(export.ExportType))
                        return;

                    var variants = AthenaUtils.GetCosmeticVariants(export);
                    string backendType = Assets.GetBackendTypeByClass(export.ExportType);

                    lock (profile)
                    {
                        profile.AddCosmetic(item.NameWithoutExtension, backendType, variants);
                    }

                    Interlocked.Increment(ref added);
                    Log.Information("Added \"{name}\" (Type: {exportType}, Variants: {variantsCount})",
                        item.NameWithoutExtension, export.ExportType, variants.Count);
                }
                catch (Exception e)
                {
                    Log.Error("Failed add item {name}: {msg}", item.NameWithoutExtension, e.Message);
                    return;
                }
            });
        }
        else
        {
            var itemShop = (ShopBuilder)builder;
            foreach (var entry in filtered)
            {
                try
                {
                    itemShop.AddCatalogEntry(entry.PathWithoutExtension);

                    added++;
                    Log.Information("Added shop asset \"{name}\"", entry.NameWithoutExtension);
                }
                catch (Exception e)
                {
                    // skip those exceptions that occour only on some assets like BPs
                    if (e is InvalidOperationException || e is ArgumentOutOfRangeException)
                    {
                        Log.Error("[IGNORE] Failed add shop item {name}", entry.NameWithoutExtension);
                        continue;
                    }

                    Log.Error("Failed add shop item {name}: {msg}", entry.NameWithoutExtension, e.Message);
                    continue;
                }
            }
        }

        Log.Information("Building {model} with {addedCount} {itemType}", model.DisplayName(), added, itemType);
        string rawJson = builder.Build();

        if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        { 
            // create the directory if doesn't exist
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            Log.Warning("The output directory you did set does not exist! Creating it.");
        }

        try
        {
            await File.WriteAllTextAsync(savePath, rawJson);
        }
        catch (Exception ex)
        {
            Log.Warning("There was an error saving {model} ({ex}). Saving in general folder.", model.DisplayName(), ex.Message);
            savePath = Path.Combine(Directories.Output, Path.GetFileName(savePath));
            await File.WriteAllTextAsync(savePath, rawJson);
        }

        Log.Information("Saved {model} in output directory {savePath}", model.DisplayName(), savePath);
        return EReturnResult.Success;
    }

    private bool ReturnToMenu(bool bFromError = false)
    {
        Console.WriteLine(""); // spacing
        string prompt = bFromError ? "Do you want to try again?" : "Do you want to go back to menu?";
        if (!AnsiConsole.Confirm(prompt))
            return false;

        ResetState();
        return true;
    }

    private void ResetState()
    {
        _backupName = "";
        UEParser.NewEntries.Clear();
    }
    #endregion

    #region selectors
    private EModelType SelectModel()
    {
        return App.SelectionPrompt("What do you want to generate?", _models, m => m.DisplayName());
    }

    private EGenerationType SelectGenerationType(EModelType selectedModel)
    {
        string title = $"What do you want to use to generate this [12]{selectedModel.DisplayName()}[/]?";
        return App.SelectionPrompt(title, _generationTypes.FindAll(gt => !gt.DisabledFor(selectedModel)), gt => gt.DisplayName());
    }

    private List<FGuid> SelectArchives(List<IAesVfsReader> availableEntries)
    {
        var archives = App.MultiSelectionPrompt("What [12]Paks[/] do you want generate?", availableEntries, vf => $"{vf.Name} ({vf.GetSize()})");
        return [.. archives.Select(ar => ar.EncryptionKeyGuid)];
    }

    private EBackupOption SelectBackupMode()
    {
        return App.SelectionPrompt("What type of [12]Backup[/] do you want to use?", _backupOptions, opt => opt.DisplayName());
    }

    private FileInfo SelectLocalBackup(List<FileInfo> backupsToShow)
    {
        return App.SelectionPrompt("What [12]Local Backup[/] do you want to use?", backupsToShow, f => f.Name);
    }

    private HashSet<string> GetCustomCosmetics(EModelType selectedModel)
    {
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string input = $"{selectedModel.ItemTypeName()} Ids";
        var selected = App.Ask($"Insert the [12]{input}[/] you want to add separated by [12];[/]:", 0);
        ids.UnionWith(selected.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
        return ids;
    }
    #endregion

    #region selection logic
    private async Task<EReturnResult> HandleNewCosmetics(bool bIncludeArchives = false)
    {
        var oldFiles = await GetBackupEntries();
        if (oldFiles is null) return EReturnResult.Error;

        UEParser.LoadEntries(ar => ar.EncryptionKeyGuid == UEParser.ZERO_GUID ||
            (bIncludeArchives && !UEParser.Provider.RequiredKeys.Contains(ar.EncryptionKeyGuid)),
            oldFiles,
            bNew: true
        );

        return EReturnResult.Success;
    }
    
    private EReturnResult HandleArchiveCosmetics(EModelType selectedModel)
    {
        var filter = selectedModel is EModelType.ProfileAthena ? _cosmeticsFilter : _shopAssetsFilter;
        var archivesToShow = _availableArchives.Where(vf => vf.HasFilter(filter)).ToList();

        if (archivesToShow.Count == 0)
        {
            Log.Error($"There are no archives available containing {selectedModel.ItemTypeName().ToLower()}");
            return EReturnResult.Warning;
        }

        var selected = SelectArchives([..archivesToShow]);
        UEParser.LoadEntries(ar => selected.Contains(ar.EncryptionKeyGuid), bNew: true);

        return EReturnResult.Success;
    }

    // this never return errors
    private void HandleSelectedCosmetics(EModelType selectedModel)
    {
        var customIds = GetCustomCosmetics(selectedModel);
        UEParser.LoadEntries(ar => ar.EncryptionKeyGuid == UEParser.ZERO_GUID ||
            !UEParser.Provider.RequiredKeys.Contains(ar.EncryptionKeyGuid),
            customIds,
            bIsCustom: true
        );
    }

    private async Task<EReturnResult> HandleWaitForArchivesUpdate(EModelType selectedModel)
    {
        var token = new CancellationTokenSource();
        _ = Task.Run(() =>
        {
            while (true)
            {
                if (!Console.KeyAvailable)
                    continue;

                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Escape)
                {
                    token.Cancel();
                    break;
                }
            }
        }, token.Token); // use cancellation here to cancel this task once finished

        List<DynamicKey> newKeys;
        while (!token.IsCancellationRequested)
        {
            try
            {
                Log.Information("Checking for new AES keys (press Escape to return to menu).");
                await Task.Delay(TimeSpan.FromSeconds(5), token.Token);

                var res = await Api.Dilly.GetAESKeysAsync(false);
                if (res is null || res.DynamicKeys.Count == 0)
                    continue;

                newKeys = [..res.DynamicKeys.Where(k => 
                    !_availableArchives.Any(ar => ar.EncryptionKeyGuid == new FGuid(k.Guid)))];

                if (newKeys.Count == 0)
                    continue;

                AppSettings.Default.LocalKeys = res;
                UEParser.LoadKeysList(newKeys);

                // load only the new archives that contain files based on the selected model
                var filter = selectedModel is EModelType.ProfileAthena ? _cosmeticsFilter : _shopAssetsFilter;
                var vfs = UEParser.Provider.MountedVfs.Where(vf => 
                    vf.EncryptionKeyGuid != UEParser.ZERO_GUID && vf.Files.Values.Any(filter)).ToList();

                if (vfs.Count == 0)
                    continue;

                Log.Information("Detected {tot} new archives containing {model}!", vfs.Count, selectedModel.ItemTypeName().ToLower());
                UEParser.LoadEntries(ar => vfs.Any(vf => vf.EncryptionKeyGuid == ar.EncryptionKeyGuid), bNew: true);

                foreach (var vf in vfs)
                {
                    _availableArchives.Add(vf);
                }

                return EReturnResult.Success;
            }
            catch (OperationCanceledException)
            {
                Log.Information("Stopping AES watcher task.");
                return EReturnResult.NoResult;
            }
        }

        return EReturnResult.NoResult;
    }

    private async Task<HashSet<string>?> GetBackupEntries()
    {
        var mode = SelectBackupMode();

        FileInfo backupFile;
        if (mode == EBackupOption.Streamed) // principal & default mpde
        {
            var backup = await GetLatestBackup();
            if (backup is null) return null;

            string downloadPath = Path.Combine(Directories.Backups, backup.FileName);
            if (await Api.DownloadFileAsync(backup.DownloadUrl, downloadPath, true) is not { } fileInfo)
            {
                Log.Error("Failed to download backup.");
                return null;
            }
            backupFile = fileInfo;
        }
        else
        {
            var backups = Directories.GetSavedBackups();
            if (backups.Count == 0)
            {
                Log.Error("No local backups have been found.");
                return null;
            }

            backupFile = SelectLocalBackup(backups);
        }

        _backupName = backupFile.Name;
        return await BackupParser.Parse(backupFile);
    }

    private async Task<Backup?> GetLatestBackup()
    {
        var backups = await Api.Athena.GetBackupsAsync();
        if (backups is null)
        {
            Log.Error("Failed to get backups!");
            return null;
        }
        else if (backups.Length == 0)
        {
            Log.Error("There are no backups available at the moment. Please try again later.");
            return null;
        }

        return backups.Last();
    }
    #endregion
}