
// app start
await Athena.Managers.Athena.Initialize();

// The Application will remain open till the user presses the enter key
Log.Information("Press the enter key to close the application");

ConsoleKeyInfo keyInfo;
do { keyInfo = Console.ReadKey(true); } 
while (keyInfo.Key != ConsoleKey.Enter);

Log.Information(" --------------- Application Closed --------------- ");
