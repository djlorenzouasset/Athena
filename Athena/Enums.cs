using System.ComponentModel;
using Athena.Models;

namespace Athena;

public enum EReturnResult : int
{
    Error = 0, // >> error: used to display the "try again" question
    Warning = 1, // >> warning: used to return the user to menu on non-user errors
    Success = 2, // >> success: the task was completed without any issue

    NoResult = 3 // >> no result: used to show the menu without issues
}

public enum EModelType : int
{
    [Description("Profile Athena"), ItemType("Cosmetics")]
    ProfileAthena = 0,

    [Description("ItemShop Catalog"), ItemType("Shop Assets")]
    ItemShopCatalog = 1
}

public enum EGenerationType : int
{
    // ItemShopCatalog is disabled because we cannot create a shop that big
    // as we dont know if the game can handle it, idk if has been ever tested
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
    ReturnToMenu = 6 // >> return to main menu
}

public enum EBackupOption : int
{
    [Description("Latest Backup")]
    Streamed = 0,

    [Description("Local Backup")]
    Local = 1
}

public enum EBackupVersion : byte
{
    BeforeVersionWasAdded = 0,
    Initial,

    PerfectPath,

    LatestPlusOne,
    Latest = LatestPlusOne - 1
}