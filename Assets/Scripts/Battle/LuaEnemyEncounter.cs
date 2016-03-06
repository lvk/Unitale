using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;

internal class LuaEnemyEncounter : EnemyEncounter
{
    internal ScriptWrapper script;
    internal static ScriptWrapper script_ref;
    private Script[] waves;
    private string[] waveNames;

    public override Vector2 ArenaSize
    {
        get
        {
            if (script.GetVar("arenasize") != null)
            {
                Table size = script.GetVar("arenasize").Table;
                if (size == null)
                {
                    return base.ArenaSize;
                } 
                if (size.Get(1).Number < 16 || size.Get(2).Number < 16)
                { // TODO remove hardcoding (but player never changes size so nobody cares
                    return new Vector2(
                        size.Get(1).Number > 16 ? (int)size.Get(1).Number : 16,
                        size.Get(2).Number > 16 ? (int)size.Get(2).Number : 16
                        );
                }
                return new Vector2((int)size.Get(1).Number, (int)size.Get(2).Number);
            }
            return base.ArenaSize;
        }
    }

    /// <summary>
    /// Attempts to initialize the encounter's script file and bind encounter-specific functions to it.
    /// </summary>
    /// <returns>True if initialization succeeded, false if there was an error.</returns>
    private bool initScript()
    {
        script = new ScriptWrapper();
        script.scriptname = StaticInits.ENCOUNTER;
        string scriptText = ScriptRegistry.Get(ScriptRegistry.ENCOUNTER_PREFIX + StaticInits.ENCOUNTER);
        try
        {
            script.DoString(scriptText);
        }
        catch (InterpreterException ex)
        {
            UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, ex.DecoratedMessage);
            return false;
        }
        script.Bind("RandomEncounterText", (Func<string>)RandomEncounterText);
        script.Bind("CreateProjectile", (Func<Script, string, float, float, DynValue>)CreateProjectile);
        script.Bind("CreateProjectileAbs", (Func<Script, string, float, float, DynValue>)CreateProjectileAbs);
        script_ref = script;
        return true;
    }

    private DynValue CreateProjectileAbs(Script s, string sprite, float xpos, float ypos)
    {
        LuaProjectile projectile = (LuaProjectile)BulletPool.instance.Retrieve();
        SpriteUtil.SwapSpriteFromFile(projectile, sprite);
        projectile.owner = s;
        projectile.gameObject.SetActive(true); 
        projectile.ctrl.MoveToAbs(xpos, ypos);
        //projectile.ctrl.z = Projectile.Z_INDEX_NEXT; //doesn't work yet, thanks unity UI
        projectile.transform.SetAsLastSibling();
        projectile.ctrl.UpdatePosition();
        DynValue projectileController = UserData.Create(projectile.ctrl);
        return projectileController;
    }

    private DynValue CreateProjectile(Script s, string sprite, float xpos, float ypos)
    {
        return CreateProjectileAbs(s, sprite, ArenaSizer.arenaCenter.x + xpos, ArenaSizer.arenaCenter.y + ypos);
    }

    private void prepareWave()
    {
        DynValue nextWaves = script.GetVar("nextwaves");
        waves = new Script[nextWaves.Table.Length];
        waveNames = new string[waves.Length];
        int currentWaveScript = 0;
        try
        {
            for (int i = 0; i < waves.Length; i++)
            {
                currentWaveScript = i;
                waves[i] = LuaScriptBinder.boundScript();
                DynValue ArenaStatus = UserData.Create(ArenaSizer.luaStatus);
                waves[i].Globals.Set("Arena", ArenaStatus);
                waves[i].Globals["State"] = (Action<string>)UIController.instance.SwitchStateOnString;
                waves[i].Globals["CreateProjectile"] = (Func<Script, string, float, float, DynValue>)CreateProjectile;
                waves[i].Globals["CreateProjectileAbs"] = (Func<Script, string, float, float, DynValue>)CreateProjectileAbs;
                waves[i].Globals["EndWave"] = (Action)endWaveTimer;
                if (nextWaves.Table.Get(i + 1).Type != DataType.String){
                    UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, "Non-string value encountered in nextwaves table");
                    return;
                } else {
                    waveNames[i] = nextWaves.Table.Get(i + 1).String;
                }
                waves[i].DoString(ScriptRegistry.Get(ScriptRegistry.WAVE_PREFIX + nextWaves.Table.Get(i + 1).String));
            }
        }
        catch (InterpreterException ex)
        {
            UnitaleUtil.displayLuaError(nextWaves.Table.Get(currentWaveScript + 1).String + ".lua", ex.DecoratedMessage);
        }
    }

    public new void Awake()
    {
        if (initScript())
        {
            loadEnemiesAndPositions();
        }
        CanRun = true;
    }

    protected override void loadEnemiesAndPositions()
    {
        AudioSource musicSource = Camera.main.GetComponent<AudioSource>();
        EncounterText = script.GetVar("encountertext").String;
        DynValue enemyScriptsLua = script.GetVar("enemies");
        DynValue enemyPositionsLua = script.GetVar("enemypositions");
        string musicFile = script.GetVar("music").String;

        try
        {
            enemies = new LuaEnemyController[enemyScriptsLua.Table.Length]; // dangerously assumes enemies is defined
        }
        catch (Exception)
        {
            UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, "There's no enemies table in your encounter. Is this a pre-0.1.2 encounter? It's easy to fix!\n\n"
                + "1. Create a Monsters folder in the mod's Lua folder\n"
                + "2. Add the monster script (custom.lua) to this new folder\n"
                + "3. Add the following line to the beginning of this encounter script, located in the mod folder/Lua/Encounters:\nenemies = {\"custom\"}\n"
                + "4. You're done! Starting from 0.1.2, you can name your monster and encounter scripts anything.");
            return;
        }
        if (enemyPositionsLua != null && enemyPositionsLua.Table != null)
        {
            enemyPositions = new Vector2[enemyPositionsLua.Table.Length];
            for (int i = 0; i < enemyPositionsLua.Table.Length; i++)
            {
                Table posTable = enemyPositionsLua.Table.Get(i + 1).Table;
                if (i >= enemies.Length)
                    break;

                enemyPositions[i] = new Vector2((float)posTable.Get(1).Number, (float)posTable.Get(2).Number);
            }
        }

        if (musicFile != null)
        {
            try
            {
                AudioClip music = AudioClipRegistry.GetMusic(musicFile);
                musicSource.clip = music;
            }
            catch (Exception)
            {
                Debug.Log("Loading custom music failed.");
            }
        }
        else
        {
            musicSource.clip = AudioClipRegistry.GetMusic("mus_battle1");
        }

        // Instantiate all the enemy objects
        if (enemies.Length > enemyPositions.Length)
        {
            UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, "All enemies in an encounter must have a screen position defined. Either your enemypositions table is missing, "
                + "or there are more enemies than available positions. Refer to the documentation's Basic Setup section on how to do this.");
        }
        enemyInstances = new GameObject[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            enemyInstances[i] = Instantiate(Resources.Load<GameObject>("Prefabs/LUAEnemy"));
            enemyInstances[i].transform.SetParent(gameObject.transform);
            enemyInstances[i].transform.localScale = new Vector3(1, 1, 1); // apparently this was suddenly required or the scale would be (0,0,0)
            enemies[i] = enemyInstances[i].GetComponent<LuaEnemyController>();
            enemies[i].scriptName = enemyScriptsLua.Table.Get(i + 1).String;
            if (i < enemyPositions.Length)
                enemies[i].GetComponent<RectTransform>().anchoredPosition = enemyPositions[i];
            else
                enemies[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 1);
        }

        // Attach the controllers to the encounter's enemies table
        DynValue[] enemyStatusCtrl = new DynValue[enemies.Length];
        Table luaEnemyTable = script.GetVar("enemies").Table;
        for (int i = 0; i < enemyStatusCtrl.Length; i++)
        {
            //enemies[i].luaStatus = new LuaEnemyStatus(enemies[i]);
            enemies[i].script = new ScriptWrapper();
            luaEnemyTable.Set(i + 1, UserData.Create(enemies[i].script));
        }
        script.SetVar("enemies", DynValue.NewTable(luaEnemyTable));
        musicSource.Play(); // play that funky music
    }

    public override void HandleItem(UnderItem item)
    {
        if (!CustomItemHandler(item))
            item.inCombatUse();
    }

    public bool CallOnSelfOrChildren(string func, DynValue[] param = null)
    {
        bool result;
        if (param != null)
            result = TryCall(func, param);
        else
            result = TryCall(func);

        if (!result)
        {
            bool calledOne = false;
            foreach (LuaEnemyController enemy in enemies)
            {
                if (param != null)
                {
                    if (enemy.TryCall(func, param))
                    {
                        calledOne = true;
                    }
                }
                else
                {
                    if (enemy.TryCall(func))
                    {
                        calledOne = true;
                    }
                }
            }

            return calledOne;
        }
        else
        {
            return true;
        }
    }

    public bool TryCall(string func, DynValue[] param = null)
    {
        try
        {
            if (script.GetVar(func) == null)
                return false;
            if (param != null)
                script.Call(func, param);
            else
                script.Call(func);
            return true;
        }
        catch (InterpreterException ex)
        {
            UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, ex.DecoratedMessage);
            return true;
        }
    }

    public override void HandleSpare()
    {
        /*
        if (script.GetVar("HandleSpare") == null)
            base.HandleSpare();
        else
            if (!script.Call(script.Globals["HandleSpare"]).Boolean)
                base.HandleSpare();
         */
        base.HandleSpare();
    }

    // /<summary>
    // /Overrideable item handler on a per-encounter basis. Should return true if a custom action is executed for the given item.
    // /</summary>
    // /<param name="item">Item to be checked for custom action</param>
    // /<returns>true if a custom action should be executed for given item, false if the default action should happen</returns>
    protected virtual bool CustomItemHandler(UnderItem item)
    {
        return CallOnSelfOrChildren("HandleItem", new DynValue[] { DynValue.NewString(item.ID) });
    }

    public override void updateWave()
    {
        string currentScript = "";
        try
        {
            for (int i=0;i<waves.Length;i++)
            {
                currentScript = waveNames[i];
                waves[i].Call(waves[i].Globals["Update"]);
            }
        }
        catch (InterpreterException ex)
        {
            UnitaleUtil.displayLuaError(currentScript, ex.DecoratedMessage);
            return;
        }
    }

    public override void nextWave()
    {
        turnCount++;
        prepareWave();
        if (script.GetVar("wavetimer") != null)
            waveTimer = Time.time + (float)script.GetVar("wavetimer").Number;
        else
            waveTimer = Time.time + 4.0f;
    }

    private void endWaveTimer()
    {
        waveTimer = Time.time;
    }

    public override void endWave()
    {
        CallOnSelfOrChildren("DefenseEnding");
        EncounterText = script.GetVar("encountertext").String;
        // Projectile.Z_INDEX_NEXT = Projectile.Z_INDEX_INITIAL; // doesn't work yet
    }

    public bool waveInProgress()
    {
        if (Time.time < waveTimer)
        {
            return true;
        }
        return false;
    }

    public static void BattleDialog(DynValue arg)
    {
        TextMessage[] msgs = null;
        if (arg.Type == DataType.String)
        {
            msgs = new TextMessage[]{new RegularMessage(arg.String)};
        }
        else if (arg.Type == DataType.Table)
        {
            msgs = new TextMessage[arg.Table.Length];
            for (int i = 0; i < arg.Table.Length; i++)
            {
                msgs[i] = new RegularMessage(arg.Table.Get(i + 1).String);
            }
        }
        UIController.instance.ActionDialogResult(msgs, UIController.UIState.ENEMYDIALOGUE);
    }

    /*public static void BattleDialog(List<string> lines)
    {
        TextMessage[] msgs = new TextMessage[lines.Count];
        for (int i = 0; i < lines.Count; i++)
        {
            msgs[i] = new RegularMessage(lines[i]);
        }
        UIController.instance.ActionDialogResult(msgs, UIController.UIState.ENEMYDIALOGUE);
    }*/
}