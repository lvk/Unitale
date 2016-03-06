using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using UnityEngine;

/// <summary>
/// Takes care of creating <see cref="MoonSharp.Interpreter.Script"/> objects with globally bound functions.
/// Doubles as a dictionary for the SetGlobal/GetGlobal functions attached to these scripts.
/// </summary>
public static class LuaScriptBinder
{
    private static Dictionary<string, DynValue> dict = new Dictionary<string, DynValue>();
    private static MusicManager mgr = new MusicManager();

    /// <summary>
    /// Registers C# types with MoonSharp so we can bind them to Lua scripts later.
    /// </summary>
    static LuaScriptBinder()
    {
        UserData.RegisterType<MusicManager>();              // TODO: fix functions with return values that shouldn't return anything anyway
        UserData.RegisterType<ProjectileController>();
        UserData.RegisterType<LuaArenaStatus>();
        UserData.RegisterType<LuaPlayerStatus>();
        UserData.RegisterType<LuaEnemyStatus>();
        UserData.RegisterType<LuaInputBinding>();
        UserData.RegisterType<LuaUnityTime>();
        UserData.RegisterType<ScriptWrapper>();
        UserData.RegisterType<LuaSpriteController>();
    }

    /// <summary>
    /// Generates Script object with globally defined functions and objects bound, and the os/io/file modules taken out.
    /// </summary>
    /// <returns>Script object for use within Unitale</returns>
    public static Script boundScript()
    {
        Script script = new Script();
        // library support
        script.Options.ScriptLoader = new FileSystemScriptLoader();
        ((ScriptLoaderBase)script.Options.ScriptLoader).ModulePaths = new string[] { FileLoader.pathToModFile("Lua/?.lua"), FileLoader.pathToDefaultFile("Lua/?.lua"), FileLoader.pathToModFile("Lua/Libraries/?.lua"), FileLoader.pathToDefaultFile("Lua/Libraries/?.lua") };
        // cheap sandboxing
        script.Globals["os"] = null;
        script.Globals["io"] = null;
        script.Globals["file"] = null;
        // separate function bindings
        script.Globals["SetGlobal"] = (Action<Script, string, DynValue>)LuaScriptBinder.Set;
        script.Globals["GetGlobal"] = (Func<Script, string, DynValue>)LuaScriptBinder.Get;
        script.Globals["CreateSprite"] = (Func<string, LuaSpriteController>)SpriteUtil.MakeIngameSprite;
        script.Globals["BattleDialog"] = (Action<DynValue>)LuaEnemyEncounter.BattleDialog;
        script.Globals["Encounter"] = (ScriptWrapper)LuaEnemyEncounter.script_ref;
        script.Globals["DEBUG"] = (Action<string>)UserDebugger.instance.userWriteLine;
        // clr bindings
        DynValue MusicMgr = UserData.Create(mgr);
        script.Globals.Set("Audio", MusicMgr);
        DynValue PlayerStatus = UserData.Create(PlayerController.luaStatus);
        script.Globals.Set("Player", PlayerStatus);
        DynValue InputMgr = UserData.Create(GlobalControls.luaInput);
        script.Globals.Set("Input", InputMgr);
        DynValue TimeInfo = UserData.Create(new LuaUnityTime());
        script.Globals.Set("Time", TimeInfo);
        return script;
    }

    /// <summary>
    /// Get a variable from the global dictionary.
    /// </summary>
    /// <param name="script">Script that called this function. MoonSharp fills this in automatically.</param>
    /// <param name="key">Key for the value to retrieve.</param>
    /// <returns></returns>
    public static DynValue Get(Script script, string key)
    {
        if (dict.ContainsKey(key))
        {
            // Due to how MoonSharp tables require an owner, we have to create an entirely new table if we want to work with it in other scripts.
            if (dict[key].Type == DataType.Table)
            {
                DynValue t = DynValue.NewTable(script);
                foreach (TablePair pair in dict[key].Table.Pairs)
                {
                    t.Table.Set(pair.Key, pair.Value);
                }
                return t;
            }
            else
            {
                return dict[key];
            }
        }
        return null;
    }

    /// <summary>
    /// Put a variable in the global dictionary.
    /// </summary>
    /// <param name="script">Script that called this function. MoonSharp fills this in automatically.</param>
    /// <param name="key">Key to set the value for.</param>
    /// <param name="value">Value to set for given key.</param>
    public static void Set(Script script, string key, DynValue value)
    {
         dict[key] = value;
    }

    /// <summary>
    /// Clears the global dictionary. Used in the reset functionality, as everything is reinitialized.
    /// </summary>
    public static void Clear()
    {
        dict.Clear();
    }
}