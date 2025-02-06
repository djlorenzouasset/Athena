using System.Diagnostics;
using Athena.Utils;
using Athena.Models.API.Athena;

namespace Athena.Services;

public class Updater
{
    public static Updater Instance = new();

    private AthenaRelease Release = null!;
    private readonly string _currentInstance = Path.Combine(Environment.CurrentDirectory, "Athena.exe");
    private readonly string _tempReleaseFile = Path.Combine(Directories.Data.FullName, "Athena.exe");
    private readonly string _updaterFile = Path.Combine(Directories.Data.FullName, "AthenaUpdater.exe");
    private readonly string _updaterUrl = "https://github.com/djlorenzouasset/Athena/raw/refs/heads/main/Athena.Updater/AthenaUpdater.exe";

    public async Task CheckForUpdate()
    {
        var release = await APIEndpoints.Athena.GetReleaseInfoAsync();
        if (release is null)
        {
            Log.Warning("Failed to request Athena release informations.");
            return;
        }

        Release = release;
        if (Release.Version > Globals.Version)
        {
            var bUpdate = Message.Show("Update available!", $"Athena {Release.Version.DisplayName} is now available. Do you want to install it?", 
                Message.MB_YESNO | Message.MB_DEFBUTTON1 | Message.MB_ICONINFORMATION
            );

            if (bUpdate == Message.BT_YES && await DownloadUpdate())
            {
                RunUpdater();
            }
        }
    }

    private async Task<bool> DownloadUpdate()
    {
        if (!File.Exists(_updaterFile))
        {
            Log.Information("Athena-Updater is not installed! Installing it..");
            if (!await APIEndpoints.DownloadFileAsync(_updaterUrl, _updaterFile))
            {
                Log.Fatal("Failed to download Athena-Updater. Please contact the staff or download the update manually from github.");
                return false;
            }
        }

        Log.Information("Downloading Athena {ver} [Size: {size}, Release Date: {date}]..", 
            Release.Version.DisplayName, Release.UpdateSize, Release.ReleaseDate);

        if (!await APIEndpoints.DownloadFileAsync(Release.DownloadUrl, _tempReleaseFile))
        {
            Log.Fatal("Failed to update Athena. Please contact the staff or download the update manually from github.");
            return false;
        }

        Log.Information("Athena {ver} has been downloaded!", Release.Version);
        return true;
    }

    private void RunUpdater()
    {
        Log.Information("Starting Athena updater.");

        var startInfo = new ProcessStartInfo
        {
            FileName = _updaterFile,
            Arguments = $"\"{_currentInstance}\" \"{_tempReleaseFile}\" \"{Release.Version}\"",
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
            Log.Fatal("Failed to update Athena: {exc}", e.Message);
        }

        FUtils.ExitThread();
    }
}