using System.Diagnostics;
using Spectre.Console;
using System.Runtime.InteropServices;
using Serilog.Sinks.SystemConsole.Themes;
using Athena.Utils;
using Athena.Models.API.Responses;

namespace Athena.Services;

public class AppService
{
    public AthenaRelease? ReleaseInfo;

    public async Task InitializeVersionInfo()
    {
        ReleaseInfo = await Api.Athena.GetReleaseInfoAsync();
        if (ReleaseInfo is null)
        {
            Log.Warning("Failed to fetch release information.");
        }
    }

    public void CreateLogger()
    {
        Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Debug()
#else
            .MinimumLevel.Information()
#endif
            .WriteTo.Logger(logger => logger.Filter
                .ByExcluding(evnt => evnt.Properties.ContainsKey("NoConsole"))
                .WriteTo.Console(
                    outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: AnsiConsoleTheme.Literate))
            .MinimumLevel.Debug() // use debug for file because we need all infos for support purposes
            .WriteTo.File(Path.Combine(Directories.Logs, $"Athena-Log-{DateTime.Now:dd-MM-yyyy}.log"))
            .CreateLogger();
    }

    public void LogDebugInformations()
    {
        Log.ForContext("NoConsole", true).Information("      --------------------------------------");
        Log.ForContext("NoConsole", true).Information("Log file opened: {date}", DateTime.Now);
        Log.ForContext("NoConsole", true).Information("Athena version: {version}", Globals.Version);
        Log.ForContext("NoConsole", true).Information("Current folder: {path}", Directories.Current);
        Log.ForContext("NoConsole", true).Information("NET version: {netVersion}", RuntimeInformation.FrameworkDescription);
#if DEBUG
        Log.ForContext("NoConsole", true).Information("bIsDebug: true"); // hahahah
#endif
    }

    public void ExitThread(int exitCode = 0)
    {
        Log.Information("Press the enter key to close the application");

        ConsoleKeyInfo keyInfo;
        do { keyInfo = Console.ReadKey(true); }
        while (keyInfo.Key != ConsoleKey.Enter);

        AppSettings.SaveSettings();
        Log.ForContext("NoConsole", true).Information("Log file closed: {date}", DateTime.UtcNow);
        Log.CloseAndFlush();
        Environment.Exit(exitCode);
    }

    // this function is used because AnsiConsole.Ask<T>() doesn't handle
    // text changes (like moving the arrows keys backwards/forwards
    public string Ask(string prompt, int linesToClear = 1)
    {
        invalid:
        {
            AnsiConsole.Markup(prompt + " ");
            var textRead = Console.ReadLine();
            if (string.IsNullOrEmpty(textRead) || string.IsNullOrWhiteSpace(textRead))
            {
                AthenaUtils.ClearConsoleLines(1);
                goto invalid;
            }

            AthenaUtils.ClearConsoleLines(linesToClear);
            return textRead;
        }
    }

    public T SelectionPrompt<T>(string title, List<T> choices, Func<T, string>? converter = null) where T : notnull
    {
        var prompt = new SelectionPrompt<T>()
            .Title(title)
            .AddChoices(choices);

        if (converter is not null)
        {
            prompt.UseConverter(converter);
        }
        return AnsiConsole.Prompt(prompt);
    }

    public List<T> MultiSelectionPrompt<T>(string title, List<T> choices, Func<T, string>? converter = null) where T : notnull
    {
        var prompt = new MultiSelectionPrompt<T>()
            .Title(title)
            .Required()
            .AddChoices(choices);

        if (converter is not null)
        {
            prompt.UseConverter(converter);
        }
        return AnsiConsole.Prompt(prompt);
    }

    public void Launch(string location, bool shellExecute = true)
    {
        Launch(new ProcessStartInfo
        {
            FileName = location,
            UseShellExecute = shellExecute
        });
    }

    public void Launch(ProcessStartInfo proc)
    {
        Process.Start(proc);
    }
}