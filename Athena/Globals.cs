using CUE4Parse.UE4.Objects.Core.Misc;
using Athena.Models;

namespace Athena;

public static class Globals
{
    public static readonly AthenaVersion Version = new(2, 0, 0, 0);

    public const string GITHUB_URL = "https://github.com/djlorenzouasset/Athena/releases/latest";
    public const string DISCORD_URL = "https://discord.gg/nJBj9NjUS4";
    public const string DONATIONS_URL = "https://ko-fi.com/djlorenzouasset";

    public static readonly FGuid ZERO_GUID = new();
}
