using UnityEngine;
using System.Collections;

public class LuaInputBinding {
    private UndertaleInput input;
    public LuaInputBinding(UndertaleInput baseInput)
    {
        this.input = baseInput;
    }

    public int Confirm { get { return (int)this.input.Confirm; } }
    public int Cancel { get { return (int)this.input.Cancel; } }
    public int Menu { get { return (int)this.input.Menu; } }
    public int Up { get { return (int)this.input.Up; } }
    public int Down { get { return (int)this.input.Down; } }
    public int Left { get { return (int)this.input.Left; } }
    public int Right { get { return (int)this.input.Right; } }
}
