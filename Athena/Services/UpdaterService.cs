using System.Diagnostics;
using Athena.Models.API.Responses;

namespace Athena.Services;

public class UpdaterService
{
    private readonly string _currentInstallation = Path.Combine(Directories.Current, "Athena.exe");
    private readonly string _tempInstallationFile = Path.Combine(Directories.Data, "Athena.exe");
    private readonly string _updateInstaller = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData), "Updater", "Updater.exe");

    public async Task CheckForUpdates()
    {
        var releaseInfo = App.ReleaseInfo;
        if (releaseInfo is null)
        {
            Log.Warning("Failed to check for updates! Release info was null maybe due a failed request.");
            return;
        }

        if (releaseInfo.Version > Globals.Version)
        {
            uint msgFlag = releaseInfo.Required ? MessageService.MB_OK : MessageService.MB_YESNO | MessageService.MB_DEFBUTTON1;
            string msgText = releaseInfo.Required
                ? $"Athena {releaseInfo.Version.DisplayName} is now available. Install it in order to use the program."
                : $"Athena {releaseInfo.Version.DisplayName} is now available. Do you want to install it?";

            int bUpdate = MessageService.Show("Update Available", msgText, msgFlag | MessageService.MB_ICONINFORMATION);
            if ((releaseInfo.Required || bUpdate == MessageService.BT_YES) && await DownloadUpdate(releaseInfo))
            {
                RunUpdater(releaseInfo.Version.DisplayName);
            }
        }
    }

    private async Task<bool> DownloadUpdate(AthenaRelease releaseInfo)
    {
        Log.Information("Downloading Athena {version} (Size: {size}mb, Release Date: {releaseDate})",
            releaseInfo.Version.DisplayName, releaseInfo.UpdateSize, releaseInfo.ReleaseDate);

        if (await Api.DownloadFileAsync(releaseInfo.DownloadUrl, _tempInstallationFile) is not FileInfo { Exists: true })
        {
            Log.Error("Failed to download the update. Contact the staff or download the update manually from GitHub.");
            return false;
        }

        return true;
    }

    private void RunUpdater(string version)
    {
        var args = new[]
        {
            $"\"{_currentInstallation}\"",
            $"\"{_tempInstallationFile}\"",
            $"\"{version}\""
        };

        var startInfo = new ProcessStartInfo
        {
            FileName = _updateInstaller,
            Arguments = string.Join(' ', args),
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        try
        {
            App.Launch(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Log.Error("Failed to start the updater: {message}", ex.Message);
        }
    }
}