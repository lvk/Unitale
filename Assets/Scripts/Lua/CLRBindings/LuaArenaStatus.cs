/// <summary>
/// Lua binding to set and retrieve information for the game's arena.
/// </summary>
public class LuaArenaStatus
{
    /// <summary>
    /// Get the arena's width, after resizing.
    /// </summary>
    public float width
    {
        get
        {
            return ArenaSizer.instance.newX;
        }
    }

    /// <summary>
    /// Get the arena's height, after resizing.
    /// </summary>
    public float height
    {
        get
        {
            return ArenaSizer.instance.newY;
        }
    }

    /// <summary>
    /// Get the arena's current width, even during resizing.
    /// </summary>
    public float currentwidth
    {
        get
        {
            return ArenaSizer.arenaAbs.width;
        }
    }

    /// <summary>
    /// Get the arena's current height, even during resizing.
    /// </summary>
    public float currentheight
    {
        get
        {
            return ArenaSizer.arenaAbs.height;
        }
    }

    /// <summary>
    /// Resize the arena to the new width/height. Throws a hilarious (read: not hilarious) error message if user was sneaky, bound it globally and tried using it outside of a wave script.
    /// </summary>
    /// <param name="w">New width for arena.</param>
    /// <param name="h">New height for arena.</param>
    public void Resize(int w, int h)
    {
        if (UIController.instance.getState() == UIController.UIState.DEFENDING)
        {
            ArenaSizer.instance.Resize(w, h);
        }
        else
        {
            UnitaleUtil.displayLuaError("NOT THE WAVE SCRIPT", "sorry but pls don't");
        }
    }

    public void ResizeImmediate(int w, int h)
    {
        if (UIController.instance.getState() == UIController.UIState.DEFENDING)
        {
            ArenaSizer.instance.ResizeImmediate(w, h);
        }
        else
        {
            UnitaleUtil.displayLuaError("NOT THE WAVE SCRIPT", "sorry but pls don't");
        }
    }
}