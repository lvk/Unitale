using UnityEngine;

public abstract class AbstractSoul
{
    protected float slowSpeed = 60.0f; // move this many pixels in 1 second of game time when slow moving (holding X in vanilla UT)
    protected float normalSpeed = 120.0f; // move this many pixels in 1 second of game time when normally moving
    protected float speed; // actual player speed used in game update cycles

    private PlayerController player;

    public AbstractSoul(PlayerController player)
    {
        this.player = player;
        this.speed = normalSpeed;
    }

    public abstract Color color { get; }

    /// <summary>
    /// called when pressing and when releasing X
    /// </summary>
    /// <param name="isHalfSpeed">true if speed should be halved, false otherwise</param>
    public void setHalfSpeed(bool isHalfSpeed)
    {
        if (isHalfSpeed)
            speed = slowSpeed;
        else
            speed = normalSpeed;
    }

    public abstract Vector2 GetMovement(float xDir, float yDir);

    public virtual void PostMovement(float xDelta, float yDelta)
    {
    }
}