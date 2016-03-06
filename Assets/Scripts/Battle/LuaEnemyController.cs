using System;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

public class LuaEnemyController : EnemyController
{
    public LuaEnemyStatus luaStatus;

    internal string scriptName;
    internal ScriptWrapper script;
    internal bool inFight = true; // if false, enemy will no longer be considered as an option in menus and such
    private string lastBubbleName;
    private bool error = false;

    internal bool spared = false;
    internal bool killed = false;

    public override string Name
    {
        get
        {
            return script.GetVar("name").String;
        }

        set
        {
            script.SetVar("name", DynValue.NewString(value));
        }
    }

    public override string[] ActCommands
    {
        get
        {
            if (error)
            {
                return new string[] { "LUA error" };
            }
            DynValue actCmds = script.GetVar("commands");
            string[] tempActCmds;
            int add = 0;
            if (!CanCheck)
            {
                tempActCmds = new string[actCmds.Table.Length];
            }
            else
            {
                tempActCmds = new string[actCmds.Table.Length + 1];
                tempActCmds[0] = "Check"; // HACK: remove hardcoding of Check, but otherwise gets converted to Tuple? idk
                add = 1;
            }
            for (int i = add; i < actCmds.Table.Length + add; i++)
            {
                tempActCmds[i] = actCmds.Table.Get(i - add + 1).String;
            }
            return tempActCmds;
        }

        set
        {
            DynValue[] values = new DynValue[value.Length];
            for (int i = 0; i < value.Length; i++)
                values[i] = DynValue.NewString(value[i]);
            script.SetVar("commands", DynValue.NewTuple(values));
        }
    }

    public override string[] Comments
    {
        get
        {
            DynValue comments = script.GetVar("comments");
            string[] tempComments = new string[comments.Table.Length];
            for (int i = 0; i < comments.Table.Length; i++)
            {
                tempComments[i] = comments.Table.Get(i + 1).String;
            }
            return tempComments;
        }

        set
        {
            DynValue[] values = new DynValue[value.Length];
            for (int i = 0; i < value.Length; i++)
                values[i] = DynValue.NewString(value[i]);
            script.SetVar("comments", DynValue.NewTuple(values));
        }
    }

    public override string[] Dialogue
    {
        get
        {
            DynValue randDialogue = script.GetVar("randomdialogue");
            if (randDialogue == null || randDialogue.Table == null)
                return null;
            string[] tempDialogue = new string[randDialogue.Table.Length];
            for (int i = 0; i < randDialogue.Table.Length; i++)
            {
                tempDialogue[i] = randDialogue.Table.Get(i + 1).String;
            }
            return tempDialogue;
        }

        set
        {
            DynValue[] values = new DynValue[value.Length];
            for (int i = 0; i < value.Length; i++)
                values[i] = DynValue.NewString(value[i]);
            script.SetVar("randomdialogue", DynValue.NewTuple(values));
        }
    }

    public override string CheckData
    {
        get
        {
            return script.GetVar("check").String;
        }

        set
        {
            script.SetVar("check", DynValue.NewString(value));
        }
    }

    public override int HP
    {
        get
        {
            return (int)script.GetVar("hp").Number;
        }

        set
        {
            script.SetVar("hp", DynValue.NewNumber(value));
        }
    }

    public override int Attack
    {
        get
        {
            return (int)script.GetVar("atk").Number;
        }

        set
        {
            script.SetVar("atk", DynValue.NewNumber(value));
        }
    }

    public override int Defense
    {
        get
        {
            return (int)script.GetVar("def").Number;
        }

        set
        {
            script.SetVar("def", DynValue.NewNumber(value));
        }
    }

    public override int XP
    {
        get
        {
            return (int)script.GetVar("xp").Number;
        }

        set
        {
            script.SetVar("xp", DynValue.NewNumber(value));
        }
    }

    public override int Gold
    {
        get
        {
            return (int)script.GetVar("gold").Number;
        }

        set
        {
            script.SetVar("gold", DynValue.NewNumber(value));
        }
    }

    public override string DialogBubble
    {
        get
        {
            if (script.GetVar("dialogbubble") == null)
                return "UI/SpeechBubbles/right";

            string bubbleToGet = script.GetVar("dialogbubble").String;
            return "UI/SpeechBubbles/" + bubbleToGet;
        }
    }

    public override bool CanSpare
    {
        get
        {
            DynValue spareVal = script.GetVar("canspare");
            if (spareVal == null)
                return false;
            return spareVal.Boolean;
        }

        set
        {
            script.SetVar("canspare", DynValue.NewBoolean(value));
        }
    }

    public override bool CanCheck
    {
        get
        {
            DynValue checkVal = script.GetVar("cancheck");
            if (checkVal == null)
                return true;
            return checkVal.Boolean;
        }

        set
        {
            script.SetVar("cancheck", DynValue.NewBoolean(value));
        }
    }

