using Spectre.Console;
using Newtonsoft.Json;
using Athena.Managers;

namespace Athena.Models;

public class Config
{
    public static Config config;

    public string athenaProfileId { get; set; }
    public string profileDirectory { get; set; }
    public string shopDirectory { get; set; }

    public static void LoadSettings()
    {
        config = new Config();

        if (!File.Exists(Path.Combine(DirectoryManager.Settings, "settings.json")))
        {
            // ask the name for the profile athena (locker loadout name & other things)
            config.athenaProfileId = AnsiConsole.Ask<string>("Insert the [62]name[/] to use for the [62]Profile-Athena[/]:");

            // ask the path where the profile will be saved (default is profiles folder)
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
                    Log.Error("The directory you inserted not exists.");
                    goto profileSaveQuestion;
                }
                else
                {
                    config.profileDirectory = profileSavePath;
                }
            }

            // ask the path where the shop will be saved (default is profiles folder)
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
                else
                {
                    config.shopDirectory = shopSavePath;
                }
            }

            Save();
        }
        else
        {
            config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(Path.Combine(DirectoryManager.Settings, "settings.json")));
        }
    }

    public static void Save()
    {
        File.WriteAllText(Path.Combine(DirectoryManager.Settings, "settings.json"), JsonConvert.SerializeObject(config, Formatting.Indented));
    }
}
