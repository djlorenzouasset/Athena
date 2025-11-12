namespace Athena.Models.Profiles;

public class Cosmetic : IProfileItem
{
    public string TemplateId = string.Empty;
    public Attributes Attributes = new();
    public int Quantity = 1;

    public Cosmetic(string cosmeticId, string backendType, List<Variant> variants)
    {
        TemplateId = $"{backendType}:{cosmeticId}";
        if (variants.Count > 0)
        {
            Attributes.Variants.AddRange(variants);
        }
    }
}

public class Attributes
{
    // public DateTime CreationTime = DateTime.UtcNow; this causes no variants on > 18.00 (???)
    public int MaxLevelBonus = 0;
    public int Level = 1;
    public bool ItemSeen = true;
    public int RndSelCnt = 0;
    public int Xp = 0;
    public List<Variant> Variants = [];
    public bool Favorite = false;
    public bool Archived = false;
}