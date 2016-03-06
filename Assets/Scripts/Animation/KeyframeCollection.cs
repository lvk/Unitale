using UnityEngine;
using System.Collections;

public class KeyframeCollection : MonoBehaviour {
    public float timePerFrame = 1 / 30f;
    public Keyframe[] keyframes;
    internal float registrationTime;
    internal LuaSpriteController spr;
    internal LoopMode loop = LoopMode.LOOP;
    private float totalTime;
    private Keyframe EMPTY_KEYFRAME = new Keyframe(SpriteRegistry.EMPTY_SPRITE);

    public enum LoopMode
    {
        ONESHOT,
        LOOP
    }

    public void Set(Keyframe[] keyframes, float timePerFrame = 1/30f)
    {
        this.keyframes = keyframes;
        this.timePerFrame = timePerFrame;
        totalTime = timePerFrame * keyframes.Length;
        registrationTime = Time.time;
    }

    public Keyframe getCurrent()
    {
        if (loop == LoopMode.LOOP)
        {
            int index = (int)(((Time.time - registrationTime) % totalTime) / timePerFrame);
            //Debug.Log("i: "+index+"  tt: " + totalTime + "  mod: " + (Time.time-registrationTime)%totalTime);
            return keyframes[index];
        }
        else if (loop == LoopMode.ONESHOT)
        {
            int index = (int)((Time.time - registrationTime) / timePerFrame);
            if (index >= keyframes.Length)
            {
                return EMPTY_KEYFRAME;
            }
            return keyframes[index];
        }
        return null;
    }

    public bool animationComplete()
    {
        if (loop == LoopMode.ONESHOT)
        {
            return ((Time.time - registrationTime) / timePerFrame) >= keyframes.Length;
        }
        return false;
    }

    void Update()
    {
        spr.UpdateAnimation();
    }
}
