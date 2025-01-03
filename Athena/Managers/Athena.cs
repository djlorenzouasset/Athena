using System.Runtime.InteropServices;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Athena.Rest;
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
        Log.Information("Current folder is {path}", DirectoryManager.Current);
        Log.Information(".NET version is: {RuntimeVer}", RuntimeInformation.FrameworkDescription);

#if DEBUG
        Log.Information("bIsDebug: {bDebug}", true); // funny
#else
        Log.Information("bIsDebug: {bDebug}", false);
#endif
        Console.Clear(); // clear console after utils logs

        if (!DirectoryManager.IsCurrentDirectoryValid())
        {
            Log.Error("You can't use Athena in this directory. Please move it in another folder and try again.");
            Helper.ExitThread(1);
        }

        DirectoryManager.CreateFolders();
#if !DEBUG
        // check for updates
        await CheckForUpdates();
#endif

        Config.LoadSettings();
        // clear the console for a better look in case the user inserted paths 
        Console.Clear();

        var latestRelease = await GetLatestRelease();
        var notices = latestRelease?.Notices ?? [];

        DiscordRichPresence.Initialize(); // this will be optional in v2 :/
        await Dataminer.Instance.Initialize(notices);
        await Dataminer.Instance.ShowMenu();
    }

    private static async Task CheckForUpdates()
    {
        var latestRelease = await GetLatestRelease();
        if (latestRelease != null && latestRelease.VersionChanged())
        {
            int handle = Helper.GetConsoleWindow();
            uint flags = 0x00000004 | 0x00000040 | 0x00000000;
            var bmresult = Helper.MessageBox(handle, $"Athena {latestRelease.Version} is now available. Do you want to install it?", "Update Available", flags);

            if (bmresult == 6) // 6 = YES button
            {
                flags = 0x00000040 | 0x00000000 | 0x00000000;
                await DownloadUpdate(latestRelease);
                Helper.MessageBox(handle, "Update installed! The program will now restart.", "Update download finished", flags);
                Helper.ExitThreadAfterUpdate(latestRelease.Version);
            }
        }
    }

    private static async Task<AthenaRelease?> GetLatestRelease()
    {
        return await APIEndpoints.AthenaEndpoints.GetReleaseInfoAsync();
    }

    private static async Task DownloadUpdate(AthenaRelease info)
    {
        if (!File.Exists(DirectoryManager.UpdaterFile))
        {
            Log.Information("Athena-Updater is not installed! Installing it..");
            await APIEndpoints.AthenaEndpoints.DownloadFileAsync(
                info.UpdaterFile, DirectoryManager.UpdaterFile);
        }

        Log.Information("Downloading Athena {ver} [Release Date: {date}]..", info.Version, info.ReleaseDate);
        await APIEndpoints.AthenaEndpoints.DownloadFileAsync(
            info.Download, Path.Combine(DirectoryManager.ChunksDir, "Athena.exe"));

        Log.Information("Athena {ver} has been downloaded!", info.Version);
    }
}