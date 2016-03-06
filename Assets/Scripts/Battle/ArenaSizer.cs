using System;
using UnityEngine;

/// <summary>
/// Behaviour attached to the arena used to resize it.
/// </summary>
public class ArenaSizer : MonoBehaviour
{
    public const int UIWidth = 565; // width of the inner Undertale UI box
    public const int UIHeight = 130; // height of the inner Undertale UI box
    public static ArenaSizer instance; // Static instance of this class for referencing purposes.
    [HideInInspector]
    public static Rect arenaAbs; // arena hitbox
    [HideInInspector]
    public static Vector2 arenaCenter; // arena center, updated here to save computation time on doing it per frame
    [HideInInspector]
    public static LuaArenaStatus luaStatus { get; private set; } // The Lua Arena object on the C# side

    private RectTransform outer; // RectTransform of the slightly larger white box under the arena (it's the border).
    private RectTransform inner; // RectTransform of the inner part of the arena.
    private int pxPerSecond = 100 * 10; // How many pixels per second the arena should resize

    private float currentX; // Current width of the arena as it is resizing
    private float currentY; // Current height of the arena as it is resizing
    internal float newX; // Desired width of the arena; internal so the Lua Arena object may refer to it (lazy)
    internal float newY; // Desired height of the arena; internal so the Lua Arena object may refer to it (lazy)

    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake()
    {
        // unlike the player we really dont want this on two components at the same time
        if (instance != null)
            throw new InvalidOperationException("Currently, the ArenaSizer may only be attached to one object.");

        outer = GameObject.Find("arena_border_outer").GetComponent<RectTransform>();
        inner = GameObject.Find("arena").GetComponent<RectTransform>();
        newX = currentX;
        newY = currentY;
        instance = this;
        luaStatus = new LuaArenaStatus();
    }

    private void Start()
    {
        LateUpdater.lateActions.Add(LateStart);
    }

    private void LateStart()
    {
        arenaAbs = new Rect(inner.position.x - inner.rect.width / 2, inner.position.y - inner.rect.height / 2, inner.rect.width, inner.rect.height);
        arenaCenter = RTUtil.AbsCenterOf(inner);
        currentX = inner.rect.width;
        currentY = inner.rect.height;
    }

    /// <summary>
    /// Set the desired size of this arena, after which it will keep resizing until it reaches the desired size.
    /// </summary>
    /// <param name="newx">Desired width of the arena</param>
    /// <param name="newy">Desired height of the arena</param>
    public void Resize(int newx, int newy)
    {
        this.newX = newx; 
        this.newY = newy;
    }

    /// <summary>
    /// Set the desired size of this arena immediately, without the animation.
    /// </summary>
    /// <param name="newx">Desired width of the arena</param>
    /// <param name="newy">Desired height of the arena</param>
    public void ResizeImmediate(int newx, int newy)
    {
        Resize(newx, newy);
        currentX = newX;
        currentY = newY;
        applyResize(newx, newy);
    }

    /// <summary>
    /// Gets how far along this arena is with resizing. Does this by returning the lowest of ratios of desired width/height to intended width/height.
    /// </summary>
    /// <returns>0.0 if the resizing has just started, 1.0 if it has finished.</returns>
    public float getProgress()
    {
        // depending on whether arena gets larger or smaller, adjust division order
        float xFrac = newX > currentX ? currentX / newX : newX / currentX;
        float yFrac = newY > currentY ? currentY / newY : newY / currentY;
        return Mathf.Min(xFrac, yFrac);
    }

    /// <summary>
    /// Used to check if the arena is currently in the process of resizing.
    /// </summary>
    /// <returns>true if it hasn't reached the intended size yet, false otherwise</returns>
    public bool isResizeInProgress()
    {
        return currentX != newX || currentY != newY;
    }

    /// <summary>
    /// Resizes the arena if the desired size is different from the current size.
    /// </summary>
    private void Update()
    {
        if (currentX == newX && currentY == newY)
            return;

        if (currentX < newX)
        {
            currentX += pxPerSecond * Time.deltaTime;
            if (currentX >= newX)
                currentX = newX;
        }
        else if (currentX > newX)
        {
            currentX -= pxPerSecond * Time.deltaTime;
            if (currentX <= newX)
                currentX = newX;
        }

        if (currentY < newY)
        {
            currentY += pxPerSecond * Time.deltaTime;
            if (currentY >= newY)
                currentY = newY;
        }
        else if (currentY > newY)
        {
            currentY -= pxPerSecond * Time.deltaTime;
            if (currentY <= newY)
                currentY = newY;
        }

        applyResize(currentX, currentY);
    }

    /// <summary>
    /// Takes care of actually applying the resize and updating the arena's rectangle.
    /// </summary>
    /// <param name="arenaX">New width</param>
    /// <param name="arenaY">New height</param>
    private void applyResize(float arenaX, float arenaY)
    {
        inner.sizeDelta = new Vector2(arenaX, arenaY);
        outer.sizeDelta = new Vector2(arenaX + 10, arenaY + 10);
        arenaAbs.x = inner.position.x - inner.rect.width / 2;
        arenaAbs.y = inner.position.y - inner.rect.height / 2;
        arenaAbs.width = inner.rect.width;
        arenaAbs.height = inner.rect.height;
        arenaCenter = new Vector2(inner.transform.position.x, inner.transform.position.y);
    }
}