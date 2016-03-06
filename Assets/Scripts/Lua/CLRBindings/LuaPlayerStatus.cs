using UnityEngine;

/// <summary>
/// Lua binding to set and retrieve information for the on-screen player.
/// </summary>
public class LuaPlayerStatus
{
    /// <summary>
    /// This Lua controller's attached PlayerController.
    /// </summary>
    protected PlayerController player;

    /// <summary>
    /// The sprite controller for the player.
    /// </summary>
    private LuaSpriteController spr;

    /// <summary>
    /// Create a new Lua controller intended for this player.
    /// </summary>
    /// <param name="p">PlayerController this controller is intended for</param>
    public LuaPlayerStatus(PlayerController p)
    {
        this.player = p;
        spr = new LuaSpriteController(p.GetComponent<UnityEngine.UI.Image>());
    }

    /// <summary>
    /// Get player's X position relative to the arena's center.
    /// </summary>
    public float x
    {
        get
        {
            return player.self.anchoredPosition.x - ArenaSizer.arenaCenter.x;
        }
    }

    /// <summary>
    /// Get player's Y position relative to the arena's center.
    /// </summary>
    public float y
    {
        get
        {
            return player.self.anchoredPosition.y - ArenaSizer.arenaCenter.y;
        }
    }

    /// <summary>
    /// Get player's X position relative to the bottom left of the screen.
    /// </summary>
    public float absx
    {
        get
        {
            return player.self.anchoredPosition.x;
        }
    }

    /// <summary>
    /// Get player's Y position relative to the bottom left of the screen.
    /// </summary>
    public float absy
    {
        get
        {
            return player.self.anchoredPosition.y;
        }
    }

    /// <summary>
    /// Sprite controller for the player soul.
    /// </summary>
    public LuaSpriteController sprite
    {
        get
        {
            return spr;
        }
    }

    /// <summary>
    /// Get player's current HP.
    /// </summary>
    public int hp
    {
        get
        {
            return player.HP;
        }
        set
        {
            player.setHP(value);
        }
    }

    /// <summary>
    /// Player character's name.
    /// </summary>
    public string name
    {
        get
        {
            return PlayerCharacter.Name;
        }
        set
        {
            string shortName = value;
            if (shortName.Length > 6)
            {
                shortName = value.Substring(0, 6);
            }
            PlayerCharacter.Name = shortName;
            UIStats.instance.setPlayerInfo(shortName, PlayerCharacter.LV);
        }
    }

    /// <summary>
    /// Player character's level. Adjusts stats when set.
    /// </summary>
    public int lv
    {
        get
        {
            return PlayerCharacter.LV;
        }
        set
        {
            if (PlayerCharacter.LV != value)
            {
                PlayerCharacter.SetLevel(value);
                if (PlayerCharacter.HP > PlayerCharacter.MaxHP)
                {
                    player.setHP(PlayerCharacter.MaxHP);
                }
                UIStats.instance.setPlayerInfo(PlayerCharacter.Name, PlayerCharacter.LV);
                UIStats.instance.setMaxHP();
            }
        }
    }

    /// <summary>
    /// True if player is currently blinking and invincible, false otherwise.
    /// </summary>
    public bool isHurting
    {
        get
        {
            return player.isHurting();
        }
    }

    /// <summary>
    /// True if player is currently moving, false otherwise. Being pushed by the edges of the arena counts as moving.
    /// </summary>
    public bool isMoving
    {
        get
        {
            return player.isMoving();
        }
    }

    /// <summary>
    /// Hurt the player for the given damage and the default invulnerability time. If this gets the player to 0 (or less) HP, you get the game over screen.
    /// </summary>
    /// <param name="damage">Damage to deal to the player</param>
    public void Hurt(int damage)
    {
        player.Hurt(damage);
    }

    /// <summary>
    /// Hurts the player with the given damage and invulnerabilty time. If this gets the player to 0 (or less) HP, you get the game over screen.
    /// </summary>
    /// <param name="damage">Damage to deal to the player</param>
    /// <param name="invulTime">Invulnerability time in seconds</param>
    public void Hurt(int damage, float invulTime)
    {
        player.Hurt(damage, invulTime);
    }

    /// <summary>
    /// Heals the player. Convenience method which is the same as hurting the player for -damage and no invulnerability time.
    /// </summary>
    /// <param name="heal">Value to heal the player for</param>
    public void Heal(int heal)
    {
        player.Hurt(-heal, 0.0f);
    }

    /// <summary>
    /// Override player control. Note: this will disable all movement checking on the player, making it ignore the arena walls.
    /// </summary>
    /// <param name="overrideControl"></param>
    public void SetControlOverride(bool overrideControl)
    {
        if(UIController.instance.getState() == UIController.UIState.DEFENDING){
            player.setControlOverride(overrideControl);
        }
    }

    /// <summary>
    /// Move player relative to arena center.
    /// </summary>
    /// <param name="x">X position of player relative to arena center.</param>
    /// <param name="y">Y position of player relative to arena center.</param>
    /// <param name="ignoreWalls">If false, it will place you at the edge of the arena instead of over it.</param>
    public void MoveTo(float x, float y, bool ignoreWalls)
    {
        this.MoveToAbs(ArenaSizer.arenaCenter.x + x, ArenaSizer.arenaCenter.y + y, ignoreWalls);
    }

    /// <summary>
    /// Move player relative to the lower left of the screen.
    /// </summary>
    /// <param name="x">X position of player relative to the lower left of the screen.</param>
    /// <param name="y">Y position of player relative to the lower left of the screen.</param>
    /// <param name="ignoreWalls">If false, it will place you at the edge of the arena instead of over it.</param>
    public void MoveToAbs(float x, float y, bool ignoreWalls)
    {
        player.SetPosition(x, y, ignoreWalls);
    }
}