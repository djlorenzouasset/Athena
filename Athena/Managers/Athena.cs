using System.Runtime.InteropServices;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Athena.Models;
using Athena.Services;

namespace Athena.Managers;

public static class Athena
{
    public static async Task Initialize()
    {
        Console.Title = "Athena: Starting"; // do we really need this??

        Log.Logger = new LoggerConfiguration().WriteTo
            .Console(LogEventLevel.Information, theme: AnsiConsoleTheme.Literate).WriteTo
            .File(Path.Combine(DirectoryManager.Logs, $"Athena-Log-{DateTime.Now:dd-MM-yyyy}.log"))
            .CreateLogger();

        /* informations needed for support */
        Log.Information("      --------------------------------------      ");
        Log.Information("Log file opened: {date}", DateTime.Now);
        Log.Information("Athena version is: {ver}", Globals.VERSION);
        Log.Information(".NET version is: {RuntimeVer}", RuntimeInformation.FrameworkDescription);

#if DEBUG
        Log.Information("bIsDebug: {bDebug}", true); // funny
#else
        Log.Information("bIsDebug: {bDebug}", false);
#endif

        Console.Clear(); // clear console after utils logs
        DirectoryManager.CreateFolders();
        Config.LoadSettings();
        // clear the console for a better look in case the user inserted paths 
        Console.Clear();

        DiscordRichPresence.Initialize(); // this will be optional in v2 :/
        await Dataminer.Instance.Initialize();
        await Dataminer.Instance.ShowMenu();
    }
}