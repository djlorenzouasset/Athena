using Newtonsoft.Json;
using Spectre.Console;
using Athena.Models;

// ask the name for the profile
if (!File.Exists("config.json"))
{
    Config.config = new Config();
    Config.config.athenaProfileId = AnsiConsole.Ask<string>("Insert the [62]name[/] to use for the [62]Profile-Athena[/]:");
    File.WriteAllText("config.json", JsonConvert.SerializeObject(Config.config, Formatting.Indented));
}
else
{
    Config.config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("config.json"));
}

Console.Clear(); // clear the console after input

// app start
await Athena.Managers.Athena.Initialize();
Console.ReadKey(); // the program have to remain opened until the user decide to close it
