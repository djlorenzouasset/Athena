namespace Athena.Models;

public class ProfileAthena
{
    public string _id = Config.config.athenaProfileId;
    public DateTime created = DateTime.Now;
    public DateTime updated = DateTime.Now;
    public int rvn = 100;
    public int wipeNumber = 1;
    public string accountId = Config.config.athenaProfileId;
    public string profileId = "athena";
    public string version = Config.config.athenaProfileId;
    public Items items { get; set; } = new();
    public Stats stats { get; set; } = new();
    public int commandRevision = 100;

    public class Items
    {
        public Loadout sandbox_loadout { get; set; } = new();
    }

    public class Loadout
    {
        public string templateId = "CosmeticLocker:cosmeticlocker_athena";
        public Attributes attributes { get; set; } = new();
        public int quantity = 1;
    }

    public class Attributes
    {
        public Locker_Slots_Data locker_slots_data { get; set; } = new();
        public int use_count = 1;
        public string banner_icon_template = "BRS11_Prestige5";
        public string locker_name = Config.config.athenaProfileId;
        public string banner_color_template = "DefaultColor40";
        public bool item_seen = false;
        public bool favorite = false;
    }

    public class Locker_Slots_Data
    {
        public Slots slots { get; set; } = new();
    }

    public class Slots
    {
        public Pickaxe Pickaxe = new();
        public Dance Dance = new();
        public Glider Glider = new();
        public Character Character = new();
        public Backpack Backpack = new();
        public ItemWrap ItemWrap = new();
        public LoadingScreen LoadingScreen = new();
        public MusicPack MusicPack = new();
        public SkydiveContrail SkyDiveContrail = new();
    }

    public class Pickaxe
    {
        public List<string> items = new() { null };
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class Dance
    {
        public List<string> items = new() { null };
    }

    public class Glider
    {
        public List<string> items = new() { null };
    }

    public class Character
    {
        public List<string> items = new() { null };
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
        public List<string> items = new() { null };
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class ItemWrap
    {
        public List<string> items = new() { null };
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class LoadingScreen
    {
        public List<string> items = new() { null };
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class MusicPack
    {
        public List<string> items = new() { null };
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class SkydiveContrail
    {
        public List<string> items = new() { null };
        public List<ActiveVariant> activeVariants { get; set; } = new();
    }

    public class Stats
    {
        public StatAttributes attributes { get; set; } = new();
    }

    public class StatAttributes
    {
        public int season_match_boost = 999;
        public List<string> loadouts = new() { "sandbox_loadout" };
        public int rested_xp_overflow = 0;
        public bool mfa_reward_claimed = true;
        public Quest_Manager quest_manager = new();
        public int book_level = 1000;
        public int season_num = 15;
        public int season_update = 1;
        public int book_xp = 1;
        public List<object> permissions = new();
        public bool book_purchased = true;
        public int lifetime_wins = 999;
        public string party_assist_quest = string.Empty;
        public List<object> purchased_battle_pass_tier_offers = new();
        public float rested_xp_exchange = 0.333f;
        public int level = 999;
        public long xp_overflow = 999999999999;
        public int rested_xp = 204000;
        public float rested_xp_mult = 12.75f;
        public int accountLevel = 10000;
        public Competitive_Identity competitive_identity = new();
        public int inventory_limit_bonus = 0;
        public string last_applied_loadout = "sandbox_loadout";
        public Daily_Rewards daily_rewards = new();
        public int xp = 10;
        public int season_friend_match_boost = 1;
        public int active_loadout_index = 1;
        public List<Past_Seasons> past_seasons = new();
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
        public int seasonNumber = 0;
        public int numWins = 10000;
        public int seasonXp = 1000000;
        public int seasonLevel = 500;
        public int bookXp = 1000000;
        public int bookLevel = 500;
        public bool purchasedVIP = true;
    }
}

