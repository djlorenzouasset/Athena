using Newtonsoft.Json;

namespace Athena.Models;

public class ProfileCommonCore
{
    public string _id = Config.config.athenaProfileId;
    public DateTime created = DateTime.Now;
    public DateTime updated = DateTime.Now;
    public int rvn = 100;
    public int wipeNumber = 1;
    public string accountId = Config.config.athenaProfileId;
    public string profileId = "common_core";
    public string version = Config.config.athenaProfileId;
    public Items items = new();
    public Stats stats = new();
}

public class Items
{
    [JsonProperty("Currency:MtxPurchased")]
    public CurrencyMtxPurchased currencyAttributes = new();
}

public class Stats
{
    public StatsAttributes attributes = new();
}

public class CurrencyMtxPurchased
{
    public CurrencyAttributes attributes = new();
    public int quantity { get; set; } = 0;
    public string templateId = "Currency:MtxPurchased";
}

public class CurrencyAttributes
{
    public string platform = "EpicPC";
}

public class StatsAttributes
{
    public string mtx_affiliate = "Neonite";
    public string current_mtx_platform = "EpicPC";
    public object mtx_purchase_history = new(); // not used
}