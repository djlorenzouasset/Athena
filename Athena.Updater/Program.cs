using System.Diagnostics;
using System.Runtime.InteropServices;
using Serilog;

class AthenaUpdater
{
    public const uint MB_OK = 0x00000000;
    public const uint MB_ICONERROR = 0x00000010;
    public const uint MB_ICONINFORMATION = 0x00000040;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hInstance, string lpText, string lpCaption, uint type);

    [DllImport("kernel32.dll")]
    private static extern int GetConsoleWindow();

    static void Main(string[] args)
    {
        string logFile = Path.Combine("AthenaUpdaterLogs", "AthenaUpdater.log");
        if (File.Exists(logFile)) File.Delete(logFile);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(logFile).CreateLogger();

        if (args.Length != 3)
        {
            Log.Error($"Invalid arguments! Needed 3. Received {args.Length}");
            ShowMessage("Invalid arguments!", $"Needed 3. Received {args.Length}.", MB_ICONERROR | MB_OK);
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
                ShowMessage("Athena", $"Athena {newVersion} has been installed successfully.", 
                    MB_ICONINFORMATION | MB_OK);
            });
        }
        catch (Exception e)
        {
            Log.Error($"Failed to install Athena {newVersion}.");
            Log.Fatal(e.ToString());
            ShowMessage("An error has occurred!", $"Failed to install Athena {newVersion}: {e.Message}.",
                MB_ICONERROR | MB_OK);
        }
    }

    static bool IsProcessRunning(string processPath)
    {
        var processes = Process.GetProcessesByName("Athena");
        return processes.Any(p => p.MainModule is { } mainModule
            && mainModule.FileName.Equals(processPath.Replace("/", "\\")));
    }

    static int ShowMessage(string title, string caption, uint flags)
    {
        return MessageBox(GetConsoleWindow(), caption, title, flags);
    }
}