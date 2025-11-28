namespace Athena.Models;

[AttributeUsage(AttributeTargets.Field)]
public class ItemType(string itemsType) : Attribute
{
    public string ItemsType = itemsType;
}