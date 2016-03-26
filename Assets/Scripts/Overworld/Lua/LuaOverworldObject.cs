using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using TiledSharp;

public class LuaOverworldObject : MonoBehaviour{

    internal List<ScriptWrapper> extraScripts = new List<ScriptWrapper>();

    /// <summary>
    /// Attempts to initialize the encounter's script file and bind encounter-specific functions to it.
    /// </summary>
    /// <returns>True if initialization succeeded, false if there was an error.</returns>
    private bool initScript(string name) {

        ScriptWrapper script = new ScriptWrapper();

        script.scriptname=name;
        string scriptText = ScriptRegistry.Get(ScriptRegistry.OBJECT_PREFIX+name+"/"+name);

        try {
            script.DoString(scriptText);
        }
        catch (InterpreterException ex) {
            UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, ex.DecoratedMessage);
            return false;
        }

        script.Call("Start");
        extraScripts.Add(script);

        return true;
    }

    public void Start() {

    }

    public void Update() {
        foreach (ScriptWrapper s in extraScripts)
            s.Call("Update");
    }

    public static Dictionary<string, Type> avaliableProperties = new Dictionary<string, Type>();

    internal void UpdateToObject(TmxObjectGroup.TmxObject obj) {

        gameObject.name=obj.Name;

        //Add unity scripts via the "properties" property
        if(obj.Properties.ContainsKey("properties")){
            string[] properties = obj.Properties["properties"].Split(',');

            foreach(string s in properties) {

                if (avaliableProperties.ContainsKey(s)) {
                    gameObject.AddComponent(avaliableProperties[s]);
                }
            }
        }

        //Add lua scripts via the "scripts" property
        if (obj.Properties.ContainsKey("scripts")) {
            string[] scripts = obj.Properties["scripts"].Split(',');

            foreach(string s in scripts) {
                if (ScriptRegistry.ScriptExists(ScriptRegistry.OBJECT_PREFIX+obj.Name+"/"+s)) {
                    initScript(s);
                }
            }
        }
    }
}
