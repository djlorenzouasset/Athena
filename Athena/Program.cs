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

// The Application will remain open till the user presses the enter key
Log.Information("Press the enter key to close the application");

ConsoleKeyInfo keyInfo;
do { keyInfo = Console.ReadKey(true); } 
while (keyInfo.Key != ConsoleKey.Enter);

Log.Information(" --------------- Application Closed --------------- ");
