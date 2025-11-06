using System.Runtime.InteropServices;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Athena.Utils;
using Athena.Services;
using Athena.Models.App;

namespace Athena.Core;

public class AthenaCore
{
    public static async Task Init()
    {
        Console.Title = "Athena: Starting..";

        Log.Logger = new LoggerConfiguration().WriteTo
            .Logger(consoleLogger => consoleLogger.Filter
            .ByExcluding(logEvent => logEvent.Properties.ContainsKey("NoConsole")).WriteTo
            .Console(LogEventLevel.Information, "[{Level:u3}] {Message:lj}{NewLine}{Exception}", theme: AnsiConsoleTheme.Literate)).WriteTo
            .File($"Athena-Log-{DateTime.Now:dd-MM-yyyy}.log")
            .CreateLogger();

        #region DEBUG LOGS
        Log.ForContext("NoConsole", true).Information("      --------------------------------------      ");
        Log.ForContext("NoConsole", true).Information("Log file opened: {0}", DateTime.Now);
        Log.ForContext("NoConsole", true).Information("Athena version: {0}", Globals.Version);
        Log.ForContext("NoConsole", true).Information("Current folder: {0}", Environment.CurrentDirectory);
        Log.ForContext("NoConsole", true).Information("NET version: {0}", RuntimeInformation.FrameworkDescription);
#if DEBUG
        Log.ForContext("NoConsole", true).Information("bIsDebug: {0}", true); // funny
#else
        Log.ForContext("NoConsole", true).Information("bIsDebug: {0}", false);
#endif
        #endregion

        if (!Directories.IsCurrentDirectoryValid())
        {
            Log.Error("You can't run Athena in this directory! Please move it into another folder and try again.");
            Message.Show("Athena", "You can't run Athena in this directory! Please move it into another folder and try again.", 
                Message.MB_ICONERROR | Message.MB_OK
            );

            FUtils.ExitThread(1);
        }

        Directories.CreateDefaultFolders();
#if !DEBUG
        await Updater.Instance.CheckForUpdate();
#endif

        UserSettings.LoadSettings();
        Console.Clear(); // clear the console after settings creation/load

        if (UserSettings.Current.bUseDiscordRPC)
        {
            DiscordRichPresence.Initialize();
        }

        if (UserSettings.Current.EpicAuth is null || !UserSettings.Current.EpicAuth.IsValid())
        {
            if (!await UserSettings.CreateAuth())
            {
                FUtils.ExitThread(1);
            }
        }

        // initialize the dataminer shit
        await Dataminer.Instance.Initialize();

        // initialize main menu (errors are handled inside of that class)
        var generator = new Generator();
        await generator.ShowMenu();
    }
}