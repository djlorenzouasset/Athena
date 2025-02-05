namespace Athena.Utils;

public static class FUtils
{
    public static void ExitThread(int exitCode = 0)
    {
        Log.Information("Press the enter key to close the application");

        ConsoleKeyInfo keyInfo;
        do { keyInfo = Console.ReadKey(true); }
        while (keyInfo.Key != ConsoleKey.Enter);

        Log.ForContext("NoConsole", true).Information(" --------------- Application Closed --------------- ");
        Log.CloseAndFlush();
        Environment.Exit(exitCode);
    }

    public static void ExitThreadAfterUpdate(string version)
    {
        Log.Information("Starting Athena updater.");

        // @TODO: add updater logic
        ExitThread();
    }
}