namespace Athena.Services;

public class Directories
{
    public static readonly DirectoryInfo Data = new(Path.Combine(Environment.CurrentDirectory, ".data"));
    public static readonly DirectoryInfo Mappings = new(Path.Combine(Data.FullName, ".mappings"));

    public static readonly DirectoryInfo Logs = new(Path.Combine(Environment.CurrentDirectory, "Logs"));
    public static readonly DirectoryInfo Backups = new(Path.Combine(Environment.CurrentDirectory, "Backups"));
    public static readonly DirectoryInfo Output = new(Path.Combine(Environment.CurrentDirectory, "Output"));

    private static List<string> _prohibitedDirs = [
        Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
    ];

    public static bool IsCurrentDirectoryValid()
        => !_prohibitedDirs.Contains(Environment.CurrentDirectory);

    public static void CreateDefaultFolders()
    {
        CreateFolder(Data);
        CreateFolder(Logs);
        CreateFolder(Output);
        CreateFolder(Backups);
        CreateFolder(Mappings);
    }

    private static void CreateFolder(DirectoryInfo folder)
    {
        if (!folder.Exists)
        {
            folder.Create();
            Log.Information("Created directory {dir}", folder.FullName);
        }
    }

    public static string? GetSavedMappings()
    {
        var recent = Mappings.GetFiles("*.usmap")
            .OrderByDescending(f => f.LastWriteTime).FirstOrDefault();

        if (recent is null)
            return null;

        return recent.FullName;
    }
}