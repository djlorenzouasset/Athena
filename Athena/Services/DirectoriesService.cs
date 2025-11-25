namespace Athena.Services;

public class DirectoriesService
{
    public string Current => Environment.CurrentDirectory;

    public string Data => Path.Combine(Current, ".data");
    public string Logs => Path.Combine(Current, "Logs");
    public string Output => Path.Combine(Current, "Output");
    public string Backups => Path.Combine(Current, "Backups");
    public string Mappings => Path.Combine(Current, "Mappings");

    public void CreateDefaultDirectories()
    {
        Directory.CreateDirectory(Data);
        Directory.CreateDirectory(Logs);
        Directory.CreateDirectory(Output);
        Directory.CreateDirectory(Backups);
        Directory.CreateDirectory(Mappings);
    }

    public string? GetSavedMappings()
    {
        var mapping = new DirectoryInfo(Mappings)
            .GetFiles("*.usmap")
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault();

        return mapping?.FullName;
    }

    public List<FileInfo> GetSavedBackups()
    {
        return [..new DirectoryInfo(Backups)
            .GetFiles("*.fbkp")
            .OrderByDescending(f => f.LastWriteTime)];
    }

    public List<FileInfo> GetCachedChunks(TimeSpan maxLifetime)
    {
        var lifetime = DateTime.Now - maxLifetime;
        return [..new DirectoryInfo(Data)
            .GetFiles("*.*chunk")
            .Where(f => f.LastWriteTime >= lifetime)];
    }
}