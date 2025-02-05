using System.Runtime.InteropServices;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

using Athena.Utils;
using Athena.Services;

namespace Athena.Core;

public class AthenaCore
{
    public static async Task Init()
    {
        Console.Title = "Athena: Starting..";
        Log.Logger = new LoggerConfiguration().WriteTo
            .Logger(consoleLogger => consoleLogger.Filter
            .ByExcluding(logEvent => logEvent.Properties.ContainsKey("NoConsole")).WriteTo
            .Console(LogEventLevel.Information, theme: AnsiConsoleTheme.Literate)).WriteTo
            .File($"Athena-Log-{DateTime.Now:dd-MM-yyyy}.log")
            .CreateLogger();

        Log.ForContext("NoConsole", true).Information("      --------------------------------------      ");
        Log.ForContext("NoConsole", true).Information("Log file opened: {date}", DateTime.Now);
        Log.ForContext("NoConsole", true).Information("Athena version is: {ver}", Globals.Version);
        Log.ForContext("NoConsole", true).Information("Current folder is {path}", Environment.CurrentDirectory);
        Log.ForContext("NoConsole", true).Information(".NET version is: {runtimeVer}", RuntimeInformation.FrameworkDescription);
#if DEBUG
        Log.ForContext("NoConsole", true).Information("bIsDebug: {bDebug}", true); // funny
#else
        Log.ForContext("NoConsole", true).Information("bIsDebug: {bDebug}", false);
#endif

        if (!Directories.IsCurrentDirectoryValid())
        {
            Log.Error("You can't use Athena in this directory. Please move it in another folder and try again.");
            FUtils.ExitThread(1);
        }

        Directories.CreateDefaultFolders();
    }
}