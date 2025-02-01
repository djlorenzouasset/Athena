namespace Athena;

public enum Model : int
{
    ItemShop = 0,
    ProfileAthena = 1
}

public enum Actions : int
{
    AddNew = 0, // new cosmetics
    AddArchive = 1, // only selected archive (ioStore)
    BulkArchive = 2, // add only selected archives (ioStores)
    AddEverything = 3, // add all cosmetics (athena profile only)
    AddOnlySelected = 4, // add only selected cosmetics (filtered by ids) - +v1.5.0
    AddNewWithArchives = 5 // new files + archives (ioStores)
}

// FModel versioned backup
public enum EBackupVersion : byte
{
    BeforeVersionWasAdded = 0,
    Initial,

    LatestPlusOne,
    Latest = LatestPlusOne - 1
}