using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    /// <summary>
    /// used to refer to the player from other scripts without expensive lookup operations
    /// </summary>
    [HideInInspector]
    public static PlayerController instance;

    /// <summary>
    /// object used to control the player from Lua code or request information from it
    /// </summary>
    [HideInInspector]
    public static LuaPlayerStatus luaStatus;

    /// <summary>
    /// the RectTransform of the inner box of the battle arena - set using Unity Inspector
    /// </summary>
    public RectTransform arenaBounds;

    /// <summary>
    /// absolute position of the player on screen, used mainly by projectiles for collision detection
    /// </summary>
    [HideInInspector]
    public Rect playerAbs;

    /// <summary>
    /// take a wild guess
    /// </summary>
    internal int HP = PlayerCharacter.MaxHP;

    /// <summary>
    /// invulnerability timer, player blinks and is invulnerable for this many seconds when set
    /// </summary>
    internal float invulTimer = 0.0f;

    /// <summary>
    /// the player's RectTransform
    /// </summary>
    internal RectTransform self;

    /// <summary>
    /// how long does it take to do a full blink (appear+disappear), in seconds
    /// </summary>
    private float blinkCycleSeconds = 0.18f;

    /// <summary>
    /// pixels to inset the player's hitbox, as a temporary replacement for having actually good hitboxes
    /// </summary>
    private int hitboxInset = 4;

    /// <summary>
    /// the hurt sound component, attached to the player
    /// </summary>
    private AudioSource playerAudio;

    /// <summary>
    /// The player hurt sound.
    /// </summary>
    private AudioClip hurtSound;

    /// <summary>
    /// The player heal sound.
    /// </summary>
    private AudioClip healSound;

    /// <summary>
    /// intended direction for movement; -1 OR 1 for x, -1 OR 1 for y. Multiplied by speed in Move() function
    /// </summary>
    private Vector2 intendedShift;

    /// <summary>
    /// is the player moving or not? Set in the Move function, retrieved through isMoving()
    /// </summary>
    private bool moving = false;

    /// <summary>
    /// if true, ignores movement input. Done when the player should be controlled by something else, like the UI
    /// </summary>
    private bool overrideControl = false;

    /// <summary>
    /// the Image of the player
    /// </summary>
    private Image selfImg;

    /// <summary>
    /// contains a Soul type that affects what player movement does
    /// </summary>
    private AbstractSoul soul;

    /// <summary>
    /// Contains directions the player can go in. This is to make abstracting out controls and adding control schemes at a later point easier.
    /// </summary>
    private enum Directions
    {
        /// <summary>
        /// stylecop wants documentation here. take a wild guess what UP means
        /// </summary>
        UP,

        /// <summary>
        /// stylecop wants documentation here. take a wild guess what DOWN means
        /// </summary>
        DOWN,

        /// <summary>
        /// stylecop wants documentation here. we get it already stylecop
        /// </summary>
        LEFT,

        /// <summary>
        /// sigh
        /// </summary>
        RIGHT
    };

    /// <summary>
    /// Hurts the player and makes them invulnerable for invulnerabilitySeconds.
    /// </summary>
    /// <param name="damage">Damage to deal to the player</param>
    /// <param name="invulnerabilitySeconds">Optional invulnerability time for the player, in seconds.</param>
    /// <returns></returns>
    public virtual void Hurt(int damage = 3, float invulnerabilitySeconds = 1.7f)
    {
        // set timer and play the hurt sound if player was actually hurt
        // TODO: factor in stats and what the actual damage should be
        if (damage > 0 && invulTimer <= 0)
        {
            playerAudio.clip = hurtSound;
            playerAudio.Play();
            setHP(HP - damage);
        }
        else if (damage < 0)
        {
            playerAudio.clip = healSound;
            playerAudio.Play();
            setHP(HP - damage);
        }

        if (HP > 0 && invulTimer <= 0)
        {
            invulTimer = invulnerabilitySeconds;
        }
    }

    public void setHP(int newhp)
    {
        if (newhp <= 0)
        {
            HP = 0;
            invulTimer = 0;
            GameObject.DontDestroyOnLoad(this.gameObject);
            this.gameObject.transform.SetParent(null);
            this.setControlOverride(true);
            gameObject.AddComponent<GameOverBehavior>();
        }
        else if (newhp > PlayerCharacter.MaxHP)
        {
            HP = PlayerCharacter.MaxHP;
        }
        else
        {
            HP = newhp;
        }
        PlayerCharacter.HP = HP;
        UIStats.instance.setHP(HP);
    }

    public bool isHurting()
    {
        return invulTimer > 0;
    }

    // check if player is moving, used in orange/blue projectiles to see if they should hurt or not
    public bool isMoving()
    {
        return moving;
    }

    // modify absolute player position, accounting for walls
    public void ModifyPosition(float xMove, float yMove, bool ignoreBounds)
    {
        float xPos = self.anchoredPosition.x + xMove;
        float yPos = self.anchoredPosition.y + yMove;

        SetPosition(xPos, yPos, ignoreBounds);
    }

    // move within arena boundaries given 'directional' vector (non-unit: x is -1 OR 1 and y is -1 OR 1)
    public virtual void Move(Vector2 dir)
    {
        Vector2 soulDir = soul.GetMovement(dir.x, dir.y);
        soulDir *= Time.deltaTime;

        // reusing the direction Vector2 for position to save ourselves the creation of a new object
        float oldXPos = self.anchoredPosition.x;
        float oldYPos = self.anchoredPosition.y;
        ModifyPosition(soulDir.x, soulDir.y, false);
        MovementDelta(oldXPos, oldYPos);
    }

    // set to ignore regular battle arena controls and updates. Used to forfeit control to UI without disabling player controller.
    public void setControlOverride(bool overrideControls)
    {
        this.overrideControl = overrideControls;
        soul.setHalfSpeed(false);
    }

    public void SetPosition(float xPos, float yPos, bool ignoreBounds)
    {
        // check if new position would be out of arena bounds, and modify accordingly if it is
        if (!ignoreBounds)
        {
            if (xPos < arenaBounds.position.x - arenaBounds.sizeDelta.x / 2 + self.rect.size.x / 2)
            {
                xPos = arenaBounds.position.x - arenaBounds.sizeDelta.x / 2 + self.rect.size.x / 2;
            }
            else if (xPos > arenaBounds.position.x + arenaBounds.sizeDelta.x / 2 - self.rect.size.x / 2)
            {
                xPos = arenaBounds.position.x + arenaBounds.sizeDelta.x / 2 - self.rect.size.x / 2;
            }

            if (yPos < arenaBounds.position.y - arenaBounds.sizeDelta.y / 2 + self.rect.size.y / 2)
            {
                yPos = arenaBounds.position.y - arenaBounds.sizeDelta.y / 2 + self.rect.size.y / 2;
            }
            else if (yPos > arenaBounds.position.y + arenaBounds.sizeDelta.y / 2 - self.rect.size.y / 2)
            {
                yPos = arenaBounds.position.y + arenaBounds.sizeDelta.y / 2 - self.rect.size.y / 2;
            }
        }

        // set player position on screen
        self.anchoredPosition = new Vector2(xPos, yPos);
        // modify the player rectangle position so projectiles know where it is
        playerAbs.x = self.anchoredPosition.x - self.rect.size.x / 2 + hitboxInset;
        playerAbs.y = self.anchoredPosition.y - self.rect.size.y / 2 + hitboxInset;
    }

    public void SetSoul(AbstractSoul s)
    {
        selfImg.color = s.color;
        soul = s;
        // if still holding X keep the slow applied
        if (InputUtil.Held(GlobalControls.input.Cancel))
        {
            s.setHalfSpeed(true);
        }
    }

    /// <summary>
    /// Built-in Unity function for initialization.
    /// </summary>
    private void Awake()
    {
        self = GetComponent<RectTransform>();
        selfImg = GetComponent<Image>();
        playerAbs = new Rect(0, 0, selfImg.sprite.texture.width - hitboxInset * 2, selfImg.sprite.texture.height - hitboxInset * 2);
        instance = this;
        playerAudio = GetComponent<AudioSource>();
        hurtSound = AudioClipRegistry.GetSound("hurtsound");
        healSound = AudioClipRegistry.GetSound("healsound");
        SetSoul(new RedSoul(this));
        luaStatus = new LuaPlayerStatus(this);
    }

    /// <summary>
    /// Modifies the movement direction based on input. Broken up into single ifs so pressing opposing keys prevents you from moving.
    /// </summary>
    private void HandleInput()
    {
        if (InputUtil.Held(GlobalControls.input.Up))
        {
            ModifyMovementDirection(Directions.UP);
        }
        if (InputUtil.Held(GlobalControls.input.Down))
        {
            ModifyMovementDirection(Directions.DOWN);
        }

        if (InputUtil.Held(GlobalControls.input.Left))
        {
            ModifyMovementDirection(Directions.LEFT);
        }
        if (InputUtil.Held(GlobalControls.input.Right))
        {
            ModifyMovementDirection(Directions.RIGHT);
        }

        if (InputUtil.Pressed(GlobalControls.input.Cancel))
        {
            soul.setHalfSpeed(true);
        }
        else if (InputUtil.Released(GlobalControls.input.Cancel))
        {
            soul.setHalfSpeed(false);
        }
    }

    // given an input direction, let intendedShift carry 'directional' vector (non-unit: x is -1 OR 1 and y is -1 OR 1)
    // TODO: make the return value matter instead of relying on an in-class variable, it looks stupid
    private void ModifyMovementDirection(Directions d)
    {
        switch (d)
        {
            case Directions.UP:
                intendedShift += Vector2.up;
                break;

            case Directions.DOWN:
                intendedShift += Vector2.down;
                break;

            case Directions.LEFT:
                intendedShift += Vector2.left;
                break;

            case Directions.RIGHT:
                intendedShift += Vector2.right;
                break;

            default:
                intendedShift = Vector2.zero;
                break;
        }
    }

    private void MovementDelta(float oldX, float oldY)
    {
        float xDelta = self.anchoredPosition.x - oldX;
        float yDelta = self.anchoredPosition.y - oldY;

        // if the position is the same, the player hasnt moved - by doing it like this we account
        // for things like being moved by external factors like being shoved by boundaries
        // TODO: account for external factors like being moved by other scripts (enemies e.a.)
        if (xDelta == 0.0f && yDelta == 0.0f)
            moving = false;
        else
            moving = true;
        soul.PostMovement(xDelta, yDelta);
    }

    /// <summary>
    /// Built-in Unity function called once per frame.
    /// </summary>
    private void Update()
    {
        // DEBUG CONTROLS
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetSoul(new RedSoul(this));
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetSoul(new BlueSoul(this));
        }
        // END DEBUG CONTROLS

        // handle input and movement, unless control is overridden by the UI controller, for instance
        if (!overrideControl)
        {
            intendedShift = Vector2.zero; // reset direction we are going in
            HandleInput(); // get direction we want to go in
            Move(intendedShift); // move in direction we just got
        }

        // if the invulnerability timer has more than 0 seconds (usually when you get hurt), blink to reflect the hurt state
        if (invulTimer > 0.0f)
        {
            invulTimer -= Time.deltaTime;
            if (invulTimer % blinkCycleSeconds > blinkCycleSeconds / 2.0f)
            {
                selfImg.enabled = false;
            }
            else
            {
                selfImg.enabled = true;
            }

            if (invulTimer <= 0.0f)
                selfImg.enabled = true;
        }
    }
}