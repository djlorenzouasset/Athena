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
        _client.OnError += (_, args) => Log.Error($"Error while starting Discord RPC: {args.Message}");
        _client.Initialize();
        _client.SetPresence(_defaultPresence);
    }

    public void Update(string text, string icon = "")
    {
        _client?.UpdateState(text);
        _client?.UpdateSmallAsset(icon);
    }

    public void Update(EModelType model)
    {
        var modelName = model.DisplayName();
        _client?.UpdateState($"Generating {modelName}");
        _client?.UpdateSmallAsset(model.ToString().ToLower(), modelName);
    }
}