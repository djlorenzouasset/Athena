using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Athena.Rest.Endpoints;

namespace Athena.Services;

public static class APIEndpoints
{
    private static readonly RestClient _client = new(new RestClientOptions
    {
        Timeout = TimeSpan.FromMilliseconds(5 * 1000),
        UserAgent = $"Athena/v{Globals.Version}"
    }, configureSerialization: s => s.UseSerializer<JsonNetSerializer>());

    public static readonly BackupAPI Backups = new(_client);
    public static readonly AthenaEndpoints Athena = new(_client);
    public static readonly EpicGamesAPI EpicGames = new(_client);
    public static readonly FortniteCentralAPI FortniteCentral = new(_client);

    public static async Task<bool> DownloadFileAsync(string url, string path, bool bOverride = true)
    {
        var request = new RestRequest(url);
        var data = await _client.DownloadDataAsync(request);
        if (data is null) return false;

        if (!File.Exists(path) || bOverride)
        {
            var file = new FileInfo(path);
            await File.WriteAllBytesAsync(file.FullName, data);
        }

        return true;
    }
}