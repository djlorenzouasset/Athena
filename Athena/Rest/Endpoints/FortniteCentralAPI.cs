using RestSharp;
using Athena.Services;
using Athena.Models.API.Fortnite;

namespace Athena.Rest.Endpoints;

public class FortniteCentralAPI(RestClient client) : AthenaRestClient(client)
{
    private const string AESKEYS_ENDPOINT = "https://fortnitecentral.genxgames.gg/api/v1/aes";
    private const string MAPPINGS_ENDPOINT = "https://fortnitecentral.genxgames.gg/api/v1/mappings";

    public async Task<string?> GetMappingsAsync()
    {
        var response = await ExecuteAsync<Mappings[]?>(MAPPINGS_ENDPOINT);
        if (response == null || response.Length == 0) return null;

        string path = Path.Combine(Directories.Mappings.FullName, response[0].FileName);
        if (!await APEndpoints.Instance.DownloadFileAsync(response[0].Url, path, false)) return null;
        return path;
    }

    public async Task<AESKeys?> GetAESKeysAsync(bool bLog = true)
    {
        return await ExecuteAsync<AESKeys>(AESKEYS_ENDPOINT, Method.Get, bLog);
    }
}