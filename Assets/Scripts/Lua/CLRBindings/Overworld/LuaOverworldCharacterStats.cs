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

    public LuaCharacterSpriteController sprite {
        get {
            return spr;
        }
    }

    public int moveState {
        get {
            return thisCharacter.state;
        }
        set {
            thisCharacter.state=value;
        }
    }

    public float moveSpeed {
        get {
            return thisCharacter.moveSpeed;
        }
        set {
            thisCharacter.moveSpeed=value;
        }
    }

    public void MoveToPoint(float x, float y) {
        thisCharacter.MoveToPoint(new Vector2(x,y));
    }
}
