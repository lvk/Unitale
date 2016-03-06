using UnityEngine;
using UnityEngine.UI;

// The base projectile class. All projectiles, including new/combined types, should inherit from this.
public abstract class Projectile : MonoBehaviour
{
    /*
     * Commented out because Z indices don't really work yet becanse the Unity 5 UI likes to work differently, despite operating in world space.
     * 
    public const float Z_INDEX_INITIAL = 30.0f; //Z index the projectiles start spawning at, reset after every wave
    public static float Z_INDEX_NEXT //Used to set the initial Z position for projectiles when they're created.
    {
        get
        {
            zIndexCurrent -= 0.001f;
            return zIndexCurrent;
        }
        set
        {
            zIndexCurrent = value;
        }
    }
    private static float zIndexCurrent = Z_INDEX_INITIAL;
     */

    protected internal RectTransform self; // RectTransform of this projectile
    protected internal ProjectileController ctrl; // come to think of it protected internal is pretty useless atm
    protected Rect selfAbs; // Rectangle containing position and size of this projectile

    private bool currentlyVisible = true; // Used to keep track of whether this object is currently visible to potentially save time in SetRenderingActive().

    /// <summary>
    /// Built-in Unity function run for initialization
    /// </summary>
    private void Awake()
    {
        self = GetComponent<RectTransform>();
        ctrl = new ProjectileController(this);
    }

    /// <summary>
    /// Built-in Unity function run on enabling this object
    /// </summary>
    private void OnEnable()
    {
        Image img = GetComponent<Image>();
        img.color = Color.white;
        img.rectTransform.eulerAngles = Vector3.zero;
        Vector2 half = new Vector2(0.5f, 0.5f);
        img.rectTransform.anchorMax = half;
        img.rectTransform.anchorMin = half;
        img.rectTransform.pivot = half;
        self.sizeDelta = img.sprite.rect.size;
        selfAbs = new Rect(self.anchoredPosition.x - self.rect.width / 2, self.anchoredPosition.y - self.rect.height / 2, self.sizeDelta.x, self.sizeDelta.y);
        OnStart();
    }

    /// <summary>
    /// Renew the attached Projectile Controller. This is done whenever this projectile is dequeued from the bullet pool.
    /// </summary>
    public void renewController()
    {
        ctrl = new ProjectileController(this);
    }

    /// <summary>
    /// Built-in Unity function run on every frame
    /// </summary>
    private void Update()
    {
        ctrl.UpdatePosition();
        OnUpdate();
        if (HitTest())
        {
            OnProjectileHit();
        }
    }

    /// <summary>
    /// Overrideable start function to set projectile-specific settings.
    /// </summary>
    public virtual void OnStart() { }

    /// <summary>
    /// Overrideable update function to execute on every frame.
    /// </summary>
    public virtual void OnUpdate() { }

    /// <summary>
    /// Overrideable method that executes when player is hit. Usually, this calls Hurt() on the player in some way.
    /// </summary>
    public virtual void OnProjectileHit()
    {
        PlayerController.instance.Hurt();
    }

    /// <summary>
    /// Updates the projectile's hitbox.
    /// </summary>
    public virtual void UpdateHitRect()
    {
        selfAbs.x = self.position.x - self.rect.width / 2;
        selfAbs.y = self.position.y - self.rect.height / 2;
    }

    /// <summary>
    /// Return the rectangle surrounding this projectile.
    /// </summary>
    /// <returns>The rectangle surrounding this projectile.</returns>
    public Rect getRect()
    {
        return selfAbs;
    }

    /// <summary>
    /// Enable or disable rendering of this projectile and its children.
    /// </summary>
    /// <param name="active">true to enable rendering, false to disable</param>
    protected void SetRenderingActive(bool active)
    {
        // dont cycle through children if they arent changing state anyway
        if (currentlyVisible == active)
            return;
        Image selfImg = GetComponent<Image>();
        if (selfImg != null)
            selfImg.enabled = active;
        Image[] images = (Image[])GetComponentsInChildren<Image>();
        foreach (Image image in images)
        {
            image.enabled = active;
        }

        currentlyVisible = active;
    }

    /// <summary>
    /// Overrideable method run on every frame that should update the hitbox and return true if this projectile is hitting the player.
    /// </summary>
    /// <returns>true if there's a collision, otherwise false</returns>
    public virtual bool HitTest()
    {
        UpdateHitRect();
        if (selfAbs.Overlaps(PlayerController.instance.playerAbs))
            return true;
        return false;
    }
}