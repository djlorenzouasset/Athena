using RestSharp;
using Athena.Models.API.Base;
using Athena.Models.API.Responses;

namespace Athena.Models.API;

public class AthenaEndpoints(RestClient client) : AthenaRestClient(client)
{
    //protected override string BaseURL => "http://prod.athena.dev:8000";

    // TODO: make these endpoints available on my localhost (ignore this stupid comment)
    private const string NEWS_ENDPOINT = "http://prod.athena.dev:8000/api/v1/news";
    private const string RELEASE_ENDPOINT = "http://prod.athena.dev:8000/api/v1/version";
    private const string BACKUPS_ENDPOINT = "https://laylaleaks.de/api/backups";
    //private const string MAPPINGS_ENDPOINT = "https://laylaleaks.de/api/mappings";

    public async Task<AthenaRelease?> GetReleaseInfoAsync()
    {
        return await ExecuteAsync<AthenaRelease>(RELEASE_ENDPOINT);
    }

    public async Task<Backup[]?> GetBackupsAsync()
    {
        return await ExecuteAsync<Backup[]>(BACKUPS_ENDPOINT);
    }

    //public async Task<Mappings?> GetMappingAsync()
    //{
    //    // this is very shit I know but is temporarily
    //    var req = await ExecuteAsync<Mappings[]>(MAPPINGS_ENDPOINT);
    //    return req?.FirstOrDefault();
    //}
}