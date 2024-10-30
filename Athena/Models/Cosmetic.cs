namespace Athena.Models;

/* REWRITE IN V2*/

public class ProfileCosmetic
{
    public ProfileCosmetic(string cosmeticId, string backendType, Dictionary<string, List<string>>? variants = null)
    {
        templateId = $"{backendType}:{cosmeticId}";
        attributes.favorite = false;
        attributes.archived = false;

        if (variants != null && variants.Count > 0)
        {
            foreach (var (channel, ownedParts) in variants)
            {
                attributes.variants.Add(new() 
                { 
                    channel = channel, 
                    active = ownedParts.Count > 0 ? ownedParts[0] : "", // scuffed thing for FortCosmeticRichColorVariant
                    owned = ownedParts 
                });
            }
        }
    }

    public string templateId { get; set; }
    public Attributes attributes = new();
    public int quantity = 1;
}

public class Attributes
{
    // public DateTime creation_time = DateTime.Now; this cause no variants on > 18.00 (???)
    public int max_level_bonus = 0;
    public int level = 1;
    public bool item_seen = true;
    public int rnd_sel_cnt = 0;
    public int xp = 0;
    public List<ProfileAthena.Variant> variants = [];
    public bool favorite = false;
    public bool archived = false;
}