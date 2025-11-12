using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using Athena.Models.API;

namespace Athena.Services;

public class APIService
{
    public readonly DillyAPI Dilly;
    public readonly EpicGamesAPI EpicGames;
    public readonly AthenaEndpoints Athena;

    private readonly RestClient _client;

    public APIService()
    {
        _client = new RestClient(new RestClientOptions
        {
            Timeout = TimeSpan.FromMilliseconds(5 * 1000),
            UserAgent = $"Athena/{Globals.Version.DisplayName}"
        }, configureSerialization: s => s.UseSerializer<JsonNetSerializer>());

        Athena = new(_client);
        EpicGames = new(_client);
        Dilly = new(_client);
    }

    public async Task<FileInfo> DownloadFileAsync(string url, string path, bool bOverwrite = true)
    {
        var request = new RestRequest(url);
        var fileData = await _client.DownloadDataAsync(request);
        if (fileData is not null && (!File.Exists(path) || bOverwrite))
        {
            await File.WriteAllBytesAsync(path, fileData);
        }

        return new FileInfo(path);
    }
}