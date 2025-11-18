namespace Athena.Models.Settings;

public class ItemOptions : IOption
{
    public string ItemId { get; set; } = string.Empty;

    public int Price { get; set; } = -1;
    public string ViolatorTag { get; set; } = null!;
    public CardOptions CardOptions { get; set; } = new();
}

public class BundleOptions : IOption
{
    public string BundleId { get; set; } = string.Empty;

    public int Price { get; set; } = -1;
    public string ViolatorTag { get; set; } = null!;
    public CardOptions CardOptions { get; set; } = new();
    public List<string> BundleCosmetics { get; set; } = [];
}