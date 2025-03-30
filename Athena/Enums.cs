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
    AllCosmetics = 0, // >> add all cosmetics
    NewCosmetics = 1, // >> add only new cosmetics filtered by backup
    NewCosmeticsAndArchives = 2, // >> add only new cosmetics + encrypted archives

    SingleArchive = 3, // >> add a single archive
    MultipleArchives = 4, // >> add multiple archives by name or guid
    WaitForArchivesUpdate = 5, // >> watch for new archives to become availables

    SelectedCosmeticsOnly = 6 // >> add only selected cosmetics by ID/DAv2
}

public enum EBackupVersion : byte
{
    BeforeVersionWasAdded = 0,
    Initial,

    PerfectPath,

    LatestPlusOne,
    Latest = LatestPlusOne - 1
}