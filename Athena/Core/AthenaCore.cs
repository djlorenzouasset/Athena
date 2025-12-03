using Athena.Services;

namespace Athena.Core;

public class AthenaCore
{
    public static async Task Initialize()
    {
        Console.Title = $"Athena {Globals.Version.DisplayName} - Loading";

        Directories.CreateDefaultDirectories();

        App.CreateLogger();
        App.LogDebugInformations();

        AppSettings.LoadSettings();
        AppSettings.ValidateSettings();
        Console.Clear(); // clear the console after settings loading and validation

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            App.ExitThread(0, true);
        };
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
        {
            var ex = e.ExceptionObject as Exception;

            string msg = ex!.Message;
            if (e.IsTerminating)
            {
                msg += $"\n\nAthena will now close. Please contact the staff in {Globals.DISCORD_URL} if you still see this message.";
            }

            MessageService.Show("An error has occurred!", msg, MessageService.MB_ICONERROR | MessageService.MB_OK);
            if (e.IsTerminating)
            {
                App.ExitThread(-1, true);
            }
        };

        await App.InitializeVersionInfo();
#if RELEASE
        await Updater.CheckForUpdates();
#endif

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

#if RELEASE
        if (AppSettings.Default.LastDonationPopup.AddDays(7) < DateTime.UtcNow)
        {
            _ = Task.Run(() =>
            {
                var result = MessageService.Show(
                    "Enjoying Athena?", 
                    "Consider donating to support the development of Athena! It would be really appreciated. <3\n\n", 
                    MessageService.MB_ICONINFORMATION | MessageService.MB_YESNO);

                if (result == MessageService.BT_YES)
                {
                    Log.ForContext("NoConsole", true).Debug("Launching donation page {URL}", Globals.DONATIONS_URL);
                    App.Launch(Globals.DONATIONS_URL);
                }
            });

            AppSettings.Default.LastDonationPopup = DateTime.UtcNow;
        }
#endif

        var generator = new Generator();
        generator.LoadAvailableArchives();
        await generator.ShowMenu();
    }
}