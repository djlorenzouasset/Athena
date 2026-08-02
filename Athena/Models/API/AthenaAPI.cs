using RestSharp;
using Athena.Models.API.Base;
using Athena.Models.API.Responses;

namespace Athena.Models.API;

public class AthenaEndpoints(RestClient client) : AthenaRestClient(client)
{
    protected override string BaseURL => "https://athena-service-prod.djlorexzo.dev";

    private const string NEWS_ENDPOINT = "api/v1/news";
    private const string RELEASE_ENDPOINT = "api/v1/version";
    private const string BACKUPS_ENDPOINT = "api/v1/backups";
    private const string HEARTBEAT_ENDPOINT = "api/v1/analytics/{trackingId}/heartbeat";

    public async Task<AthenaRelease?> GetReleaseInfoAsync()
        => await ExecuteAsync<AthenaRelease>(RELEASE_ENDPOINT);

    public async Task<Backup[]?> GetBackupsAsync()
        => await ExecuteAsync<Backup[]>(BACKUPS_ENDPOINT);

    public async Task<AppNews?> GetNewsAsync()
        => await ExecuteAsync<AppNews>(NEWS_ENDPOINT, bLog: false);

    public async Task<RestResponse> SendHeartbeat(string trackingId)
        => await ExecuteAsync(HEARTBEAT_ENDPOINT, Method.Post, bLog: false, new UrlSegmentParameter("trackingId", trackingId));
}