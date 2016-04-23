using UnityEngine;
using System.Collections;

//Lua binding for the sprite controller
public class LuaCharacterSpriteController {

    /// <summary>
    /// This Lua controller's overworld character.
    /// </summary>
    protected CharacterSpriteController controller;

    public LuaCharacterSpriteController(CharacterSpriteController p) {
        this.controller=p;
    }

    public void SetAnimationValue(string name, string value) {
        controller.SetAnimationValue(name,value);
    }

    public void SetCurrentAnimation(string set) {
        controller.SetCurrentAnimation(set);
    }

    public void SetWalkingAnimation(string set) {
        controller.SetWalkAnimationName(set);
    }

    public void SetIdleAnimation(string set) {
        controller.SetIdleAnimationName(set);
    }

}

