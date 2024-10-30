using DiscordRPC;

namespace Athena.Services;

public static class DiscordRichPresence
{
    private static DiscordRpcClient? _client;

    private static readonly RichPresence _default = new()
    {
        Timestamps = new() { Start = DateTime.UtcNow },
        Assets = new() { LargeImageKey = "logo", LargeImageText = $"Athena {Globals.VERSION}" },
        Buttons = [
            new() { Label = "Join Athena", Url = Globals.DISCORD },
            new() { Label = "Download Athena", Url = Globals.DOWNLOAD }
        ]
    };

    public static void Initialize()
    {
        _client = new DiscordRpcClient(Globals.APPID);
        _client.OnReady += (_, args) => Log.Information("Discord Presence started for {username}", args.User.Username);
        _client.OnError += (_, args) => throw new Exception($"Error while starting Discord RPC: {args.Message}");
        _client.Initialize();
        _client.SetPresence(_default);
    }

    public static void Update(string text)
    {
        _client?.UpdateState(text);
    }
}