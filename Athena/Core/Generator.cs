using System.Diagnostics;
using Spectre.Console;
using CUE4Parse.Utils;
using CUE4Parse.UE4.VirtualFileSystem;
using CUE4Parse.UE4.Objects.Core.Misc;
using Athena.Utils;
using Athena.Services;
using Athena.Extensions;
using Athena.Models.API.Fortnite;

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
    private readonly Dictionary<EModelType, List<EGenerationType>> _options = new()
    {
        { EModelType.ProfileAthena, _profileOptions },
        { EModelType.ItemShopCatalog, _shopOptions },
    };

    private readonly List<string> _acceptedClasses = [
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
    private static readonly List<string> _acceptedPaths = [
        "Athena/Items/Cosmetics/",
        "GameFeatures/MeshCosmetics/", // Caper and Alias cosmetics
        "GameFeatures/CosmeticShoes/",
        "GameFeatures/SparksCosmetics/",
        "GameFeatures/FM/SparksSongTemplates/",
        "GameFeatures/FM/SparksCosmetics/",
        "GameFeatures/FM/SparksCharacterCommon/",
        "GameFeatures/VehicleCosmetics/",
        "GameFeatures/Juno/"
    ];

    private readonly Func<VfsEntry, bool> _shopAssetsFilter = (x => x.Name.StartsWith("DAv2"));
    private readonly Func<VfsEntry, bool> _cosmeticsFilter = (x => _acceptedPaths.Any(
        p => x.Path.Contains(p, StringComparison.OrdinalIgnoreCase))
    );

    public async Task ShowMenu()
    {
        Console.Clear(); // clear the previous logs from the console

        string gameVersion = Dataminer.Instance.Manifest.GameVersion;
        Console.Title = $"Athena {Globals.Version.DisplayName} - FortniteGame {gameVersion}";
        DiscordRichPresence.Update($"In Menu - FortniteGame {gameVersion}");

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

        var start = Stopwatch.StartNew();
        switch (generationType)
        {
            case EGenerationType.AllCosmetics:
                break;

            case EGenerationType.NewCosmetics:
                await HandleNewCosmetics();
                break;

            case EGenerationType.NewCosmeticsAndArchives:
                await HandleNewCosmetics(true);
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

        Log.Information("All tasks finished in {sec}s ({ms}ms).", 
            Math.Round(start.Elapsed.TotalSeconds, 2),
            Math.Round(start.Elapsed.TotalMilliseconds));

        await ReturnToMenu();
        return;
    }

    #region SELECTORS

    private EModelType SelectModel()
    {
        return AnsiConsole.Prompt(
            new SelectionPrompt<EModelType>()
            .Title("What do you want to generate?")
            .AddChoices(_menuOptions)
            .UseConverter(e => e.GetDescription())
        );
    }

    private EGenerationType SelectGenerationType(EModelType model)
    {
        _options.TryGetValue(model, out var optionsToShow);

        return AnsiConsole.Prompt(
            new SelectionPrompt<EGenerationType>()
            .Title($"What do you want to use to generate this [62]{model.GetDescription()}[/]?")
            .AddChoices(optionsToShow!)
            .UseConverter(e => e.GetDescription())
        );
    }

    private List<DynamicKey> SelectArchives()
    {
        return AnsiConsole.Prompt(
            new MultiSelectionPrompt<DynamicKey>()
            .Title("What [62]Paks[/] do you want generate?")
            .Required()
            .AddChoices(Dataminer.Instance.AESKeys!.DynamicKeys)
            .UseConverter(e => $"{e.Name} ({e.Size.Formatted}, {e.FileCount} files)"));
    }

    private List<string> GetCustomCosmetics(EModelType model)
    {
        var type = model == EModelType.ProfileAthena ? "cosmetics ids" : "DAv2s";
        var selected = AnsiConsole.Ask<string>($"Insert the [62]{type}[/] you want to add separated by [62];[/]:");
        return selected.Split(';').Select(x => x.Trim()).ToList();
    }

    #endregion

    #region HANDLERS

    private async Task HandleNewCosmetics(bool includeArchives = false)
    {
        var oldFiles = await LoadBackup();
        Dataminer.Instance.LoadEntries(
            ar => ar.EncryptionKeyGuid.Equals(Globals.ZERO_GUID) || (includeArchives && 
            (Dataminer.Instance.AESKeys?.GuidsList.Contains(ar.EncryptionKeyGuid) ?? false)),
            oldFiles,
            bNew: true
        );
    }

    private void HandleArchiveCosmetics()
    {
        var dynamicKeys = SelectArchives();
        _customCosmeticsOrPaks.AddRange(dynamicKeys.Select(p => p.Name));

        Dataminer.Instance.LoadEntries(
            ar => dynamicKeys.Select(k => new FGuid(k.Guid)).Contains(ar.EncryptionKeyGuid),
            bNew: true
        );
    }

    private async Task HandleWaitForArchivesUpdate()
    {
        var newKeys = await APITask.GetNewDynamicKeys();

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
            _customCosmeticsOrPaks.ToHashSet(),
            bIsCustom: true
        );
    }

    #endregion

    private async Task<HashSet<string>> LoadBackup()
    {
        var backupFile = await FBackup.Download();
        if (backupFile is null)
        {
            Log.Error("Backup response was invalid!");
            return [];
        }

        _backupName = backupFile.Name.SubstringBeforeLast('.');
        return await FBackup.Parse(backupFile);
    }

    private async Task ReturnToMenu(bool bSkipRequest = false, bool bFromError = false)
    {
        if (!bSkipRequest)
        {
            string prompt = bFromError ? "Do you want to try again?" : "Do you want to go back to menu?";
            if (!AnsiConsole.Confirm(prompt)) return;
        }

        _backupName = "";
        _customCosmeticsOrPaks.Clear();
        Dataminer.Instance.NewEntries.Clear();

        await ShowMenu();
    }
}