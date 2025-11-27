using System.Diagnostics;
using Athena.Models.API.Responses;

namespace Athena.Services;

public class UpdaterService
{
    public AthenaRelease? ReleaseInfo;

    private readonly string _currentInstallation = Path.Combine(Directories.Current, "Athena.exe");
    private readonly string _tempInstallationFile = Path.Combine(Directories.Data, "Athena.exe");
    private readonly string _updateInstaller = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData), "Updater", "Updater.exe");

    public async Task CheckForUpdates()
    {
        ReleaseInfo = await Api.Athena.GetReleaseInfoAsync();
        if (ReleaseInfo is null)
        {
            Log.Warning("Failed to fetch release information.");
            return;
        }

        if (ReleaseInfo.Version > Globals.Version)
        {
            uint msgFlag = ReleaseInfo.Required ? MessageService.MB_OK : MessageService.MB_YESNO | MessageService.MB_DEFBUTTON1;
            string msgText = ReleaseInfo.Required
                ? $"Athena {ReleaseInfo.Version.DisplayName} is now available. Install it in order to use the program."
                : $"Athena {ReleaseInfo.Version.DisplayName} is now available. Do you want to install it?";

            int bUpdate = MessageService.Show("Update Available", msgText, msgFlag | MessageService.MB_ICONINFORMATION);
            if ((ReleaseInfo.Required || bUpdate == MessageService.BT_YES) && await DownloadUpdate())
            {
                RunUpdater();
            }
        }
    }

    private async Task<bool> DownloadUpdate()
    {
        Log.Information("Downloading Athena {version} (Size: {size}mb, Release Date: {releaseDate})",
            ReleaseInfo!.Version.DisplayName, ReleaseInfo!.UpdateSize, ReleaseInfo!.ReleaseDate);

        if (await Api.DownloadFileAsync(ReleaseInfo!.DownloadUrl, _tempInstallationFile) is not FileInfo { Exists: true })
        {
            Log.Error("Failed to download the update. Contact the staff or download the update manually from GitHub.");
            return false;
        }

        return true;
    }

    private void RunUpdater()
    {
        var args = new[]
        {
            $"\"{_currentInstallation}\"",
            $"\"{_tempInstallationFile}\"",
            $"\"{ReleaseInfo!.Version}\""
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