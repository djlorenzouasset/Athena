using System.Diagnostics;
using Spectre.Console;
using CUE4Parse.Utils;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.Objects.Core.Misc;
using Athena.Utils;
using Athena.Services;
using Athena.Extensions;
using Athena.Models.API.Responses;
using CUE4Parse.UE4.VirtualFileSystem;

namespace Athena.Core;

public class Generator
{
    private string _backupName = "";
    private readonly List<string> _customCosmeticsOrPaks = [];

    private readonly List<EModelType> _menuOptions = [
        EModelType.ProfileAthena, 
        EModelType.ItemShopCatalog
    ];

    private static readonly List<EGenerationType> _shopOptions = [
        EGenerationType.NewCosmetics,
        EGenerationType.NewCosmeticsAndArchives,
        EGenerationType.ArchiveCosmetics,
        EGenerationType.WaitForArchivesUpdate,
        EGenerationType.SelectedCosmeticsOnly,
        EGenerationType.ReturnToMenu
    ];
    private static readonly List<EGenerationType> _profileOptions = [
        EGenerationType.AllCosmetics, .._shopOptions
    ];
    private readonly Dictionary<EModelType, List<EGenerationType>> _modelsOptions = new()
    {
        { EModelType.ProfileAthena, _profileOptions },
        { EModelType.ItemShopCatalog, _shopOptions },
    };

