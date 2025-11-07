using System.Diagnostics;
using Athena.Utils;
using Athena.Models.API.Responses;

namespace Athena.Services;

public class Updater
{
    public static Updater Instance = new();

    private AthenaRelease Release = null!;
    private readonly string _currentFile = Path.Combine(Environment.CurrentDirectory, "Athena.exe");
    private readonly string _tmpReleaseFile = Path.Combine(Directories.Data.FullName, "Athena.exe");
    private readonly string _updaterFile = Path.Combine(Directories.Data.FullName, "AthenaUpdater.exe");

    public async Task CheckForUpdate()
    {
        var release = await APIEndpoints.Instance.Athena.GetReleaseInfoAsync();
        if (release is null)
        {
            Log.Warning("Failed to request Athena release informations.");
            return;
        }

        Release = release;
        if (Release.Version > Globals.Version)
        {
            uint flags;
            string msg;

            if (Release.Required)
            {
                flags = Message.MB_OK | Message.MB_ICONINFORMATION;
                msg = $"Athena {Release.Version.DisplayName} is now available. Install it in order to use the program.";
            }
            else
            {
                flags = Message.MB_YESNO | Message.MB_DEFBUTTON1 | Message.MB_ICONINFORMATION;
                msg = $"Athena {Release.Version.DisplayName} is now available. Do you want to install it?";
            }

            var bUpdate = Message.Show("Update available!", msg, flags);
            if ((Release.Required || bUpdate == Message.BT_YES) && await DownloadUpdate())
            {
                RunUpdater();
            }
        }
    }

    private async Task<bool> DownloadUpdate()
    {
        Log.Information("Downloading Athena {0} (Size: {1}, Release Date: {2})", 
            Release.Version.DisplayName, Release.UpdateSize, Release.ReleaseDate);

        if (!await APIEndpoints.Instance.DownloadFileAsync(Release.DownloadUrl, _tmpReleaseFile))
        {
            Log.Fatal("Failed to update Athena. Please contact the staff or download the update manually from github.");
            return false;
        }

        Log.Information("Athena {0} has been downloaded!", Release.Version);
        return true;
    }

    private void RunUpdater()
    {
        Log.Information("Starting Athena updater.");

        var startInfo = new ProcessStartInfo
        {
            FileName = _updaterFile,
            Arguments = $"\"{_currentFile}\" \"{_tmpReleaseFile}\" \"{Release.Version}\"",
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception e)
        {
            Log.Fatal("Failed to update Athena: {0}", e.Message);
        }

        SettingsService.Current.bShowChangelog = true;
        SettingsService.SaveSettings();

        AthenaUtils.ExitThread();
    }
}