using Athena.Services;

public class Cosmetic
{
    public string TemplateId { get; set; }
    public Attributes Attributes = new();
    public int Quantity = 1;

    public Cosmetic(string cosmeticId, string backendType, List<ProfileAthena.Variant>? variants = null)
    {
        TemplateId = $"{backendType}:{cosmeticId}";
        Attributes.Favorite = false;
        Attributes.Archived = false;

        if (variants != null && variants.Count > 0)
        {
            Attributes.Variants.AddRange(variants);
        }
    }
}

public class Attributes
{
    // public DateTime CreationTime = DateTime.UtcNow; this causes no variants on >18.00 (???)
    public int MaxLevelBonus = 0;
    public int Level = 1;
    public bool ItemSeen = true;
    public int RndSelCnt = 0;
    public int Xp = 0;
    public List<ProfileAthena.Variant> Variants = [];
    public bool Favorite = false;
    public bool Archived = false;
}

public class ProfileAthena
{
    public string Id = "aa479043-3602-40be-959b-853003ea8fce";
    public DateTime Created = DateTime.UtcNow;
    public DateTime Updated = DateTime.UtcNow;
    public int Rvn = 100;
    public int WipeNumber = 1;
    public string AccountId = SettingsService.Current.Profiles.ProfileId;
    public string ProfileId = "athena";
    public string Version = "";
    public ProfileItems Items = new();
    public ProfileStats Stats = new();
    public int CommandRevision = 100;

    public class ProfileItems
    {
        public Loadout SandboxLoadout = new();
    }

    public class Loadout
    {
        public string TemplateId = "CosmeticLocker:cosmeticlocker_athena";
        public Attributes Attributes = new();
        public int Quantity = 1;
    }

    public class Attributes
    {
        public LockerSlotsData LockerSlotsData = new();
        public int UseCount = 1;
        public string BannerIconTemplate = "BRS11_Prestige5";
        public string LockerName = SettingsService.Current.Profiles.ProfileId;
        public string BannerColorTemplate = "DefaultColor40";
        public bool ItemSeen = false;
        public bool Favorite = false;
    }

    public class LockerSlotsData
    {
        public Slots Slots = new();
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

    public class Variant
    {
        public string Channel { get; set; }
        public string Active { get; set; }
        public List<string> Owned { get; set; }
    }

    public class ProfileStats
    {
        public StatAttributes Attributes = new();
    }

    public class StatAttributes
    {
        public int SeasonMatchBoost = 999999;
        public List<string> Loadouts = ["sandbox_loadout"];
        public int RestedXpOverflow = 0;
        public bool MfaRewardClaimed = true;
        public QuestManager QuestManager = new();
        public int BookLevel = -1;
        public int SeasonNum = 999999;
        public int SeasonUpdate = -1;
        public int BookXp = -1;
        public List<object> Permissions = [];
        public bool BookPurchased = true;
        public int LifetimeWins = -1;
        public string PartyAssistQuest = string.Empty;
        public List<object> PurchasedBattlePassTierOffers = [];
        public float RestedXpExchange = -1f;
        public int Level = SettingsService.Current.Profiles.BattlepassLevel;
        public long XpOverflow = -1;
        public int RestedXp = -1;
        public float RestedXpMult = -1f;
        public int AccountLevel = SettingsService.Current.Profiles.BattlepassLevel;
        public CompetitiveIdentity CompetitiveIdentity = new();
        public int InventoryLimitBonus = 0;
        public string LastAppliedLoadout = "sandbox_loadout";
        public DailyRewards DailyRewards = new();
        public int Xp = 10;
        public int SeasonFriendMatchBoost = 1;
        public int ActiveLoadoutIndex = 1;
        public List<PastSeasons> PastSeasons = [];
    }

    public class QuestManager
    {
    }

    public class CompetitiveIdentity
    {
    }

    public class DailyRewards
    {
    }

    public class PastSeasons
    {
    }
}