namespace Athena;

public enum Model : int
{
    ItemShop = 0,
    ProfileAthena = 1
}

public enum Actions : int
{
    AddNew = 0, // new files
    AddArchive = 1, // only selected archive (ioStore)
    BulkArchive = 2, // add only selected archives (ioStores)
    AddEverything = 3, // add all cosmetics (athena profile only)
    AddOnlySelected = 4, // upcoming shit
    AddNewWithArchives = 5 // new files + archives (ioStores)
}