namespace Athena.Services;

public class DependencyService
{
    public async Task EnsureDependencies()
    {
        var dependencies = await Api.Athena.GetRequirementsAsync();
        if (dependencies is null)
        {
            Log.Error("Failed to download dependencies!");
            return;
        }

        // TODO: make this shit better
        foreach (var dep in dependencies)
        {
            var path = Path.Combine(Directories.Data, dep.Filename);
            if (dep.Required && !File.Exists(path))
            {
                Log.Information("Downloading required file: {filename}", dep.Filename);
                if (!await dep.Download(path))
                    Log.Error("Failed to download required file: {filename}", dep.Filename);
            }
        }
    }
}