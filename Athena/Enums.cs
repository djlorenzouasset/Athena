using System.ComponentModel;

namespace Athena;

public enum EModelType : int
{
    [Description("Profile Athena")]
    ProfileAthena = 0,
    [Description("ItemShop Catalog")]
    ItemShopCatalog = 1
}

public enum EGenerationType : int
{
    [Description("All Cosmetics")]
    AllCosmetics = 0, // >> add all cosmetics
    [Description("New Cosmetics")]
    NewCosmetics = 1, // >> add only new cosmetics filtered by backup
    [Description("New Cosmetics with Paks")]
    NewCosmeticsAndArchives = 2, // >> add only new cosmetics + encrypted archives

    [Description("Pak Cosmetics")]
    SingleArchive = 3, // >> add a single archive
    [Description("Multiple Paks Cosmetics")]
    MultipleArchives = 4, // >> add multiple archives by name or guid
    [Description("Wait for Pak decryption")]
    WaitForArchivesUpdate = 5, // >> watch for new archives to become availables

    [Description("Custom Cosmetics")]
    SelectedCosmeticsOnly = 6, // >> add only selected cosmetics by ID/DAv2

    [Description("Return to Menu")]
    ReturnToMenu = 7 // >> return to main menù
}

public enum EBackupVersion : byte
{
    BeforeVersionWasAdded = 0,
    Initial,

    PerfectPath,

    LatestPlusOne,
    Latest = LatestPlusOne - 1
}