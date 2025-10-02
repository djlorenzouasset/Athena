global using Serilog;

public static class Globals
{
    public const string VERSION = "1.8.0.1";
    public const string APPID = "1142239120471634042";
    public const string DISCORD = "https://discord.gg/nJBj9NjUS4";
    public const string DOWNLOAD = "https://github.com/djlorenzouasset/Athena/releases/latest";
    public const string RELEASE = "https://djlorexzo.dev/api/v1/athena/release";

    // endpoints
    public const string MAPPINGS = "https://laylaleaks.de/api/mappings";
    public const string AESKEYS = "https://fortnitecentral.genxgames.gg/api/v1/aes";
    public const string BACKUPS = "https://www.laylaleaks.de/api/backups";
    public const string AUTH = "https://account-public-service-prod03.ol.epicgames.com/account/api/oauth/token";
    public const string VERIFY = "https://account-public-service-prod.ol.epicgames.com/account/api/oauth/verify";
    public const string MANIFEST = "https://launcher-public-service-prod06.ol.epicgames.com/launcher/api/public/assets/v2/platform/Windows/namespace/fn/catalogItem/4fe75bbc5a674f4f9b356b5c90567da5/app/Fortnite/label/Live";
    
    // auth
    public const string BASIC = "basic M2Y2OWU1NmM3NjQ5NDkyYzhjYzI5ZjFhZjA4YThhMTI6YjUxZWU5Y2IxMjIzNGY1MGE2OWVmYTY3ZWY1MzgxMmU=";

    /// <summary>
    /// DO NOT ENABLE THIS!! ENDPOINTS ARE NOT LIVE!!
    /// </summary>
    public static bool bUseV2Endpoints = false; // non-const because we change it at runtime
}