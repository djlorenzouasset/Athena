using Athena.Models.API.Base;
using Athena.Models.API.Responses;
using Athena.Services;
using RestSharp;

namespace Athena.Models.API;

public class AthenaEndpoints(RestClient client) : AthenaRestClient(client)
{
    protected override string BaseURL => "https://prod.athena.dev:8000";

    private const string RELEASE_ENDPOINT = "api/v1/version";
    private const string BACKUPS_ENDPOINT = "api/v1/backups";
    private const string MAPPINGS_ENDPOINT = "api/v1/mappings";
    private const string REQUIREMENTS_ENDPOINT = "api/v1/requirements";

    public async Task<AthenaRelease?> GetReleaseInfoAsync()
    {
        return await ExecuteAsync<AthenaRelease>(RELEASE_ENDPOINT);
    }

    public async Task<Backup[]?> GetBackupsAsync()
    {
        return await ExecuteAsync<Backup[]>(BACKUPS_ENDPOINT);
    }

    public async Task<string?> GetMappingAsync()
    {
        var response = await ExecuteAsync<Mappings>(MAPPINGS_ENDPOINT);
        if (response == null) return null;

        string path = Path.Combine(Directories.Mappings.FullName, response.FileName);
        if (!await APIEndpoints.Instance.DownloadFileAsync(response.Url, path, false)) return null;
        return path;
    }

    public async Task<Dependency[]?> GetRequirementsAsync()
    {
        return await ExecuteAsync<Dependency[]>(REQUIREMENTS_ENDPOINT);
    }
}