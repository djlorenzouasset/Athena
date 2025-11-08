using System.ComponentModel;

namespace Athena.Extensions;

public static class EnumExtensions
{
    public static string DisplayName(this Enum value)
    {
        return value.GetType().GetField(value.ToString())?
            .GetCustomAttributes(typeof(DescriptionAttribute), false)
            .SingleOrDefault() is not DescriptionAttribute attribute
            ? value.ToString() : attribute.Description;
    }
}