#pragma warning disable 0649

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Behaviour that retrieves resources from the built-in registry rather than setting them on the components in the Unity Editor.
/// </summary>
class AutoloadResourcesFromRegistry : MonoBehaviour
{
    [Header("Image Resource")]
    public bool SetNativeSize;
    public string SpritePath;
    [Header("Audio Resource")]
    public bool Loop;
    public string SoundPath;

    void Awake()
    {
        if (StaticInits.Initialized)
        {
            LateStart();
        }
        else
        {
            LateUpdater.lateInit.Add(LateStart);
        }
    }

    void LateStart()
    {
        if(!string.IsNullOrEmpty(SpritePath)){
            Image img = GetComponent<Image>();
            if (img != null)
            {
                img.sprite = SpriteRegistry.Get(SpritePath);
                if (SetNativeSize)
                {
                    img.SetNativeSize();
                }
            }

            ParticleSystem psys = GetComponent<ParticleSystem>();
            if (psys != null)
            {
                ParticleSystemRenderer prender = GetComponent<ParticleSystemRenderer>();
                prender.material.mainTexture = SpriteRegistry.Get(SpritePath).texture;
            }
        }

        if(!string.IsNullOrEmpty(SoundPath)){
            AudioSource aSrc = GetComponent<AudioSource>();
            aSrc.clip = AudioClipRegistry.Get(SoundPath);
            aSrc.loop = Loop;
        }
    }
}
