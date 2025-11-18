namespace Athena.Models.Settings;

public class CatalogSettings
{
    public string ShopName { get; set; } = "shopv3.json";
    public string OutputPath { get; set; } = Directories.Output;
    public int DefaultItemsPrice { get; set; } = -1;
    public int DefaultBundlesPrice { get; set; } = -1;
    public CardOptions DefaultCardOptions { get; set; } = new();
    public List<string> DefaultBundleCosmetics { get; set; } = [
        "CID_A_040_Athena_Commando_F_Temple",
        "Pickaxe_ID_015_HolidayCandyCane",
        "EID_Omega"
    ];
    public List<ItemOptions> CustomItemsOptions { get; set; } = [];
    public List<BundleOptions> CustomBundlesOptions { get; set; } = [];
}