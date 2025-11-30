using Serilog;
using System.Diagnostics;
using Athena.Core;
using Athena.Services;

class AthenaUpdater
{
    static void Main(string[] args)
    {
        string logFile = Path.Combine(AthenaServices.Directories.Logs, "AthenaUpdater.log");
        if (File.Exists(logFile)) File.Delete(logFile);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logFile).CreateLogger();

        if (args.Length != 3)
        {
            Log.Error($"Invalid arguments! Needed 3. Received {args.Length}");
            MessageService.Show("Invalid arguments!", $"Needed 3. Received {args.Length}.",
                MessageService.MB_ICONERROR | MessageService.MB_OK);
            return;
        }

        string appPath = args[0].Replace("\"", "");
        string releasePath = args[1].Replace("\"", "");
        string newVersion = args[2];
        Log.Debug($"Logging out received arguments:\n  - {appPath}\n  - {releasePath}\n  - {newVersion}");

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
            Task.Run(() =>
            {
                MessageService.Show("Athena", $"Athena {newVersion} has been installed successfully.", 
                    MessageService.MB_ICONINFORMATION | MessageService.MB_OK);
            });
        }
        catch (Exception e)
        {
            Log.Error($"Failed to install Athena {newVersion}.");
            Log.Fatal(e.ToString());
            MessageService.Show("An error has occurred!", $"Failed to install Athena {newVersion}: {e.Message}.",
                MessageService.MB_ICONERROR | MessageService.MB_OK);
        }
    }

    static bool IsProcessRunning(string processPath)
    {
        var processes = Process.GetProcessesByName("Athena");
        return processes.Any(p => p.MainModule is { } mainModule
            && mainModule.FileName.Equals(processPath.Replace("/", "\\")));
    }
}