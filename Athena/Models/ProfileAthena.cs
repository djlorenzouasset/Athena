/* 
    Solitude ProfileAthena.cs 
    File at https://github.com/OutTheShade/Solitude/blob/master/Solitude/Objects/Profile/ProfileAthena.cs
*/

namespace Athena.Models;

public class ProfileAthenaModel
{
    public string _id = Config.config.athenaProfileId;
    public DateTime created { get; set; } = DateTime.Now;
    public DateTime updated { get; set; } = DateTime.Now;
    public int rvn { get; set; } = 69;
    public int wipeNumber { get; set; } = 1;
    public string accountId = Config.config.athenaProfileId;
    public string profileId { get; set; } = "athena";
    public string version = Config.config.athenaProfileId;
    public Items items { get; set; } = new();
    public Stats stats { get; set; } = new();
    public int commandRevision { get; set; } = 69;

    public class Items
    {
        public Loadout sandbox_loadout { get; set; } = new();
    }

    public class Loadout
    {
        public string templateId { get; set; } = "CosmeticLocker:cosmeticlocker_athena";
        public Attributes attributes { get; set; } = new();
        public int quantity { get; set; } = 1;

        public void AddCharacter(string id) => attributes.locker_slots_data.slots.Character.items.Add(id);
        public void AddBackpack(string id) => attributes.locker_slots_data.slots.Backpack.items.Add(id);
        public void AddDance(string id) => attributes.locker_slots_data.slots.Dance.items.Add(id);
        public void AddGlider(string id) => attributes.locker_slots_data.slots.Glider.items.Add(id);
        public void AddItemWrap(string id) => attributes.locker_slots_data.slots.ItemWrap.items.Add(id);
        public void AddLoadingScreen(string id) => attributes.locker_slots_data.slots.LoadingScreen.items.Add(id);
        public void AddMusicPack(string id) => attributes.locker_slots_data.slots.MusicPack.items.Add(id);
        public void AddPickaxe(string id) => attributes.locker_slots_data.slots.Pickaxe.items.Add(id);
        public void AddContrail(string id) => attributes.locker_slots_data.slots.SkyDiveContrail.items.Add(id);
    }

    public class Attributes
    {
        public Locker_Slots_Data locker_slots_data { get; set; } = new();
        public int use_count { get; set; } = 1;
        public string banner_icon_template { get; set; } = "BRS11_Prestige5";
        public string locker_name = Config.config.athenaProfileId;
        public string banner_color_template { get; set; } = "DefaultColor40";
        public bool item_seen { get; set; } = false;
        public bool favorite { get; set; } = false;
    }

    public class Locker_Slots_Data
    {
        public Slots slots { get; set; } = new();
    }

    public class Slots
    {
        public Pickaxe Pickaxe { get; set; } = new();
        public Dance Dance { get; set; } = new();
        public Glider Glider { get; set; } = new();
        public Character Character { get; set; } = new();
        public Backpack Backpack { get; set; } = new();
        public ItemWrap ItemWrap { get; set; } = new();
        public LoadingScreen LoadingScreen { get; set; } = new();
        public MusicPack MusicPack { get; set; } = new();
        public SkydiveContrail SkyDiveContrail { get; set; } = new();
    }

    public class Pickaxe
    {
        public List<string> items { get; set; } = new();
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class Dance
    {
        public List<string> items { get; set; } = new();
    }

    public class Glider
    {
        public List<string> items { get; set; } = new();
    }

    public class Character
    {
        public List<string> items { get; set; } = new();
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class ActiveVariant
    {
        public List<Variant> variants { get; set; }
    }

    public class Variant
    {
        public string channel { get; set; }
        public string active { get; set; }
        public List<string> owned { get; set; }
    }

    public class Backpack
    {
        public List<string> items { get; set; } = new();
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class ItemWrap
    {
        public List<string> items { get; set; } = new();
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class LoadingScreen
    {
        public List<string> items { get; set; } = new();
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class MusicPack
    {
        public List<string> items { get; set; } = new();
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class SkydiveContrail
    {
        public List<string> items { get; set; } = new();
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class Stats
    {
        public StatAttributes attributes { get; set; } = new();
    }

    public class StatAttributes
    {
        public int season_match_boost { get; set; } = 999;
        public List<string> loadouts { get; set; } = new() { "sandbox_loadout" };
        public int rested_xp_overflow { get; set; } = 0;
        public bool mfa_reward_claimed { get; set; } = true;
        public Quest_Manager quest_manager { get; set; } = new();
        public int book_level { get; set; } = 1000;
        public int season_num { get; set; } = 15;
        public int season_update { get; set; } = 1;
        public int book_xp { get; set; } = 1;
        public List<object> permissions { get; set; } = new();
        public bool book_purchased { get; set; } = true;
        public int lifetime_wins { get; set; } = 999;
        public string party_assist_quest { get; set; } = string.Empty;
        public List<object> purchased_battle_pass_tier_offers { get; set; } = new();
        public float rested_xp_exchange { get; set; } = 0.333f;
        public int level { get; set; } = 999;
        public long xp_overflow { get; set; } = 999999999999;
        public int rested_xp { get; set; } = 204000;
        public float rested_xp_mult { get; set; } = 12.75f;
        public int accountLevel { get; set; } = 10000;
        public Competitive_Identity competitive_identity { get; set; } = new();
        public int inventory_limit_bonus { get; set; } = 0;
        public string last_applied_loadout { get; set; } = "sandbox_loadout";
        public Daily_Rewards daily_rewards { get; set; } = new();
        public int xp { get; set; } = 10;
        public int season_friend_match_boost { get; set; } = 1;
        public int active_loadout_index { get; set; } = 1;
        public List<Past_Seasons> past_seasons { get; set; } = new();
    }

    public class Quest_Manager
    {
    }

    public class Competitive_Identity
    {
    }

    public class Daily_Rewards
    {
    }

    public class Past_Seasons
    {
        public int seasonNumber { get; set; }
        public int numWins { get; set; } = 10000;
        public int seasonXp { get; set; } = 1000000;
        public int seasonLevel { get; set; } = 500;
        public int bookXp { get; set; } = 1000000;
        public int bookLevel { get; set; } = 500;
        public bool purchasedVIP { get; set; } = true;
    }
}

