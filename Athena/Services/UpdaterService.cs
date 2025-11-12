using System.Diagnostics;
using Athena.Models.API.Responses;

namespace Athena.Services;

public class UpdaterService
{
    private AthenaRelease? _releaseInfos;

    private readonly string _currentInstallation = Path.Combine(Directories.Current, "Athena.exe");
    private readonly string _tempInstallationFile = Path.Combine(Directories.Data, "Athena.exe");
    private readonly string _updateInstaller = Path.Combine(Environment.GetFolderPath(
        Environment.SpecialFolder.ApplicationData), "Updater", "Updater.exe");

    public async Task CheckForUpdates()
    {
        _releaseInfos = await Api.Athena.GetReleaseInfoAsync();
        if (_releaseInfos is null)
        {
            Log.Warning("Failed to fetch release information.");
            return;
        }

        if (_releaseInfos.Version > Globals.Version)
        {
            uint msgFlag = _releaseInfos.Required ? Message.MB_OK : Message.MB_YESNO | Message.MB_DEFBUTTON1;
            string msgText = _releaseInfos.Required
                ? $"Athena {_releaseInfos.Version.DisplayName} is now available. Install it in order to use the program."
                : $"Athena {_releaseInfos.Version.DisplayName} is now available. Do you want to install it?";

            int bUpdate = Message.Show("Update Available", msgText, msgFlag | Message.MB_ICONINFORMATION);
            if ((_releaseInfos.Required || bUpdate == Message.BT_YES) && await DownloadUpdate())
            {
                RunUpdater();
            }
        }
    }

    private async Task<bool> DownloadUpdate()
    {
        Log.Information("Downloading Athena {version} (Size: {size}mb, Release Date: {releaseDate})",
            _releaseInfos!.Version.DisplayName, _releaseInfos!.UpdateSize, _releaseInfos!.ReleaseDate);

        if (await Api.DownloadFileAsync(_releaseInfos!.DownloadUrl, _tempInstallationFile) is not FileInfo { Exists: true })
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
            $"\"{_releaseInfos!.Version}\""
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