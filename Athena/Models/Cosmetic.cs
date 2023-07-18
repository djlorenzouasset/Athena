// Solitude ProfileCosmetic.cs file at https://github.com/OutTheShade/Solitude/blob/master/Solitude/Objects/Profile/ProfileCosmetic.cs

namespace Athena.Models;

public class ProfileCosmetic
{
    public ProfileCosmetic(string cosmeticId, string backendType, bool isFavorite = false, bool isArchived = false)
    {
        TemplateId = $"{backendType}:{cosmeticId}";
        Attributes.Favourite = isFavorite;
        Attributes.Archived = isArchived;
    }

    public string TemplateId { get; set; }
    public Attributes Attributes { get; set; } = new();
    public int Quantity { get; set; } = 1;
}

public class Attributes
{
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public int MaxLevelBonus { get; set; } = 0;
    public int Level { get; set; } = 1;
    public bool ItemSeen { get; set; } = true;
    public int RndSelCnt { get; set; } = 0;
    public int Xp { get; set; } = 0;
    public ProfileAthenaModel.Variant[] Variants { get; set; } = { };
    public bool Favourite { get; set; } = false;
    public bool Archived { get; set; } = false;
}