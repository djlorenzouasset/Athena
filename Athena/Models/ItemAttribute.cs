namespace Athena.Models;

[AttributeUsage(AttributeTargets.Enum)]
public class ItemType(string itemsType) : Attribute
{
    public string ItemsType = itemsType;
}