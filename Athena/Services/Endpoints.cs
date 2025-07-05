using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Athena.Rest.Endpoints;

namespace Athena.Services;

public class APEndpoints
{
    public static APEndpoints Instance = new();

    public readonly BackupAPI Backups;
    public readonly AthenaEndpoints Athena;
    public readonly EpicGamesAPI EpicGames;
    public readonly FortniteCentralAPI FortniteCentral;

    private readonly RestClient _client = new(new RestClientOptions
    {
        Timeout = TimeSpan.FromMilliseconds(5 * 1000),
        UserAgent = $"Athena/v{Globals.Version}"
    }, configureSerialization: s => s.UseSerializer<JsonNetSerializer>());

    public APEndpoints()
    {
        Athena = new(_client);
        Backups = new(_client);
        EpicGames = new(_client);
        FortniteCentral = new(_client);
    }

    public async Task<bool> DownloadFileAsync(string url, string path, bool bOverwrite = true)
    {
        var request = new RestRequest(url);
        var data = await _client.DownloadDataAsync(request);
        if (data is null) return false;

        if (!File.Exists(path) || bOverwrite)
        {
            var file = new FileInfo(path);
            await File.WriteAllBytesAsync(file.FullName, data);
        }

        return true;
    }
}