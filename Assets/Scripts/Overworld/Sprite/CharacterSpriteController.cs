using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CharacterSpriteController: MonoBehaviour {

    SpriteRenderer spr;

    Dictionary<string, string> anims = new Dictionary<string, string>();
    public string currentKey="none";
    public string walkAnimation = "walk";
    public string idleAnimation = "idle";

    public bool isAnimating = true;

    public SpriteAnimation currAnimation;
    OverworldCharacterController character;

    void Awake() {
        spr=GetComponent<SpriteRenderer>();
        character=GetComponentInParent<LuaOverworldCharacter>();
    }

    void Update() {

        if (!isAnimating)
            return;

        if (currentKey=="none") {
            isAnimating=false;
            return;
        }

        if (!anims.ContainsKey(currentKey)) {
            isAnimating=false;
            return;
        }

        if (!SpriteRegistry.AnimationExists(anims[currentKey])) {
            isAnimating=false;
            return;
        }

        currAnimation=SpriteRegistry.GetAnimation(anims[currentKey]);

        currAnimation.Progress();
        spr.sprite=currAnimation.GetSprite();

    }

    internal void SetWalkAnimationName(string obj) {
        walkAnimation=obj;
    }

    internal void SetIdleAnimationName(string obj) {
        idleAnimation=obj;
    }

    public void SetCurrentAnimation(string key) {
        if (currentKey==key)
            return;
        currAnimation.ResetProgress();
        currentKey=key;
        isAnimating=true;
    }

    public void SetAnimationValue(string key, string value) {
        if (!anims.ContainsKey(key))
            anims.Add(key, character.characterName.ToLower()+"/"+value);
        else
            anims[key]=value;
    }



}
