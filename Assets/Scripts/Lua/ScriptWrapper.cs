using UnityEngine;
using System.Collections;
using MoonSharp.Interpreter;
using System;

public class ScriptWrapper {
    private Script script;
    internal string scriptname = "???";

    public DynValue this[string key]
    {
        get { return this.GetVar(key); }
        set { this.SetVar(key, value); }
    }

    public ScriptWrapper()
    {
        script = LuaScriptBinder.boundScript();
        this.Bind("_getv", (Func<Script, string, DynValue>)this.GetVar);
        script.DoString("setmetatable({}, {__index=function(t, name) return _getv(name); end})");
    }

    internal DynValue DoString(string source)
    {
        return script.DoString(source);
    }

    public void SetVar(string key, DynValue value)
    {
        script.Globals.Set(key, MoonSharpUtil.CloneIfRequired(script, value));
    }

    public DynValue GetVar(string key)
    {
        return GetVar(null, key);
    }

    public DynValue GetVar(Script caller, string key)
    {
        DynValue value = script.Globals.Get(key);

        if (value == null || value.IsNil())
        {
            return DynValue.NewNil();
        }

        if (caller == null)
        {
            return value;
        }

        if (script.Globals[key] != null)
        {
            return MoonSharpUtil.CloneIfRequired(caller, value);
        }
        return null;
    }

    public DynValue Call(string function, DynValue[] args = null)
    {
        return this.Call(null, function, args);
    }

    public DynValue Call(string function, DynValue arg)
    {
        if (arg == null)
        {
            return this.Call(function);
        }
        else
        {
            return this.Call(function, new DynValue[] { arg });
        }
    }

    public DynValue Call(Script caller, string function, DynValue[] args = null)
    {
        if (script.Globals[function] == null || script.Globals.Get(function) == null)
        {
            return null;
            //UnitaleUtil.displayLuaError(scriptname, "Attempted to call the function " + function + " but it didn't exist.");
        }

        if(args != null){
            return script.Call(script.Globals[function], args);
        } else {
            return script.Call(script.Globals[function]);
        }
    }

    internal void Bind(string key, object func)
    {
        script.Globals[key] = func;
    }
}
