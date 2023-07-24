/*
    Solitude ProfileCosmetic.cs 
    File at https://github.com/OutTheShade/Solitude/blob/master/Solitude/Objects/Profile/ProfileCosmetic.cs
*/

namespace Athena.Models;

public class ProfileCosmetic
{
    public ProfileCosmetic(string cosmeticId, string backendType, bool isFavorite = false, bool isArchived = false)
    {
        templateId = $"{backendType}:{cosmeticId}";
        attributes.favorite = isFavorite;
        attributes.archived = isArchived;
    }

    public string templateId { get; set; }
    public Attributes attributes { get; set; } = new();
    public int quantity { get; set; } = 1;
}

public class Attributes
{
    public DateTime creation_time { get; set; } = DateTime.Now;
    public int max_level_bonus { get; set; } = 0;
    public int level { get; set; } = 1;
    public bool item_seen { get; set; } = true;
    public int rnd_sel_cnt { get; set; } = 0;
    public int xp { get; set; } = 0;
    public ProfileAthenaModel.Variant[] variants { get; set; } = { };
    public bool favorite { get; set; } = false;
    public bool archived { get; set; } = false;
}