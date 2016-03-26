﻿using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;
using System;

public class LuaOverworldPlayer : OverworldPlayerController {

    internal ScriptWrapper script;

    /// <summary>
    /// Attempts to initialize the encounter's script file and bind encounter-specific functions to it.
    /// </summary>
    /// <returns>True if initialization succeeded, false if there was an error.</returns>
    private bool initScript() {
        script=new ScriptWrapper();
        string scriptText = ScriptRegistry.Get(ScriptRegistry.CHARACTER_PREFIX+gameObject.name+"/"+gameObject.name+"_playerScript");

        script.Bind("SetPlayerSpeed", (Action<float>)SetPlayerSpeed);
        script.Bind("controller",new LuaOverworldPlayerStats(this));
        script.Bind("object",new LuaGameObject(gameObject));

        try {
            script.DoString(scriptText);
        }
        catch (InterpreterException ex) {
            UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, ex.DecoratedMessage);
            return false;
        }

        return true;
    }

    public void Start() {
        initScript();
        base.Start();
    }

    public void Update() {
        base.Update();
        script.Call("Update");
    }

    public void SetPlayerSpeed(float val) {
        playerSpeed=val;
    }

}