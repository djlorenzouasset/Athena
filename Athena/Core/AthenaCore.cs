using Athena.Services;

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
            _ = Task.Run(() =>
            {
                var result = MessageService.Show(
                    "Enjoying Athena?", 
                    "Consider donating to support the development of Athena! It would be really appreciated.\n\n" +
                    "Note: If you donate you will receive the Donator role in the Discord server! Make sure to send a message there to get it.", 
                    MessageService.MB_ICONINFORMATION | MessageService.MB_YESNO);

                if (result == MessageService.BT_YES)
                {
                    Log.ForContext("NoConsole", true).Debug("Launching donation page {URL}", Globals.DONATIONS_URL);
                    App.Launch(Globals.DONATIONS_URL);
                }
            });

            AppSettings.Default.LastDonationPopup = DateTime.UtcNow;
            AppSettings.SaveSettings();
        }

        var generator = new Generator();
        generator.LoadAvailableArchives();
        await generator.ShowMenu();
    }
}