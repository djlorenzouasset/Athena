using RestSharp;
using Athena.Models.API.Athena;

namespace Athena.Rest.Endpoints;

public class AthenaEndpoints(RestClient client) : AthenaRestClient(client)
{
    private const string RELEASE_ENDPOINT = "https://djlorexzo.dev/api/v1/athena/dev/release";

    public async Task<AthenaRelease?> GetReleaseInfoAsync()
    {
        return await ExecuteAsync<AthenaRelease>(RELEASE_ENDPOINT);
    }
}