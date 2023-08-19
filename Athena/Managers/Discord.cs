using DiscordRPC;

namespace Athena.Managers;

public static class DiscordRichPresence
{
    private static DiscordRpcClient? Client;

    private static readonly RichPresence Default = new()
    {
        Timestamps = new() { Start = DateTime.UtcNow },
        Assets = new() { LargeImageKey = "logo", LargeImageText = $"Athena {Globals.VERSION}" }
    };

    public static void Initialize()
    {
        Client = new DiscordRpcClient(Globals.APPID);
        Client.OnReady += (_, args) => Log.Information("Discord Rich Presence Started for {Username}", args.User.Username);
        Client.OnError += (_, args) => throw new Exception($"Error while starting Discord RPC: {args.Message}");

        Client.Initialize();
        Client.SetPresence(Default);
    }

    public static void Update(string text)
    {
        Client?.UpdateState(text);
    }
}
