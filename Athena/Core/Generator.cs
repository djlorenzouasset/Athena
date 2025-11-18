using System.Diagnostics;
using Spectre.Console;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.VirtualFileSystem;
using Athena.Utils;
using Athena.Builders;
using Athena.Extensions;
using Athena.Models.API.Responses;

namespace Athena.Core;

public class Generator
{
    private readonly List<EModelType> _models = [
        EModelType.ProfileAthena,
        EModelType.ItemShopCatalog
    ];
    private readonly List<EGenerationType> _generationTypes = [
        EGenerationType.AllCosmetics,
        EGenerationType.NewCosmetics,
        EGenerationType.NewCosmeticsAndArchives,
        EGenerationType.ArchiveCosmetics,
        EGenerationType.WaitForArchivesUpdate,
        EGenerationType.SelectedCosmeticsOnly,
        EGenerationType.ReturnToMenu
    ];

    private static readonly HashSet<string> _directoriesToFilter = [
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

    private readonly Func<VfsEntry, bool> ShopAssetsFilter = (x => x.Name.StartsWith("DAv2", StringComparison.OrdinalIgnoreCase));
    private readonly Func<VfsEntry, bool> CosmeticsFilter = (x => _directoriesToFilter.Any(
        p => x.Path.Contains(p, StringComparison.OrdinalIgnoreCase) && 
        (Assets.IsValidItemId(x.NameWithoutExtension) || Assets.IsValidPrefix(x.NameWithoutExtension)))
    );

    private string _backupName = string.Empty;
    private HashSet<string> _customCosmeticsOrPaks = [];

    public async Task ShowMenu()
    {
        Console.Clear(); // clear console from parser logs

        Console.Title = $"Athena {Globals.Version.DisplayName} - FortniteGame v{UEParser.Manifest.GameVersion}";
        Discord.Update($"In Menu - FortniteGame v{UEParser.Manifest.GameVersion}");

        // TODO:
        // - Add permanent text
        // - Add changelog

        var model = SelectModel();
        var generationType = SelectGenerationType(model);

        if (generationType == EGenerationType.ReturnToMenu)
        {
            await ReturnToMenu(true);
            return;
        }

        await HandleRequest(model, generationType);
    }

    private async Task HandleRequest(EModelType model, EGenerationType generationType)
    {
        Log.ForContext("NoConsole", true).Information("User selected {model} - {type}", model, generationType);
        Discord.Update(model);

        bool bForceReturnToMenu = false; // use this for backups failures
        switch (generationType)
        {
            case EGenerationType.NewCosmetics:
                bForceReturnToMenu = await HandleNewCosmetics();
                break;

            case EGenerationType.NewCosmeticsAndArchives:
                bForceReturnToMenu = await HandleNewCosmetics(true);
                break;

            case EGenerationType.ArchiveCosmetics:
                if (UEParser.AESKeys is null || UEParser.AESKeys.DynamicKeys is { Count: 0 })
                {
                    Log.Error("No paks are available at the moment.");
                    await ReturnToMenu();
                    return;
                }

                HandleArchiveCosmetics();
                break;

            case EGenerationType.WaitForArchivesUpdate:
                await HandleWaitForArchivesUpdate();
                break;

            case EGenerationType.SelectedCosmeticsOnly:
                HandleSelectedCosmetics(model);
                break;
        }

        if (bForceReturnToMenu)
        {
            await ReturnToMenu();
            return;
        }

        var start = Stopwatch.StartNew();
        if (!await GenerateModel(model, generationType))
            return; // this is already handled in the function

        Log.Information("All tasks finished in {sec}s ({ms}ms).",
            Math.Round(start.Elapsed.TotalSeconds, 2),
            Math.Round(start.Elapsed.TotalMilliseconds));

        await ReturnToMenu();
        return;
    }

    private async Task<bool> GenerateModel(EModelType model, EGenerationType generationType)
    {
        string itemType = model.ItemTypeName().ToLower();
        bool bIsProfile = model is EModelType.ProfileAthena;
        bool bUseAllEntries = generationType is EGenerationType.AllCosmetics;

        var filter = (bIsProfile ? CosmeticsFilter : ShopAssetsFilter);
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
                    Log.Error("No new {type} have been found in the following paks: {paks}.",
                        itemType, string.Join(',', _customCosmeticsOrPaks));
                    break;
                case EGenerationType.SelectedCosmeticsOnly:
                    Log.Error("The selected {type} could not be found.", itemType);
                    break;
            }
            await ReturnToMenu(bFromError: true);
            return false;
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
                    Log.Information("Added \"{0}\" (Type: {1}, Variants: {2})",
                        item.NameWithoutExtension, export.ExportType, variants.Count);
                }
                catch (Exception e)
                {
                    Log.Error("Failed add item {0}: {1}", item.NameWithoutExtension, e.Message);
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
                    Log.Information("Added shop asset \"{0}\"", entry.NameWithoutExtension);
                }
                catch (Exception e)
                {
                    Log.Error("Failed add shop item {0}: {1}", entry.NameWithoutExtension, e.Message);
                    continue;
                }
            }
        }

        if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        {
            savePath = Path.Combine(Directories.Output, Path.GetFileName(savePath));
            Log.Warning("The output directory you did set does not exist! The default one will be used");
        }

        Log.Information("Building {model} with {addedCount} {itemType}", model.DisplayName(), added, itemType);
        await File.WriteAllTextAsync(savePath, builder.Build());
        Log.Information("Saved {model} in output directory {savePath}", model.DisplayName(), savePath);

        return true;
    }

    private EModelType SelectModel()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<EModelType>()
            .Title("What do you want to generate?")
            .AddChoices(_models)
            .UseConverter(model => model.DisplayName())
        );
    }

    private EGenerationType SelectGenerationType(EModelType selectedOption)
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<EGenerationType>()
            .Title($"What do you want to use to generate this [62]{selectedOption.DisplayName()}[/]?")
            .AddChoices(_generationTypes.Where(gt => !gt.DisabledFor(selectedOption)))
            .UseConverter(e => e.DisplayName())
        );
    }

    private async Task ReturnToMenu(bool bSkipRequest = false, bool bFromError = false)
    {
        if (!bSkipRequest)
        {
            string prompt = bFromError ? "\n\nDo you want to try again?" : "\n\nDo you want to go back to menu?";
            if (!AnsiConsole.Confirm(prompt)) return;
        }

        _backupName = "";
        _customCosmeticsOrPaks.Clear();
        UEParser.NewEntries.Clear();

        await ShowMenu();
    }

    private List<DynamicKey> SelectArchives()
    {
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<DynamicKey>()
            .Title("What [62]Paks[/] do you want generate?")
            .Required()
            .AddChoices(UEParser.AESKeys!.DynamicKeys.Where(x => x.Name.EndsWith("utoc")))
            .UseConverter(e => $"{e.Name} ({e.Size.Formatted}, {e.FileCount} files)")
        );
    }

    private List<string> GetCustomCosmetics(EModelType model)
    {
        var type = model == EModelType.ProfileAthena ? "cosmetics ids" : "DAv2s";
        var selected = App.Ask($"Insert the [62]{type}[/] you want to add separated by [62];[/]:", 2);
        return [..selected.Split(';').Select(x => x.Trim())];
    }

    private async Task<bool> HandleNewCosmetics(bool includeArchives = false)
    {
        var oldFiles = await GetBackupEntries();
        if (oldFiles is { Count: 0 })
        {
            return true; // forceReturnToMenu
        }

        UEParser.LoadEntries(
            ar => ar.EncryptionKeyGuid.Equals(Globals.ZERO_GUID) || 
            (includeArchives && (UEParser.AESKeys?.GuidsList.Contains(ar.EncryptionKeyGuid) ?? false)),
            oldFiles,
            bNew: true
        );
        return false;
    }

    private void HandleArchiveCosmetics()
    {
        var dynamicKeys = SelectArchives();
        _customCosmeticsOrPaks.UnionWith(dynamicKeys.Select(
            p => p.Name.Split("pakchunk").Last().Split('-').First()
        ));

        UEParser.LoadEntries(
            ar => dynamicKeys.Select(k => new FGuid(k.Guid)).Contains(ar.EncryptionKeyGuid),
            bNew: true
        );
    }

    private async Task HandleWaitForArchivesUpdate()
    {
        var newKeys = await WatchForDynamicKeys();

        _customCosmeticsOrPaks.UnionWith(newKeys.Select(
            p => p.Name.Split("pakchunk").Last().Split('-').First()
        ));

        UEParser.LoadKeysList(newKeys);
        UEParser.LoadEntries(
            ar => newKeys.Select(k => new FGuid(k.Guid)).Contains(ar.EncryptionKeyGuid),
            bNew: true
        );
    }

    private void HandleSelectedCosmetics(EModelType model)
    {
        var customIds = GetCustomCosmetics(model);
        _customCosmeticsOrPaks.UnionWith(customIds);

        UEParser.LoadEntries(
            ar => ar.EncryptionKeyGuid.Equals(Globals.ZERO_GUID) ||
            (UEParser.AESKeys?.GuidsList.Contains(ar.EncryptionKeyGuid) ?? false),
            [.. _customCosmeticsOrPaks],
            bIsCustom: true
        );
    }

    private async Task<HashSet<string>> GetBackupEntries()
    {
        var backup = await GetLatestBackup();
        if (backup is null) return [];

        _backupName = backup.FileName;
        var file = new FileInfo(Path.Combine(Directories.Backups, backup.FileName));
        return await BackupParser.Parse(file);
    }

    private async Task<Backup?> GetLatestBackup()
    {
        var backups = await Api.Athena.GetBackupsAsync();
        if (backups is null || backups is { Length: 0 })
        {
            Log.Error("Failed to get backups!");
            return null;
        }
        return backups.LastOrDefault();
    }

    private const int TASK_COOLDOWN = 5 * 1000;
    private async Task<List<DynamicKey>> WatchForDynamicKeys()
    {
        Log.ForContext("NoConsole", true).Information(
            "WatchForDynamicKeys(): Starting. Cooldown set to {cd}ms", TASK_COOLDOWN);

        List<DynamicKey> newKeys;
        while (true)
        {
            Log.Information("Checking for new AES keys.");
            await Task.Delay(TASK_COOLDOWN);

            var res = await Api.Dilly.GetAESKeysAsync(false);
            if (res is null || res.DynamicKeys.Count == 0) continue;

            newKeys = [..res.DynamicKeys.Where(key =>
                !UEParser.AESKeys?.GuidsList.Contains(new FGuid(key.Guid)) ?? true)];

            if (newKeys.Count == 0)
                continue;

            Log.Information("Detected {tot} new Dynamic Keys!", newKeys.Count);

            UEParser.AESKeys = res;
            AppSettings.Default.LocalKeys = res;
            AppSettings.SaveSettings();

            return newKeys;
        }
    }
}