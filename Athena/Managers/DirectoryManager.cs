namespace Athena.Managers;

public static class DirectoryManager
{
    public static string Settings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Athena");
    public static string Logs = Path.Combine(Environment.CurrentDirectory, ".logs");
    public static string ChunksDir = Path.Combine(Environment.CurrentDirectory, ".data");
    public static string Profiles = Path.Combine(Environment.CurrentDirectory, ".profiles"); // not really needed
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

    public static bool GetSavedMappings(out string mappingsPath)
    {
        DirectoryInfo mappingsDir = new(MappingsDir);
        var recent = mappingsDir.GetFiles("*.usmap").OrderByDescending(f => f.LastWriteTime).First();
        if (recent is not null)
        {
            mappingsPath = recent.FullName;
            return true;
        }

        mappingsPath = string.Empty;
        return false;
    }
}