    private readonly HashSet<string> _acceptedClasses = [
        // BR
        "AthenaCharacterItemDefinition", "AthenaBackpackItemDefinition",
        "AthenaPickaxeItemDefinition", "AthenaGliderItemDefinition",
        "AthenaPetCarrierItemDefinition", "AthenaToyItemDefinition",
        "AthenaEmojiItemDefinition", "AthenaSprayItemDefinition",
        "AthenaLoadingScreenItemDefinition", "AthenaDanceItemDefinition",
        "AthenaSkyDiveContrailItemDefinition", "AthenaItemWrapDefinition",
        "AthenaMusicPackItemDefinition", "CosmeticShoesItemDefinition",
        "CosmeticCompanionItemDefinition", "CosmeticCompanionReactFXItemDefinition",
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
    private static readonly List<string> _acceptedPaths = [
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

    public static readonly Func<VfsEntry, bool> ShopAssetsFilter = (x => x.Name.StartsWith("DAv2", StringComparison.OrdinalIgnoreCase));
    public static readonly Func<VfsEntry, bool> CosmeticsFilter = (x => _acceptedPaths.Any(
        p => x.Path.Contains(p, StringComparison.OrdinalIgnoreCase))
    );

    public async Task ShowMenu()
    {
        Console.Clear(); // clear the previous logs from the console

        string gameVersion = Dataminer.Instance.Manifest.GameVersion;
        Console.Title = $"Athena {Globals.Version.DisplayName} - FortniteGame v{gameVersion}";
        DiscordRichPresence.Update($"In Menu - FortniteGame v{gameVersion}");

        // TODO: add the application permanent text

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
        DiscordRichPresence.Update(model);

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

        Log.Information("All tasks finished in {0}s ({1}ms).", 
            Math.Round(start.Elapsed.TotalSeconds, 2),
            Math.Round(start.Elapsed.TotalMilliseconds));

        await ReturnToMenu();
        return;
    }

    private async Task<bool> GenerateModel(EModelType model, EGenerationType generationType)
    {
        var dataminer = Dataminer.Instance;

        bool bIsProfile = model is EModelType.ProfileAthena;
        bool bUseAllEntries = generationType is EGenerationType.AllCosmetics;

        string itemsType = bIsProfile ? "cosmetics" : "shop assets";
        var filter = (bIsProfile ? CosmeticsFilter : ShopAssetsFilter);
        var entries = (bUseAllEntries ? dataminer.AllEntries : dataminer.NewEntries);

        var filtered = entries.Where(filter).ToHashSet();
        if (filtered.Count == 0)
        {
            switch (generationType)
            {
                case EGenerationType.AllCosmetics:
                    Log.Error("No {0} have been found.", itemsType);
                    break;
                case EGenerationType.NewCosmetics:
                case EGenerationType.NewCosmeticsAndArchives:
                    Log.Error("No new {0} have been found using backup {1}.", itemsType, _backupName);
                    break;
                case EGenerationType.ArchiveCosmetics:
                case EGenerationType.WaitForArchivesUpdate:
                    Log.Error("No new {0} have been found in the following paks: {1}.", 
                        itemsType, string.Join(',', _customCosmeticsOrPaks));
                    break;
                case EGenerationType.SelectedCosmeticsOnly:
                    Log.Error("The selected {0} could not be found.", itemsType);
                    break;
            }
            await ReturnToMenu(bFromError: true);
            return false;
        }

        IBuilder builder = bIsProfile ? new ProfileBuilder() : new ShopBuilder();
        string savePath = bIsProfile 
            ? Path.Combine(SettingsService.Current.Profiles.OutputPath, "profile_athena.json")
            : Path.Combine(SettingsService.Current.Catalog.OutputPath, SettingsService.Current.Catalog.ShopName);

        int added = 0;
        if (bIsProfile)
        {
            var profile = (ProfileBuilder)builder;
            // we use parallel here to speed up the process as we do a lot of operations
            await Parallel.ForEachAsync(filtered, new ParallelOptions { MaxDegreeOfParallelism = 8 }, async (item, token) =>
            {
                try
                {
                    var export = await dataminer.Provider.SafeLoadPackageObjectAsync(item.PathWithoutExtension);
                    if (export == null || !_acceptedClasses.Contains(export.ExportType))
                        return;

                    var variants = AthenaUtils.GetCosmeticVariants(export);
                    string backendType = AthenaUtils.GetBackendType(export.ExportType);

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
            var shop = (ShopBuilder)builder;
            foreach (var entry in filtered)
            {
                try
                {
                    shop.AddCatalogEntry(entry.PathWithoutExtension);
                }
                catch (Exception e)
                {
                    Log.Error("Failed add shop item {0}: {1}", entry.NameWithoutExtension, e.Message);
                    continue;
                }

                added++;
                Log.Information("Added shop asset \"{0}\"", entry.NameWithoutExtension);
            }
        }

        if (!Directory.Exists(Path.GetDirectoryName(savePath)))
        {
            Log.Warning("The output directory you did set does not exist! The default one will be used");
            savePath = Path.Combine(Directories.Output.FullName, bIsProfile
                ? "profile_athena.json" : SettingsService.Current.Catalog.ShopName);
        }

        Log.Information("Building {0} with {1} {2}", model.DisplayName(), added, itemsType);
        await File.WriteAllTextAsync(savePath, builder.Build());
        Log.Information("Saved {0} in output directory {1}", model.DisplayName(), savePath);

        return true;
    }

    private async Task ReturnToMenu(bool bSkipRequest = false, bool bFromError = false)
    {
        if (!bSkipRequest)
        {
            string prompt = bFromError ? "\nDo you want to try again?" : "\nDo you want to go back to menu?";
            if (!AnsiConsole.Confirm(prompt)) return;
        }

        _backupName = "";
        _customCosmeticsOrPaks.Clear();
        Dataminer.Instance.NewEntries.Clear();

        await ShowMenu();
    }

    #region SELECTORS
    private EModelType SelectModel()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<EModelType>()
            .Title("What do you want to generate?")
            .AddChoices(_menuOptions)
            .UseConverter(e => e.DisplayName())
        );
    }

    private EGenerationType SelectGenerationType(EModelType model)
    {
        _modelsOptions.TryGetValue(model, out var optionsToShow);
        return AnsiConsole.Prompt(
            new SelectionPrompt<EGenerationType>()
            .Title($"What do you want to use to generate this [62]{model.DisplayName()}[/]?")
            .AddChoices(optionsToShow!)
            .UseConverter(e => e.DisplayName())
        );
    }

    private List<DynamicKey> SelectArchives()
    {
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<DynamicKey>()
            .Title("What [62]Paks[/] do you want generate?")
            .Required()
            .AddChoices(Dataminer.Instance.AESKeys!.DynamicKeys.Where(x => x.Name.EndsWith("utoc")))
            .UseConverter(e => $"{e.Name} ({e.Size.Formatted}, {e.FileCount} files)"));
    }

    private List<string> GetCustomCosmetics(EModelType model)
    {
        var type = model == EModelType.ProfileAthena ? "cosmetics ids" : "DAv2s";
        var selected = AthenaUtils.Ask($"Insert the [62]{type}[/] you want to add separated by [62];[/]:");
        return [.. selected.Split(';').Select(x => x.Trim())];
    }

    #endregion

    #region HANDLERS
    private async Task<bool> HandleNewCosmetics(bool includeArchives = false)
    {
        var oldFiles = await LoadBackup();
        if (oldFiles is null)
        {
            return true;
        }

        Dataminer.Instance.LoadEntries(
            ar => ar.EncryptionKeyGuid.Equals(Globals.ZERO_GUID) || (includeArchives && 
            (Dataminer.Instance.AESKeys?.GuidsList.Contains(ar.EncryptionKeyGuid) ?? false)),
            oldFiles,
            bNew: true
        );
        return false;
    }

    private void HandleArchiveCosmetics()
    {
        var dynamicKeys = SelectArchives();
        _customCosmeticsOrPaks.AddRange(dynamicKeys.Select(
            p => p.Name.Split("pakchunk").Last().Split('-').First()
        ));

        Dataminer.Instance.LoadEntries(
            ar => dynamicKeys.Select(k => new FGuid(k.Guid)).Contains(ar.EncryptionKeyGuid),
            bNew: true
        );
    }

    private async Task HandleWaitForArchivesUpdate()
    {
        var newKeys = await APIWatcher.GetNewDynamicKeys();

        _customCosmeticsOrPaks.AddRange(newKeys.Select(
            p => p.Name.Split("pakchunk").Last().Split('-').First()
        ));
        Dataminer.Instance.LoadKeys(newKeys);
        Dataminer.Instance.LoadEntries(
            ar => newKeys.Select(k => new FGuid(k.Guid)).Contains(ar.EncryptionKeyGuid),
            bNew: true
        );
    }

    private void HandleSelectedCosmetics(EModelType model)
    {
        var customIds = GetCustomCosmetics(model);
        _customCosmeticsOrPaks.AddRange(customIds);

        Dataminer.Instance.LoadEntries(
            ar => ar.EncryptionKeyGuid.Equals(Globals.ZERO_GUID) ||
                  (Dataminer.Instance.AESKeys?.GuidsList.Contains(ar.EncryptionKeyGuid) ?? false),
            [.. _customCosmeticsOrPaks],
            bIsCustom: true
        );
    }

    #endregion

    private async Task<HashSet<string>?> LoadBackup()
    {
        var backupFile = await FBackup.Download();
        if (backupFile is null)
        {
            Log.Error("Backup response was invalid!");
            return null;
        }

        _backupName = backupFile.Name.SubstringBeforeLast('.');
        return await FBackup.Parse(backupFile);
    }
}