using System.Text;
using Newtonsoft.Json;

namespace Athena.Models;

public class AthenaVersion : IComparable<AthenaVersion>
{
    public readonly int Release;
    public readonly int Major;
    public readonly int Minor;
    public readonly int Patch;

    public AthenaVersion(int release = 0, int major = 0, int minor = 0, int patch = 0)
    {
        Release = release;
        Major = major;
        Minor = minor;
        Patch = patch;
    }

    public AthenaVersion(string versionString)
    {
        if (versionString[0] == 'v')
            versionString = versionString[1..];

        var split = versionString.Split('.');
        if (split.Length > 0) Release = int.Parse(split[0]);
        if (split.Length > 1) Major = int.Parse(split[1]);
        if (split.Length > 2) Minor = int.Parse(split[2]);
        if (split.Length > 3) Patch = int.Parse(split[3]);
    }

    public static bool operator >(AthenaVersion a, AthenaVersion b)
    {
        return a.CompareTo(b) > 0;
    }

    public static bool operator <(AthenaVersion a, AthenaVersion b)
    {
        return a.CompareTo(b) < 0;
    }

    public static bool operator ==(AthenaVersion a, AthenaVersion b)
    {
        return a.CompareTo(b) == 0;
    }

    public static bool operator !=(AthenaVersion a, AthenaVersion b)
    {
        return a.CompareTo(b) != 0;
    }

    public bool Equals(AthenaVersion other)
    {
        return Release == other.Release && Major == other.Major && Minor == other.Minor && Patch == other.Patch;
    }

    public int CompareTo(AthenaVersion? other)
    {
        if (other is null) return 1;
        if (ReferenceEquals(this, other)) return 0;

        var release = Release.CompareTo(other.Release);
        if (release != 0) return release;

        var major = Major.CompareTo(other.Major);
        if (major != 0) return major;

        var minor = Minor.CompareTo(other.Minor);
        if (minor != 0) return minor;

        var patch = Patch.CompareTo(other.Patch);
        if (patch != 0) return patch;

        return 0;
    }

    public string DisplayName => 'v' + ToString();

    public override string ToString()
    {
        var versionString = new StringBuilder();

        versionString.Append(Release);
        versionString.Append('.');
        versionString.Append(Major);
        versionString.Append('.');
        versionString.Append(Minor);
        versionString.Append('.');
        versionString.Append(Patch);
#if DEBUG
        versionString.Append(" (DEBUG)");
#endif
        return versionString.ToString();
    }
}

public class AthenaVersionConverter : JsonConverter<AthenaVersion>
{
    public override AthenaVersion? ReadJson(JsonReader reader, Type objectType, AthenaVersion? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var value = reader.Value?.ToString();
        if (string.IsNullOrWhiteSpace(value)) return null;

        return new AthenaVersion(value);
    }

    public override void WriteJson(JsonWriter writer, AthenaVersion? value, JsonSerializer serializer)
    {
        if (value is not AthenaVersion version) return;

        serializer.Serialize(writer, version.ToString());
    }
}