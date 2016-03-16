using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;
using System;

public class LuaOverworldCharacter : OverworldCharacterController {

    internal ScriptWrapper script;

    /// <summary>
    /// Attempts to initialize the encounter's script file and bind encounter-specific functions to it.
    /// </summary>
    /// <returns>True if initialization succeeded, false if there was an error.</returns>
    private bool initScript() {
        script=new ScriptWrapper();
        script.scriptname=characterName;
        string scriptText = OverworldScriptRegistry.Get(OverworldScriptRegistry.CHARACTER_PREFIX+characterName+"/"+characterName);

        script.Bind("char",new LuaOverworldCharacterStats(this));

        try {
            script.DoString(scriptText);
        }
        catch (InterpreterException ex) {
            UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, ex.DecoratedMessage);
            return false;
        }

        script.Call("Start");

        return true;
    }

    public void Start() {
        characterName=gameObject.name;
        initScript();
    }

    public void Update() {
        script.Call("Update");
    }

}
