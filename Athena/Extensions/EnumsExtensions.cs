using System.ComponentModel;
using Athena.Models;

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

    public static string ItemTypeName(this Enum value)
    {
        return value.GetType().GetField(value.ToString())?
            .GetCustomAttributes(typeof(ItemType), false)
            .SingleOrDefault() is not ItemType attribute
            ? value.ToString() : attribute.ItemsType;
    }

    public static bool DisabledFor(this Enum value, EModelType model)
    {
        var attr = value.GetType().GetField(value.ToString())?
            .GetCustomAttributes(typeof(DisabledFor), false)
            .SingleOrDefault() as DisabledFor;

        return attr?.DisabledModels.Contains(model) ?? false;
    }
}