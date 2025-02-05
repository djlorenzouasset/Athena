using Athena.Models.App;

namespace Athena.Services;

public class Updater
{
    public async Task<bool> CheckForUpdate()
    {
        return false;
    }

    public async Task DownloadUpdate()
    {
    }

    public async Task RunUpdater(AthenaVersion newVersion)
    {
    }
}