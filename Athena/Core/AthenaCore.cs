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

        Settings.LoadSettings();
        Settings.ValidateSettings();
        Console.Clear(); // clear the console after settings loading and validation

        // TODO: understand if this is really needed (??)
        await Dependencies.EnsureDependencies();

        if (Settings.Current.UseDiscordRPC)
        {
            Discord.Initialize();
        }

        var epicAuth = Settings.Current.EpicAuth;
        if (epicAuth is null || !epicAuth.IsValid())
        {
            if (!await Settings.CreateAuth())
            {
                App.ExitThread(-1);
            }
        }

        await UEParser.Initialize(); // init the parser

        if (Settings.Current.LastDonationPopup.AddDays(7) <= DateTime.UtcNow)
        {
            Settings.Current.LastDonationPopup = DateTime.UtcNow;
            Settings.SaveSettings();

            // TODO: show popup and handle buttons
        }

        await new Generator().ShowMenu();
    }
}