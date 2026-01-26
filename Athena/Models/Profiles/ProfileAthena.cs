using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Athena.Models.Profiles;

public class ProfileAthena
{
    public string Id = "aa479043-3602-40be-959b-853003ea8fce";
    public DateTime Created = DateTime.UtcNow;
    public DateTime Updated = DateTime.UtcNow;
    public int Rvn = 100;
    public int WipeNumber = 1;
    public string AccountId = AppSettings.Default.ProfilesSettings.ProfileId;
    public string ProfileId = "athena";
    public string Version = "";
    public Dictionary<string, IProfileItem> Items = new() {
        { "sandbox_loadout", new Loadout() }
    };
    public ProfileStats Stats = new();
    public int CommandRevision = 100;
}

public class Loadout : IProfileItem
{
    public string TemplateId = "CosmeticLocker:cosmeticlocker_athena";
    public LockerAtributes Attributes = new();
    public int Quantity = 1;
}

public class LockerAtributes
{
    [JsonProperty("locker_slots_data")] public LockerSlotsData LockerSlotsData = new();
    [JsonProperty("use_count")] public int UseCount = 1;
    [JsonProperty("banner_icon_template")] public string BannerIconTemplate = "BRS11_Prestige5";
    [JsonProperty("locker_name")] public string LockerName = AppSettings.Default.ProfilesSettings.ProfileId;
    [JsonProperty("banner_color_template")] public string BannerColorTemplate = "DefaultColor40";
    [JsonProperty("item_seen")] public bool ItemSeen = false;
    public bool Favorite = false;
}

public class LockerSlotsData
{
    public Slots Slots = new();
}

public class Slots
{
    [JsonProperty("Pickaxe")] public Pickaxe Pickaxe = new();
    [JsonProperty("Dance")] public Dance Dance = new();
    [JsonProperty("Glider")] public Glider Glider = new();
    [JsonProperty("Character")] public Character Character = new();
    [JsonProperty("Backpack")] public Backpack Backpack = new();
    [JsonProperty("ItemWrap")] public ItemWrap ItemWrap = new();
    [JsonProperty("LoadingScreen")] public LoadingScreen LoadingScreen = new();
    [JsonProperty("MusicPack")] public MusicPack MusicPack = new();
    [JsonProperty("SkyDiveContrail")] public SkydiveContrail SkyDiveContrail = new();
}

public class Pickaxe
{
    public List<string> Items = [""];
    public List<ActiveVariant> ActiveVariants = [];
}

public class Dance
{
    public List<string> Items = [""];
}

public class Glider
{
    public List<string> Items = [""];
}

public class Character
{
    public List<string> Items = ["AthenaCharacter:CID_001_Athena_Commando_F_Default"];
    public List<ActiveVariant> ActiveVariants = [];
}

public class Backpack
{
    public List<string> Items = [""];
    public List<ActiveVariant> ActiveVariants = [];
}

public class ItemWrap
{
    public List<string> Items = [""];
    public List<ActiveVariant> ActiveVariants = [];
}

public class LoadingScreen
{
    public List<string> Items = [""];
    public List<ActiveVariant> ActiveVariants = [];
}

public class MusicPack
{
    public List<string> Items = [""];
    public List<ActiveVariant> ActiveVariants = [];
}

public class SkydiveContrail
{
    public List<string> Items = [""];
    public List<ActiveVariant> ActiveVariants = [];
}

public class ActiveVariant
{
    public List<Variant> Variants = [];
}

public class ProfileStats
{
    public StatAttributes Attributes = new();
}

[JsonObject(NamingStrategyType = typeof(DefaultNamingStrategy))] //easiest way to do this. makes sure serializer doesn't change property names
public class StatAttributes
{
    public int season_match_boost = 999999;
    public List<string> loadouts = ["sandbox_loadout"];
    public int rested_xp_overflow = 0;
    public bool mfa_reward_claimed = true;
    public QuestManager quest_manager = new();
    public int book_level = 1;
    public int season_num = 999999;
    public int season_update = 1;
    public int book_xp = 1;
    public List<object> permissions = [];
    public bool book_purchased = true;
    public int lifetime_wins = 1;
    public string party_assist_quest = string.Empty;
    public List<object> purchased_battle_pass_tier_offers = [];
    public float rested_xp_exchange = 1f;
    public int level = AppSettings.Default.ProfilesSettings.BattlePassLevel;
    public long xp_overflow = 1;
    public int rested_xp = 1;
    public float rested_xp_mult = 1f;
    public int accountLevel = AppSettings.Default.ProfilesSettings.BattlePassLevel;
    public CompetitiveIdentity competitive_identity = new();
    public int inventory_limit_bonus = 0;
    public string last_applied_loadout = "sandbox_loadout";
    public DailyRewards daily_rewards = new();
    public int xp = 10;
    public int season_friend_match_boost = 1;
    public int active_loadout_index = 1;
    public List<PastSeasons> past_seasons = [];
}

// unused classes
public class QuestManager;
public class CompetitiveIdentity;
public class DailyRewards;
public class PastSeasons;