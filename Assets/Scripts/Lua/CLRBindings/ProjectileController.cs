using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

/// <summary>
/// Lua binding to set and retrieve information for bullets in the game.
/// </summary>
public class ProjectileController
{
    bool active = true;
    private Projectile p;
    private LuaSpriteController spr;
    private Dictionary<string, DynValue> vars = new Dictionary<string, DynValue>();

    public ProjectileController(Projectile p)
    {
        this.p = p;
        this.spr = new LuaSpriteController(p.GetComponent<UnityEngine.UI.Image>());
    }

    public float x
    {
        get;
        internal set;
    }

    public float y
    {
        get;
        internal set;
    }

    /*
     * not quite working due to unity's UI layering system
     * public float z
    {
        get
        {
            return p.self.position.z;
        }
        internal set
        {
            p.self.position = new Vector3(p.self.position.x, p.self.position.y, value);
        }
    }*/

    public float absx
    {
        get;
        internal set;
    }

    public float absy
    {
        get;
        internal set;
    }

    public bool isactive
    {
        get
        {
            return active;
        }
    }

    public LuaSpriteController sprite
    {
        get
        {
            return spr;
        }
    }

    public void UpdatePosition()
    {
        this.x = p.self.anchoredPosition.x - ArenaSizer.arenaCenter.x;
        this.y = p.self.anchoredPosition.y - ArenaSizer.arenaCenter.y;
        this.absx = p.self.anchoredPosition.x;
        this.absy = p.self.anchoredPosition.y;
    }

    public void Remove()
    {
        if (active)
        {
            BulletPool.instance.Requeue(p);
            this.p = null;
            active = false;
        }
    }

    public void Move(float x, float y)
    {
        MoveToAbs(p.self.anchoredPosition.x + x, p.self.anchoredPosition.y + y);
    }

    public void MoveTo(float x, float y)
    {
        MoveToAbs(ArenaSizer.arenaCenter.x + x, ArenaSizer.arenaCenter.y + y);
    }

    public void MoveToAbs(float x, float y)
    {
        if (p == null)
        {
            throw new MoonSharp.Interpreter.ScriptRuntimeException("Attempted to move a removed bullet. You can use a bullet's isactive property to check if it has been removed.");
        }
        p.self.anchoredPosition = new Vector2(x, y);
    }

    public void SendToTop()
    {
        p.self.SetAsLastSibling(); // in unity, the lowest UI component in the hierarchy renders last
    }

    public void SendToBottom()
    {
        p.self.SetAsFirstSibling();
    }

    public void SetVar(string name, DynValue value)
    {
        vars[name] = value;
    }

    public DynValue GetVar(string name)
    {
        DynValue retval;
        if (vars.TryGetValue(name, out retval))
        {
            return retval;
        }
        else
        {
            return null;
        }
    }
}