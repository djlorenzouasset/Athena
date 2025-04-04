using DiscordRPC;
using Athena.Extensions;

namespace Athena.Services;

public static class DiscordRichPresence
{
    private const string APP_ID = "1142239120471634042";

    private static DiscordRpcClient _client = null!; 
    private static readonly RichPresence _defaultPresence = new()
    {
        Timestamps = new() { Start = DateTime.UtcNow },
        Assets = new() { LargeImageKey = "logo", LargeImageText = $"Athena v{Globals.Version}" },
        Buttons = [
            new() { Label = "Download", Url = Globals.GITHUB_URL },
            new() { Label = "Join Athena", Url = Globals.DISCORD_URL }
        ]
    };

    public static void Initialize()
    {
        _client = new DiscordRpcClient(APP_ID);
        _client.OnReady += (_, args) => Log.Information("Discord Presence started for {username}", args.User.Username);
        _client.OnError += (_, args) => throw new Exception($"Error while starting Discord RPC: {args.Message}");
        _client.Initialize();
        _client.SetPresence(_defaultPresence);
    }

    public static void Update(string text)
    {
        _client?.UpdateState(text);
    }

    public static void Update(EModelType model)
    {
        _client?.UpdateState($"Generating {model.GetDescription()}");
    }
}