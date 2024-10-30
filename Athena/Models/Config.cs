using Spectre.Console;
using Newtonsoft.Json;
using Athena.Managers;

namespace Athena.Models;

public class Config
{
    public static Config config = null!;

    public string athenaProfileId { get; set; } = "[PH] LoadOut_01";
    public string accessToken { get; set; } = string.Empty; // for who download the program, to avoid errors
    public string profileDirectory { get; set; } = DirectoryManager.Profiles;
    public string shopDirectory { get; set; } = DirectoryManager.Profiles;

    public static void LoadSettings()
    {
        config = new Config();

        if (!File.Exists(Path.Combine(DirectoryManager.Settings, "settings.json")))
        {
            // name for the loadout
            config.athenaProfileId = AnsiConsole.Ask<string>("Insert the [62]name[/] to use for the [62]Profile-Athena[/]:");

            // ask the path where the profile will be saved
            profileSaveQuestion:
            string profileSavePath = AnsiConsole.Ask<string>("Insert the [62]path[/] to use for save the [62]Profile-Athena[/] (type [62]d[/] for use the default one):");
            if (profileSavePath == "d")
            {
                config.profileDirectory = DirectoryManager.Profiles;
            }
            else
            {
                if (!Directory.Exists(profileSavePath))
                {
                    Log.Error("The directory you inserted does not exists.");
                    goto profileSaveQuestion;
                }
                config.profileDirectory = profileSavePath;
            }

            // ask the path where the shop will be saved
            shopSaveQuestion:
            string shopSavePath = AnsiConsole.Ask<string>("Insert the [62]path[/] to use for save the [62]catalog[/] (type [62]d[/] for use the default one):");
            if (shopSavePath == "d")
            {
                config.shopDirectory = DirectoryManager.Profiles;
            }
            else
            {
                if (!Directory.Exists(shopSavePath))
                {
                    Log.Error("The directory you inserted not exists.");
                    goto shopSaveQuestion;
                }
                config.shopDirectory = shopSavePath;
            }
            // save settings
            Save();
        }
        else
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(DirectoryManager.Settings, "settings.json")))!;
        }
    }

    public static void Save()
    {
        File.WriteAllText(Path.Combine(DirectoryManager.Settings, "settings.json"), JsonConvert.SerializeObject(config, Formatting.Indented));
    }
}
