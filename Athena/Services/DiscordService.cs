using DiscordRPC;
using Athena.Extensions;

namespace Athena.Services;

public class DiscordService
{
    private const string APP_ID = "1142239120471634042";

    private DiscordRpcClient? _client;
    private static readonly RichPresence _defaultPresence = new()
    {
        Timestamps = new()
        { 
            Start = DateTime.UtcNow 
        },
        Assets = new() 
        { 
            LargeImageKey = "logo",
            LargeImageText = $"Athena {Globals.Version.DisplayName}"
        },
        Buttons = 
        [
            new() { Label = "Download", Url = Globals.GITHUB_URL },
            new() { Label = "Join Athena", Url = Globals.DISCORD_URL }
        ]
    };

    public void Initialize()
    {
        _client = new DiscordRpcClient(APP_ID);
        _client.OnReady += (_, args) => Log.Information("Discord Presence started for {username}", args.User.Username);
        _client.OnError += (_, args) => throw new Exception($"Error while starting Discord RPC: {args.Message}");
        _client.Initialize();
        _client.SetPresence(_defaultPresence);
    }

    public void Update(string text, bool removeSmallAsset = true)
    {
        _client?.UpdateState(text);
        if (removeSmallAsset)
            _client?.UpdateSmallAsset();
    }

    public void Update(EModelType model)
    {
        _client?.UpdateState($"Generating {model.DisplayName()}");
        _client?.UpdateSmallAsset(model.ToString().ToLower(), model.DisplayName());
    }
}