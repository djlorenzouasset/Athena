namespace Athena.Core;

public class AthenaCore
{
    public async Task Initialize()
    {
        Console.Title = $"Athena {Globals.Version.DisplayName} - Loading";

        Directories.CreateDefaultDirectories();

        App.CreateLogger();
        App.LogDebugInformations();

#if RELEASE
        await Updater.CheckForUpdates();
#endif

        AppSettings.LoadSettings();
        AppSettings.ValidateSettings();
        Console.Clear(); // clear the console after settings loading and validation

        if (AppSettings.Default.UseDiscordRPC)
        {
            Discord.Initialize();
        }

        var epicAuth = AppSettings.Default.EpicAuth;
        if (epicAuth is null || !epicAuth.IsValid())
        {
            if (!await AppSettings.CreateAuth())
            {
                App.ExitThread(-1);
            }
        }

        await UEParser.Initialize(); // init the parser

        if (AppSettings.Default.LastDonationPopup.AddDays(7) < DateTime.UtcNow)
        {
            AppSettings.Default.LastDonationPopup = DateTime.UtcNow;
            AppSettings.SaveSettings();

            // TODO: show popup and handle buttons
        }

        await new Generator().ShowMenu();
    }
}