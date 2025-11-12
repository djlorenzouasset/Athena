namespace Athena.Models;

public class ItemType(string itemsType) : Attribute
{
    public string ItemsType = itemsType;
}