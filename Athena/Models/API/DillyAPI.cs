using RestSharp;
using Athena.Models.API.Base;
using Athena.Models.API.Responses;

namespace Athena.Models.API;

public class DillyAPI(RestClient client) : AthenaRestClient(client)
{
    protected override string BaseURL => "https://api.fortniteapi.com/";

    private const string AESKEYS_ENDPOINT = "v1/aes";
    private const string MAPPINGS_ENDPOINT = "v1/mappings";

    public async Task<AESKeys?> GetAESKeysAsync(bool bLog = true)
        => await ExecuteAsync<AESKeys>(AESKEYS_ENDPOINT, Method.Get, bLog);

    public async Task<Mappings?> GetMappingAsync()
    {
        var req = await ExecuteAsync<Mappings[]>(MAPPINGS_ENDPOINT);
        return req?.FirstOrDefault();
    }
}