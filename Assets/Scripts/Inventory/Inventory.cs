using System.Collections.Generic;

/// <summary>
/// Static placeholder inventory class for the player. Will probably get moved to something else that makes sense, like the player.
/// </summary>
public static class Inventory
{
    public static List<UnderItem> container = new List<UnderItem>(
        new UnderItem[]{
            new UnderItem(),
            new UnderItem(),
            new UnderItem(),
            new UnderItem(),
            new UnderItem(),
            new UnderItem(),
            new UnderItem()
        }
    );
}