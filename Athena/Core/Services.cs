using Athena.Services;

namespace Athena.Core;

public static class AthenaServices
{
    public static readonly AppService App = new();

    public static readonly APIService Api = new();
    public static readonly DirectoriesService Directories = new();

    public static readonly UpdaterService Updater = new();
    public static readonly SettingsService AppSettings = new();
    public static readonly DependencyService Dependencies = new();

    public static readonly AssetsService Assets = new();
    public static readonly DataminerService UEParser = new();

    public static readonly DiscordService Discord = new();
}