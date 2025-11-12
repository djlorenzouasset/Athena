using System.ComponentModel;
using Athena.Models;

namespace Athena;

public enum EModelType : int
{
    [Description("Profile Athena"), ItemType("Cosmetics")]
    ProfileAthena = 0,

    [Description("ItemShop Catalog"), ItemType("Shop Assets")]
    ItemShopCatalog = 1
}

public enum EGenerationType : int
{
    // ItemShopCatalog is disabled because we cannot create a shop too big
    // as we dont know if the game can actually handle it, idk if has been ever tested
    [Description("All Cosmetics"), DisabledFor(EModelType.ItemShopCatalog)]
    AllCosmetics = 0, // >> add all cosmetics

    [Description("New Cosmetics")]
    NewCosmetics = 1, // >> add only new cosmetics filtered by backup

    [Description("New Cosmetics with Paks")]
    NewCosmeticsAndArchives = 2, // >> add only new cosmetics + encrypted archives

    [Description("Pak Cosmetics")]
    ArchiveCosmetics = 3, // >> add a single archive

    [Description("Wait for Pak decryption")]
    WaitForArchivesUpdate = 4, // >> watch for new archives to become availables

    [Description("Custom Cosmetics")]
    SelectedCosmeticsOnly = 5, // >> add only selected cosmetics by ID/DAv2

    [Description("Return to Menu")]
    ReturnToMenu = 6 // >> return to main menù
}

public enum EBackupVersion : byte
{
    BeforeVersionWasAdded = 0,
    Initial,

    PerfectPath,

    LatestPlusOne,
    Latest = LatestPlusOne - 1
}