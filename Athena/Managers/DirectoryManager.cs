using Athena.Models;

namespace Athena.Managers;

public static class DirectoryManager
{
    // general
    public static string Logs = Path.Combine(Environment.CurrentDirectory, "Logs");
    public static string Settings = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Athena");

    // All folder 
    public static string ChunksDir = Path.Combine(Environment.CurrentDirectory, ".data");
    public static string MappingsDir = Path.Combine(Environment.CurrentDirectory, ".mappings");
    public static string BackupsDir = Path.Combine(Environment.CurrentDirectory, ".backups");
    public static string Profiles = Path.Combine(Environment.CurrentDirectory, "profiles"); // optional folder

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