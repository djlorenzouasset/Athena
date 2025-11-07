using Spectre.Console;

namespace Athena.Utils;

public static class AthenaUtils
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

    public static void ClearConsoleLines(int numberOfLines)
    {
        int startLine = Console.CursorTop - numberOfLines;
        if (startLine < 0)
        {
            startLine = 0;
        }

        for (int i = 0; i < numberOfLines; i++)
        {
            Console.SetCursorPosition(0, startLine + i);
            Console.Write(new string(' ', Console.WindowWidth));
        }
        Console.SetCursorPosition(0, startLine);
    }

    // this function is used because AnsiConsole.Ask<T>() doesn't handle
    // text changes (like moving the arrows keys backwards/forwards
    public static string Ask(string prompt, int clearLines = 1)
    {
        invalid:
        AnsiConsole.Markup(prompt + " ");
        var textRead = Console.ReadLine();
        if (string.IsNullOrEmpty(textRead))
        {
            ClearConsoleLines(clearLines);
            goto invalid;
        }

        ClearConsoleLines(clearLines);
        return textRead;
    }
}