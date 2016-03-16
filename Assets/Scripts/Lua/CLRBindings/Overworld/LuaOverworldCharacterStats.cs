using UnityEngine;
using System.Collections;

/// <summary>
/// Lua binding for character stats.
/// </summary>
public class LuaOverworldCharacterStats {
    /// <summary>
    /// This Lua controller's overworld character.
    /// </summary>
    protected LuaOverworldCharacter thisCharacter;

    /// <summary>
    /// The sprite controller for the character.
    /// </summary>
    private LuaCharacterSpriteController spr;

    public LuaOverworldCharacterStats(LuaOverworldCharacter p) {
        this.thisCharacter=p;
        this.spr=new LuaCharacterSpriteController(p.GetComponentInChildren<CharacterSpriteController>());
    }

    public float x {
        get {
            return thisCharacter.transform.position.x;
        }
        set {
            thisCharacter.transform.position=new Vector3(value,y);
        }
    }

    public float y {
        get {
            return thisCharacter.transform.position.y;
        }
        set {
            thisCharacter.transform.position=new Vector3(x, value);
        }
    }

    public float rotation {
        get {
            return thisCharacter.transform.eulerAngles.z;
        }
        set {
            thisCharacter.transform.rotation=Quaternion.Euler(0,0,value);
        }
    }

    public LuaCharacterSpriteController sprite {
        get {
            return spr;
        }
    }
}
