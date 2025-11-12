using RestSharp;
using Athena.Models.API.Base;
using Athena.Models.API.Responses;

namespace Athena.Models.API;

public class DillyAPI(RestClient client) : AthenaRestClient(client)
{
    private const string AESKEYS_ENDPOINT = "https://export-service.dillyapis.com/v1/aes";

    public async Task<AESKeys?> GetAESKeysAsync(bool bLog = true)
    {
        return await ExecuteAsync<AESKeys>(AESKEYS_ENDPOINT, Method.Get, bLog);
    }
}