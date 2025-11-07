using Athena.Models.API.Responses;

namespace Athena.Services;

public class DependencyService
{
    public static async Task EnsureDependencies()
    {
        var dependencies = await APIEndpoints.Instance.Athena.GetRequirementsAsync();
        if (dependencies is null)
        {
            Log.Error("Failed to download app dependencies!");
            return;
        }

        foreach (var dep in dependencies)
        {
            var path = Path.Combine(Directories.Data.FullName, dep.Filename);

            if (dep.Required && !File.Exists(path))
            {
                Log.Information("Downloading required file: {0}", dep.Filename);
                await Download(dep, path);
            }
        }
    }

    private static async Task Download(Dependency req, string dest)
    {
        if (!await APIEndpoints.Instance.DownloadFileAsync(req.DownloadUrl, dest))
        {
            Log.Error("Failed to download required file: {0}", req.Filename);
        }
    }
}