    private void Start()
    {
        try
        {
            string scriptText = ScriptRegistry.Get(ScriptRegistry.MONSTER_PREFIX + scriptName);
            if (scriptText == null)
            {
                UnitaleUtil.displayLuaError(StaticInits.ENCOUNTER, "Tried to load monster script " + scriptName + ".lua but it didn't exist. Is it misspelled?");
                return;
            }

            script.scriptname = scriptName;
            script.Bind("SetSprite", (Action<string>)SetSprite);
            script.Bind("SetActive", (Action<bool>)SetActive);
            script.Bind("Kill", (Action)DoKill);
            script.Bind("Spare", (Action)DoSpare);
            script.DoString(scriptText);

            string spriteFile = script.GetVar("sprite").String;
            if (spriteFile != null)
                SetSprite(spriteFile);
            else
                throw new InvalidOperationException("missing sprite");

            ui = FindObjectOfType<UIController>();
            maxHP = HP;
            currentHP = HP;
            textBubbleSprite = Resources.Load<Sprite>("Sprites/UI/SpeechBubbles/right");

            /*if (script.GetVar("canspare") == null)
            {
                CanSpare = false;
            }
            if (script.GetVar("cancheck") == null)
            {
                CanCheck = true;
            }*/
        }
        catch (InterpreterException ex)
        {
            UnitaleUtil.displayLuaError(scriptName, ex.DecoratedMessage);
        }
        catch (Exception ex)
        {
            UnitaleUtil.displayLuaError(scriptName, "Unknown error. Usually means you're missing a sprite.\nSee documentation for details.\nStacktrace below in case you wanna notify a dev.\n" + ex.StackTrace);
        }
    }

    public override void HandleAttack(int hitStatus)
    {
        TryCall("HandleAttack", new DynValue[] { DynValue.NewNumber(hitStatus) });
    }

    /*public override string GetRegularScreenDialog()
    {
        if (!error)
            return script.Call(script.Globals["GetRegularScreenDialog"]).String;
        else
        {
            UIController.instance.textmgr.setFont(SpriteFontRegistry.F_UI_MONSTERDIALOG);
            luaErrorMsg = luaErrorMsg.Replace("\\n", "").Replace("\\r", "").Replace("\\t", "");
            for (int i = 0; i < luaErrorMsg.Length; i++)
                if (i > 0 && i % 40 == 0)
                {
                    luaErrorMsg = luaErrorMsg.Insert(i, "\\r");
                }
            return "[starcolor:ffffff][color:ffffff]LUA error.\n" + luaErrorMsg;
        }
    }*/

    public override string[] GetDefenseDialog()
    {
        if (!error)
        {
            DynValue dialogues = script.GetVar("currentdialogue");
            if (dialogues == null || dialogues.Table == null)
                if (dialogues.String != null)
                    return new string[] { dialogues.String };
                else if (Dialogue == null)
                    return null;
                else
                    return new string[] { Dialogue[UnityEngine.Random.Range(0, Dialogue.Length)] };

            string[] dialogueStrings = new string[dialogues.Table.Length];
            for (int i = 0; i < dialogues.Table.Length; i++)
            {
                dialogueStrings[i] = dialogues.Table.Get(i + 1).String;
            }
            script.SetVar("currentdialogue", DynValue.NewNil());
            return dialogueStrings;
        }
        else
        {
            return new string[] { "LUA\nerror." };
        }
    }

    public bool TryCall(string func, DynValue[] param = null)
    {
        try
        {
            DynValue sval = script.GetVar(func);
            if (sval == null || sval.Type == DataType.Nil)
                return false;
            if (param != null)
                script.Call(func, param);
            else
                script.Call(func);
            return true;
        }
        catch (InterpreterException ex)
        {
            UnitaleUtil.displayLuaError(scriptName, ex.DecoratedMessage);
            return true;
        }
    }

    protected override void HandleCustomCommand(string command)
    {
        TryCall("HandleCustomCommand", new DynValue[] { DynValue.NewString(command) });
    }

    public void SetSprite(string filename)
    {
        SpriteUtil.SwapSpriteFromFile(this, filename);
    }

    /// <summary>
    /// Call function to grey out enemy and pop the smoke particles, and mark it as spared. 
    /// </summary>
    public void DoSpare()
    {
        if (!inFight)
        {
            return;
        }
        // We have to code the particles separately because they don't work well in UI screenspace. Great stuff.
        ParticleSystem spareSmoke = Resources.Load<ParticleSystem>("Prefabs/MonsterSpareParticleSys");
        spareSmoke = Instantiate(spareSmoke);
        spareSmoke.Emit(10);
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[10];
        spareSmoke.GetParticles(particles);
        Vector3 particlePos = RTUtil.AbsCenterOf(GetComponent<RectTransform>());
        particlePos.z = 5;
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].position = particlePos + particles[i].velocity.normalized * 5;
        }
        spareSmoke.SetParticles(particles, particles.Length);

        // The actually relevant part of sparing code.
        GetComponent<Image>().color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
        UIController.playSoundSeparate(AudioClipRegistry.GetSound("enemydust"));
        SetActive(false);
        spared = true;

        UIController.instance.checkAndTriggerVictory();
    }

    /// <summary>
    /// Call function to turn enemy to dust and mark it as killed.
    /// </summary>
    public void DoKill()
    {
        if (!inFight)
        {
            return;
        }
        GetComponent<ParticleDuplicator>().Activate();
        SetActive(false);
        killed = true;
        UIController.playSoundSeparate(AudioClipRegistry.GetSound("enemydust"));

        UIController.instance.checkAndTriggerVictory();
    }

    /// <summary>
    /// Set if we should consider this monster for menus e.g.
    /// </summary>
    /// <param name="active"></param>
    public void SetActive(bool active)
    {
        this.inFight = active;
    }
}