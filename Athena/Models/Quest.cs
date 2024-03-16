namespace Athena.Models;

public class ProfileQuest
{
    public ProfileQuest(string questId, string rarity)
    {
        templateId = $"Quest:{questId.ToLower()}";
        attributes.quest_rarity = rarity.ToLower();
    }

    public string templateId { get; set; }
    public QuestAttributes attributes = new();
    public int quantity = 1;
}

public class QuestAttributes
{
    public DateTime creation_time = DateTime.Now;
    public string quest_state = "Active";
    public DateTime last_state_change_time = DateTime.Now;
    public int level = -1;
    public string challenge_bundle_id = Helper.GenerateRandomGuid();
    public string quest_rarity { get; set; }
    public int xp_reward_scalar = 1;
}