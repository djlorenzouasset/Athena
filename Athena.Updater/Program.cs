using System.Diagnostics;
using Serilog;

class Updater
{
    private const string _logPath = ".logs/Athena-Updater.log";

    static void Main(string[] args)
    {
        if (File.Exists(_logPath)) File.Delete(_logPath);

        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(_logPath).CreateLogger();

        if (args.Length != 3)
        {
            Log.Error($"Invalid arguments! Neeed 3. Received {args.Length}");
            return;
        }

        string appPath = args[0].Replace("\"", "");
        string releasePath = args[1].Replace("\"", "");
        string newVersion = args[2];
        Log.Information($"Logging out received arguments:\n  - {appPath}\n  - {releasePath}\n  - {newVersion}");

        try
        {
            while (IsProcessRunning(Path.GetFileNameWithoutExtension(appPath))) { };

            File.Move(releasePath, appPath, true);
            Process.Start(new ProcessStartInfo
            {
                FileName = appPath,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Normal
            });
            Log.Information($"Started Athena {newVersion}.");
        }
        catch (Exception e)
        {
            Log.Error($"Failed to install Athena {newVersion}.");
            Log.Fatal(e.ToString());
        }
    }

    // fnporting
    static bool IsProcessRunning(string processPath)
    {
        var processes = Process.GetProcessesByName("Athena");
        return processes.Any(p => p.MainModule is { } mainModule 
            && mainModule.FileName.Equals(processPath.Replace("/", "\\")));
    }
}
