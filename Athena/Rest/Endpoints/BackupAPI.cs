using RestSharp;
using Athena.Models.API.Fortnite;

namespace Athena.Rest.Endpoints;

public class BackupAPI(RestClient client) : AthenaRestClient(client)
{
    private const string BACKUPS_ENDPOINT = "https://api.fmodel.app/v1/backups/fortnitegame";

    public async Task<Backup[]?> GetBackupsAsync()
    {
        return await ExecuteAsync<Backup[]>(BACKUPS_ENDPOINT);
    }
}