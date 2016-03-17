using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;

public class LuaOverworldObject : MonoBehaviour{

    internal ScriptWrapper script;

    /// <summary>
    /// Attempts to initialize the encounter's script file and bind encounter-specific functions to it.
    /// </summary>
    /// <returns>True if initialization succeeded, false if there was an error.</returns>
    private bool initScript() {
        script=new ScriptWrapper();
        script.scriptname=gameObject.name;
        string scriptText = ScriptRegistry.Get(ScriptRegistry.OBJECT_PREFIX+gameObject.name+"/"+gameObject.name);

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
        initScript();
    }

    public void Update() {
        script.Call("Update");
    }

}
