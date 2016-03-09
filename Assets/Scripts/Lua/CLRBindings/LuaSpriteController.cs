using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LuaSpriteController {
    private Image _img;
    internal Image img { 
        get {
            if (_img == null)
            {
                throw new MoonSharp.Interpreter.ScriptRuntimeException("Attempted to perform action on removed sprite.");
            }
            else
            {
                return _img;
            }
        }
        set { _img = value; }
    }
    private Vector2 nativeSizeDelta;
    private Vector3 internalRotation = Vector3.zero;
    private float xScale = 1.0f;
    private float yScale = 1.0f;
    private Sprite originalSprite;
    private KeyframeCollection keyframes;

    public float x
    {
        get { return img.rectTransform.anchoredPosition.x; }
        set { img.rectTransform.anchoredPosition = new Vector2(value, img.rectTransform.anchoredPosition.y); }
    }

    public float y
    {
        get { return img.rectTransform.anchoredPosition.y; }
        set { img.rectTransform.anchoredPosition = new Vector2(img.rectTransform.anchoredPosition.x, value); }
    }

    public float xscale
    {
        get { return xScale; }
        set { 
            xScale = value; 
            Scale(xScale, yScale); 
        }
    }

    public float yscale
    {
        get { return yScale; }
        set { 
            yScale = value; 
            Scale(xScale, yScale); 
        }
    }

    public bool isactive
    { 
        get { return _img == null; } 
    }

    public float width
    {
        get { return img.mainTexture.width; }
    }

    public float height
    {
        get { return img.mainTexture.height; }
    }

    internal bool animcomplete
    {
        get
        {
            if (keyframes != null)
            {
                return keyframes.enabled && keyframes.animationComplete();
            }
            return false;
        }
    }

    internal KeyframeCollection.LoopMode loop
    {
        get
        {
            return keyframes.loop;
        }
        set
        {
            keyframes.loop = value;
        }
    }

    public float[] color {
        get { return new float[] { img.color.r, img.color.g, img.color.b }; }
        set {
            if (value.Length != 3)
            {
                throw new MoonSharp.Interpreter.ScriptRuntimeException("You need 3 numeric values when setting a sprite's color.");
            }
            else
            {
                img.color = new Color(value[0], value[1], value[2]);
            }
        }
    }

    public float alpha
    {
        get { return img.color.a; }
        set
        {
            float valClamped = Mathf.Clamp01(value);
            img.color = new Color(img.color.r, img.color.g, img.color.b, valClamped);
        }
    }

    public float rotation
    {
        get { return internalRotation.z; }
        set {
            internalRotation.z = Math.mod(value, 360);
            img.rectTransform.eulerAngles = internalRotation;
        }
    }

    /*
    public bool filter
    {
        get { return img.sprite.texture.filterMode != FilterMode.Point; }
        set
        {
            if (value)
            {
                img.sprite.texture.filterMode = FilterMode.Trilinear;
            }
            else
            {
                img.sprite.texture.filterMode = FilterMode.Point;
            }
        }
    }
    */

    public LuaSpriteController(Image i)
    {
        this.img = i;
        originalSprite = img.sprite;
        nativeSizeDelta = img.rectTransform.sizeDelta;
    }

    public void Set(string name)
    {
        SpriteUtil.SwapSpriteFromFile(img, name);
        originalSprite = img.sprite;
        nativeSizeDelta = new Vector2(img.sprite.texture.width, img.sprite.texture.height);
        Scale(xScale, yScale);
    }

    public void SetParent(LuaSpriteController parent)
    {
        img.transform.SetParent(parent.img.transform);
    }

    public void SetPivot(float x, float y)
    {
        img.rectTransform.pivot = new Vector2(x, y);
    }

    public void SetAnchor(float x, float y){
        img.rectTransform.anchorMin = new Vector2(x, y);
        img.rectTransform.anchorMax = new Vector2(x, y);
    }

    public void Scale(float xs, float ys)
    {
        xScale = xs;
        yScale = ys;
        img.rectTransform.sizeDelta = new Vector2(nativeSizeDelta.x * xScale, nativeSizeDelta.y * yScale);
    }

    public void SetAnimation(string[] frames)
    {
        SetAnimation(frames, 1 / 30f);
    }

    public void SetAnimation(string[] spriteNames, float frametime)
    {
        Keyframe[] kfArray = new Keyframe[spriteNames.Length];
        for (int i = 0; i < spriteNames.Length; i++)
        {
            kfArray[i] = new Keyframe(SpriteRegistry.Get(spriteNames[i]));
        }
        if (keyframes == null)
        {
            keyframes = img.gameObject.AddComponent<KeyframeCollection>();
            keyframes.spr = this;
        }
        else
        {
            keyframes.enabled = true;
        }
        
        keyframes.Set(kfArray, frametime);
    }

    public void StopAnimation()
    {
        if (keyframes != null)
        {
            keyframes.enabled = false;
            img.sprite = originalSprite;
        }
    }

    public void MoveTo(float x, float y)
    {
        img.rectTransform.anchoredPosition = new Vector2(x, y);
    }

    public void MoveToAbs(float x, float y)
    {
        img.rectTransform.position = new Vector2(x, y);
    }

    public void SendToTop()
    {
        img.rectTransform.SetAsLastSibling(); // in unity, the lowest UI component in the hierarchy renders last
    }

    public void SendToBottom()
    {
        img.rectTransform.SetAsFirstSibling();
    }

    public void Remove()
    {
        GameObject.Destroy(img.gameObject);
        img = null;
    }

    internal void UpdateAnimation()
    {
        if (keyframes == null)
        {
            return;
        }
        Keyframe k = keyframes.getCurrent();
        Sprite s = SpriteRegistry.GENERIC_SPRITE_PREFAB.sprite;
        
        if (k != null)
        {
            s = k.sprite;
        }

        if (img.sprite != s)
        {
            img.sprite = s;
        }
    }

    void Update(){
        UpdateAnimation();
    }
}
