using RestSharp;
using Athena.Models.API.Base;
using Athena.Models.API.Responses;

namespace Athena.Models.API;

public class UEDBAPI(RestClient client) : AthenaRestClient(client)
{
    protected override string BaseURL => "https://uedb.dev/svc/api/v1/fortnite";

    private const string AESKEYS_ENDPOINT = "aes";
    private const string MAPPINGS_ENDPOINT = "mappings";

    public async Task<AESKeys?> GetAESKeysAsync(bool bLog = true)
        => await ExecuteAsync<AESKeys>(AESKEYS_ENDPOINT, Method.Get, bLog);

    public async Task<UedbMappingResponse?> GetMappingAsync()
        => await ExecuteAsync<UedbMappingResponse>(MAPPINGS_ENDPOINT);
}