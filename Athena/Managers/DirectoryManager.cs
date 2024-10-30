namespace Athena.Managers;

public static class DirectoryManager
{
    public static string Current = Environment.CurrentDirectory;
    public static string Settings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Athena");
    public static string Logs = Path.Combine(Current, ".logs");
    public static string ChunksDir = Path.Combine(Current, ".data");
    public static string Profiles = Path.Combine(Current, ".profiles"); // not really needed
    public static string MappingsDir = Path.Combine(ChunksDir, ".mappings");
    public static string BackupsDir = Path.Combine(ChunksDir, ".backups");

    public static void CreateFolders()
    {
        string[] folders = { Logs, Settings, ChunksDir, MappingsDir, BackupsDir, Profiles };

        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
            {
                Log.Information("Created directory {dirPath}", folder);
                Directory.CreateDirectory(folder);
            }
            continue;
        }
    }

    public static string? GetSavedMappings()
    {
        DirectoryInfo mappingsDir = new(MappingsDir);
        var recent = mappingsDir.GetFiles("*.usmap")
            .OrderByDescending(f => f.LastWriteTime).FirstOrDefault();

        if (recent is null)
            return null;

        return recent.FullName;
    }
}