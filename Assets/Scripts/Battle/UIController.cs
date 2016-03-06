using System;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The class responsible for making some people lose faith in the project. In very dire need of refactoring, 
/// but hard to do until functionality can be split into battle and overworld functions.
/// 
/// As it stands this class is a messy finite state machine that takes care of controlling not only the battle,
/// but also a lot of things it shouldn't (text manager, enemy dialogue, keyboard controls etc.)
/// If you're familiar with the term cyclomatic complexity, you probably wouldn't want to hire me at this point.
/// 
/// The eventual redesign of the UI controller will try to change over as much of the functionality to Lua.
/// As we're missing some key functionality to accomplish this, refactoring has been put off for now.
/// </summary>
public class UIController : MonoBehaviour
{
    public static UIController instance;
    internal TextManager textmgr;

    private static Sprite actB1;
    private static Sprite fightB1;
    private static Sprite itemB1;
    private static Sprite mercyB1;
    private Image actBtn;
    private Actions action = Actions.FIGHT;
    private GameObject arenaParent;
    private GameObject canvasParent;
    internal LuaEnemyEncounter encounter;
    private Image fightBtn;
    private FightUIController fightUI;
    private Vector2 initialHealthPos = new Vector2(250, -10); // initial healthbar position for target selection
    private Image itemBtn;
    private Image mercyBtn;

    private TextManager[] monDialogues;

    // DEBUG Making running away a bit more fun. Remove this later.
    private bool musicPausedFromRunning = false;
    private int runawayattempts = 0;

    private int selectedAction = 0;
    private int selectedEnemy = 0;
    private int selectedItem = 0;
    private int selectedMercy = 0;
    private AudioClip sndConfirm;
    private AudioClip sndSelect;
    private UIState state;
    private UIState stateAfterDialogs = UIState.DEFENDING;
    private AudioSource uiAudio;
    private Vector2 upperLeft = new Vector2(65, 190); // coordinates of the upper left menu choice
    private bool victory = false;
    private bool encounterHasUpdate = false; //used to check if encounter has an update function for the sake of optimization
    private bool parentStateCall = true;
    private bool childStateCalled = false;

    public enum Actions
    {
        FIGHT,
        ACT,
        ITEM,
        MERCY
    }

    public enum UIState
    {
        NONE, // initial state. Used to see if a modder has changed the state before the UI controller wants to.
        ACTIONSELECT, // selecting an action (FIGHT/ACT/ITEM/MERCY)
        ATTACKING, // attack window with the rhythm thing
        DEFENDING, // being attacked by enemy, waves spawn here
        ENEMYSELECT, // selecting an enemy target for FIGHT or ACT
        ACTMENU, // open up the act menu
        ITEMMENU, // open up the item menu
        MERCYMENU, // open up the mercy menu
        ENEMYDIALOGUE, // player is visible and arena is resizing, but enemy still has own dialogue
        DIALOGRESULT, // executed an action that results in dialogue that results in UIState.ENEMYDIALOG or UIState.DEFENDING
        DONE // Finished state of battle. Currently just returns to the mod selection screen.
    }

    public void ActionDialogResult(TextMessage msg, UIState afterDialogState, ScriptWrapper caller = null)
    {
        ActionDialogResult(new TextMessage[] { msg }, afterDialogState, caller);
    }

    public void ActionDialogResult(TextMessage[] msg, UIState afterDialogState, ScriptWrapper caller = null)
    {
        stateAfterDialogs = afterDialogState;
        if (caller != null)
        {
            textmgr.setCaller(caller);
        }
        textmgr.setTextQueue(msg);
        SwitchState(UIState.DIALOGRESULT);
    }

    public void ShowError(TextMessage msg)
    {
        ActionDialogResult(msg, UIState.ACTIONSELECT); // otherwise just display it normally
    }

    public void SwitchState(UIState state)
    {
        //Pre-state
        if (parentStateCall)
        {
            parentStateCall = false;
            encounter.script.Call("EnteringState", new DynValue[]{DynValue.NewString(state.ToString()), DynValue.NewString(this.state.ToString())});
            parentStateCall = true;

            if (childStateCalled)
            {
                childStateCalled = false;
                return;
            }
        }
        else
        {
            childStateCalled = true;
        }

        // TODO Quick and dirty addition to add some humor to the Run away command.
        // Will be removed without question.
        if (musicPausedFromRunning)
        {
            Camera.main.GetComponent<AudioSource>().UnPause();
            musicPausedFromRunning = false;
        }
        // END DEBUG

        // below: actions based on ending a previous state, or actions that affect multiple states
        if (this.state == UIState.DEFENDING && state != UIState.DEFENDING)
        {
            encounter.endWave();
        }

        if (state != UIState.ENEMYDIALOGUE && state != UIState.DEFENDING)
        {
            ArenaSizer.instance.Resize(ArenaSizer.UIWidth, ArenaSizer.UIHeight);
            PlayerController.instance.invulTimer = 0.0f;
            PlayerController.instance.setControlOverride(true);
        }

        if (this.state == UIState.ENEMYSELECT && action == Actions.FIGHT)
        {
            foreach (LifeBarController lbc in arenaParent.GetComponentsInChildren<LifeBarController>())
            {
                Destroy(lbc.gameObject);
            }
        }

        if (state == UIState.DEFENDING || state == UIState.ENEMYDIALOGUE)
        {
            textmgr.destroyText();
            PlayerController.instance.SetPosition(320, 160, false);
            PlayerController.instance.GetComponent<Image>().enabled = true;
            fightBtn.overrideSprite = null;
            actBtn.overrideSprite = null;
            itemBtn.overrideSprite = null;
            mercyBtn.overrideSprite = null;
            textmgr.setPause(true);
        }

        if (this.state == UIState.ENEMYDIALOGUE)
        {
            TextManager[] textmen = FindObjectsOfType<TextManager>();
            foreach (TextManager textman in textmen)
            {
                if (textman.gameObject.name.StartsWith("DialogBubble")) // game object name is hardcoded as it won't change
                {
                    Destroy(textman.gameObject);
                }
            }
        }

        this.state = state;
        switch (state)
        {
            case UIState.ATTACKING:
                textmgr.destroyText();
                PlayerController.instance.GetComponent<Image>().enabled = false;
                fightUI.Init(encounter.enabledEnemies[selectedEnemy]);
                break;

            case UIState.ACTIONSELECT:
                PlayerController.instance.setControlOverride(true);
                PlayerController.instance.GetComponent<Image>().enabled = true;
                setPlayerOnAction(action);
                textmgr.setPause(ArenaSizer.instance.isResizeInProgress());
                textmgr.setCaller(encounter.script); // probably not necessary due to ActionDialogResult changes
                textmgr.setText(new RegularMessage(encounter.EncounterText));
                break;

            case UIState.ACTMENU:
                string[] actions = new string[encounter.enabledEnemies[selectedEnemy].ActCommands.Length];
                for (int i = 0; i < actions.Length; i++)
                {
                    actions[i] = encounter.enabledEnemies[selectedEnemy].ActCommands[i];
                }

                selectedAction = 0;
                setPlayerOnSelection(selectedAction);
                textmgr.setText(new SelectMessage(actions, false));
                break;

            case UIState.ITEMMENU:
                string[] items = getInventoryPage(0);
                selectedItem = 0;
                setPlayerOnSelection(0);
                textmgr.setText(new SelectMessage(items, false));
                /*ActionDialogResult(new TextMessage[] {
                    new TextMessage("Can't open inventory.\nClogged with pasta residue.", true, false),
                    new TextMessage("Might also be a dog.\nIt's ambiguous.",true,false)
                }, UIState.ENEMYDIALOG);*/
                break;

            case UIState.MERCYMENU:
                selectedMercy = 0;
                string[] mercyopts = new string[1 + (encounter.CanRun ? 1 : 0)];
                mercyopts[0] = "Spare";
                foreach (EnemyController enemy in encounter.enabledEnemies)
                {
                    if (enemy.CanSpare)
                    {
                        mercyopts[0] = "[starcolor:ffff00][color:ffff00]" + mercyopts[0] + "[color:ffffff]";
                        break;
                    }
                }
                if (encounter.CanRun)
                {
                    mercyopts[1] = "Flee";
                }
                setPlayerOnSelection(0);
                textmgr.setText(new SelectMessage(mercyopts, true));
                break;

            case UIState.ENEMYSELECT:
                string[] names = new string[encounter.enabledEnemies.Length];
                string[] colorPrefixes = new string[names.Length];
                for (int i = 0; i < encounter.enabledEnemies.Length; i++)
                {
                    names[i] = encounter.enabledEnemies[i].Name;
                    if (encounter.enabledEnemies[i].CanSpare)
                    {
                        colorPrefixes[i] = "[color:ffff00]";
                    }
                }

                textmgr.setText(new SelectMessage(names, true, colorPrefixes));
                if (action == Actions.FIGHT)
                {
                    int maxWidth = (int)initialHealthPos.x;
                    for (int i = 0; i < encounter.enabledEnemies.Length; i++)
                    {
                        int mNameWidth = UnitaleUtil.fontStringWidth(textmgr.Charset, "* " + encounter.enabledEnemies[i].Name) + 50;
                        if (mNameWidth > maxWidth)
                        {
                            maxWidth = mNameWidth;
                        }
                    }
                    for (int i = 0; i < encounter.enabledEnemies.Length; i++)
                    {
                        LifeBarController lifebar = Instantiate(Resources.Load<LifeBarController>("Prefabs/HPBar"));
                        lifebar.transform.SetParent(textmgr.transform);
                        RectTransform lifebarRt = lifebar.GetComponent<RectTransform>();
                        lifebarRt.anchoredPosition = new Vector2(maxWidth, initialHealthPos.y - i * textmgr.Charset.LineSpacing);
                        lifebarRt.sizeDelta = new Vector2(90, lifebarRt.sizeDelta.y);
                        lifebar.setFillColor(Color.green);
                        float hpFrac = (float)encounter.enabledEnemies[i].HP / (float)encounter.enabledEnemies[i].getMaxHP();
                        lifebar.setInstant(hpFrac);
                    }
                }

                if (selectedEnemy >= encounter.enabledEnemies.Length)
                    selectedEnemy = 0;
                setPlayerOnSelection(selectedEnemy * 2); // single list so skip right row by multiplying x2
                break;

            case UIState.DEFENDING:
                ArenaSizer.instance.Resize((int)encounter.ArenaSize.x, (int)encounter.ArenaSize.y);
                PlayerController.instance.setControlOverride(false);
                encounter.nextWave();
                // ActionDialogResult(new TextMessage("This is where you'd\rdefend yourself.\nBut the code was spaghetti.", true, false), UIState.ACTIONSELECT);
                break;

            case UIState.DIALOGRESULT:
                PlayerController.instance.GetComponent<Image>().enabled = false;
                break;

            case UIState.ENEMYDIALOGUE:
                PlayerController.instance.GetComponent<Image>().enabled = true;
                ArenaSizer.instance.Resize(155, 130);
                encounter.CallOnSelfOrChildren("EnemyDialogueStarting");
                monDialogues = new TextManager[encounter.enabledEnemies.Length];
                for (int i = 0; i < encounter.enabledEnemies.Length; i++)
                {
                    string[] msgs = encounter.enabledEnemies[i].GetDefenseDialog();
                    if (msgs == null)
                    {
                        UserDebugger.warn("Entered ENEMYDIALOGUE, but no current/random dialogue was set for " + encounter.enabledEnemies[i].Name);
                        SwitchState(UIState.DEFENDING);
                        break;
                    }
                    GameObject speechBub = Instantiate(SpriteFontRegistry.BUBBLE_OBJECT);
                    RectTransform enemyRt = encounter.enabledEnemies[i].GetComponent<RectTransform>();
                    TextManager sbTextMan = speechBub.GetComponent<TextManager>();
                    monDialogues[i] = sbTextMan;
                    sbTextMan.setCaller(encounter.enabledEnemies[i].script);
                    Image speechBubImg = speechBub.GetComponent<Image>();
                    SpriteUtil.SwapSpriteFromFile(speechBubImg, encounter.enabledEnemies[i].DialogBubble);
                    Sprite speechBubSpr = speechBubImg.sprite;
                    // TODO improve position setting/remove hardcoding of position setting
                    speechBub.transform.SetParent(encounter.enabledEnemies[i].transform);
                    speechBub.GetComponent<RectTransform>().anchoredPosition = encounter.enabledEnemies[i].DialogBubblePosition;
                    sbTextMan.setOffset(speechBubSpr.border.x, -speechBubSpr.border.w);
                    sbTextMan.setFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_MONSTERTEXT_NAME));
                    sbTextMan.setEffect(new RotatingEffect(sbTextMan));

                    MonsterMessage[] monMsgs = new MonsterMessage[msgs.Length];
                    for (int j = 0; j < monMsgs.Length; j++)
                    {
                        monMsgs[j] = new MonsterMessage(msgs[j]);
                    }

                    sbTextMan.setTextQueue(monMsgs);
                    speechBub.GetComponent<Image>().enabled = true;
                }
                break;

            case UIState.DONE:
                StaticInits.Reset();
                Application.LoadLevel("ModSelect");
                break;
        }
    }

    public void SwitchStateOnString(string state)
    {
        UIState newState = (UIState)Enum.Parse(typeof(UIState), state, true);
        SwitchState(newState);
    }

    private void Awake()
    {
        fightB1 = SpriteRegistry.Get("UI/Buttons/fightbt_1");
        actB1 = SpriteRegistry.Get("UI/Buttons/actbt_1");
        itemB1 = SpriteRegistry.Get("UI/Buttons/itembt_1");
        mercyB1 = SpriteRegistry.Get("UI/Buttons/mercybt_1");

        sndSelect = AudioClipRegistry.GetSound("menumove");
        sndConfirm = AudioClipRegistry.GetSound("menuconfirm");

        arenaParent = GameObject.Find("arena_border_outer");
        canvasParent = GameObject.Find("Canvas");
        uiAudio = GetComponent<AudioSource>();
        uiAudio.clip = sndSelect;

        instance = this;
    }

    private void bindEncounterScriptInteraction()
    {
        encounter.script.Bind("State", (Action<string>)SwitchStateOnString);
        foreach (LuaEnemyController enemy in encounter.enemies)
        {
            enemy.script.Bind("State", (Action<string>)SwitchStateOnString);
        }
        if (encounter.script.GetVar("Update") != null)
        {
            encounterHasUpdate = true;
        }
    }

    private void doNextMonsterDialogue()
    {
        bool singleLineAll = true;
        foreach (TextManager mgr in monDialogues)
        {
            if (mgr == null)
            {
                continue;
            }
            if (mgr.lineCount() > 1)
            { // if there's only one dialogue box for all monsters we can skip it with Z, as is the case with 'regular' undertale encounters
                singleLineAll = false;
                break;
            }
        }

        bool complete = true;
        // TODO adjust logic for multiple enemies
        foreach (TextManager mgr in monDialogues)
        {
            if (mgr == null)
            {
                continue;
            }
            if (singleLineAll && mgr.canSkip()) // just loop and kill all textboxes if pressing Z and everyone has one line at most
            {
                mgr.destroyText();
                continue;
            }

            if (mgr.allLinesComplete())
            {
                mgr.destroyText();
                GameObject.Destroy(mgr.gameObject); // this text manager's game object is a dialog bubble and should be destroyed at this point
                continue;
            }
            else
            {
                complete = false;
            }

            // part that autoskips text if [next] is introduced
            if (mgr.canAutoSkip())
            {
                if (mgr.hasNext())
                {
                    mgr.nextLine();
                }
                else
                {
                    mgr.destroyText();
                    GameObject.Destroy(mgr.gameObject); // code duplication? in my source? it's more likely than you think
                    complete = true;
                }
                continue;
            }

            if (!mgr.allLinesComplete() && mgr.lineComplete())
            {
                mgr.nextLine();
            }
        }

        // looping through the same list three times? there's a reason this class is the most in need of redoing
        // either way, after doing everything required, check which text manager has the longest text now and mute all others
        int longestTextLen = 0;
        int longestTextMgrIndex = -1;
        for (int i = 0; i < monDialogues.Length; i++)
        {
            if (monDialogues[i] == null)
            {
                continue;
            }
            monDialogues[i].setMute(true);
            if (!monDialogues[i].allLinesComplete() && monDialogues[i].letterReferences.Length > longestTextLen)
            {
                longestTextLen = monDialogues[i].letterReferences.Length;
                longestTextMgrIndex = i;
            }
        }

        if (longestTextMgrIndex > -1)
        {
            monDialogues[longestTextMgrIndex].setMute(false);
        }

        if (!complete) // break if we're not done with all text
            return;

        encounter.CallOnSelfOrChildren("EnemyDialogueEnding");
        SwitchState(UIState.DEFENDING);
    }

    private string[] getInventoryPage(int page)
    {
        int invCount = 0;
        for (int i = page * 4; i < page * 4 + 4; i++)
        {
            if (Inventory.container.Count <= i)
                break;

            invCount++;
        }

        if (invCount == 0)
            return null;

        string[] items = new string[6];
        for (int i = 0; i < invCount; i++)
        {
            items[i] = Inventory.container[i + page * 4].ShortName;
        }
        items[5] = "PAGE " + (page + 1);
        return items;
    }

    public UIState getState()
    {
        return this.state;
    }

    private void HandleAction()
    {
        switch (state)
        {
            case UIState.ATTACKING:
                fightUI.StopAction();
                break;

            case UIState.DIALOGRESULT:
                if (!textmgr.lineComplete())
                    break;

                if (!textmgr.allLinesComplete() && textmgr.lineComplete())
                {
                    textmgr.nextLine();
                    break;
                }

                if (textmgr.allLinesComplete())
                {
                    textmgr.destroyText();
                    SwitchState(stateAfterDialogs);
                }
                break;

            case UIState.ACTIONSELECT:
                switch (action)
                {
                    case Actions.FIGHT:
                        SwitchState(UIState.ENEMYSELECT);
                        break;

                    case Actions.ACT:
                        SwitchState(UIState.ENEMYSELECT);
                        break;

                    case Actions.ITEM:
                        if (Inventory.container.Count == 0)
                            return; // prevent sound playback
                        SwitchState(UIState.ITEMMENU);
                        break;

                    case Actions.MERCY:
                        SwitchState(UIState.MERCYMENU);
                        break;
                }
                playSound(sndConfirm);
                break;

            case UIState.ENEMYSELECT:
                switch (action)
                {
                    case Actions.FIGHT:
                        // encounter.enemies[selectedEnemy].HandleAttack(-1);
                        SwitchState(UIState.ATTACKING);
                        break;

                    case Actions.ACT:
                        SwitchState(UIState.ACTMENU);
                        break;
                }
                playSound(sndConfirm);
                break;

            case UIState.ACTMENU:
                textmgr.setCaller(encounter.enabledEnemies[selectedEnemy].script); // probably not necessary due to ActionDialogResult changes
                encounter.enabledEnemies[selectedEnemy].Handle(encounter.enabledEnemies[selectedEnemy].ActCommands[selectedAction]);
                playSound(sndConfirm);
                break;

            case UIState.ITEMMENU:
                encounter.HandleItem(Inventory.container[selectedItem]);
                playSound(sndConfirm);
                break;

            case UIState.MERCYMENU:
                if (selectedMercy == 0)
                {
                    LuaEnemyController[] enabledEnTemp = encounter.enabledEnemies;
                    bool sparedAny = false;
                    foreach (LuaEnemyController enemy in enabledEnTemp)
                    {
                        if (enemy.CanSpare)
                        {
                            enemy.DoSpare();
                            sparedAny = true;
                        }
                    }

                    if (sparedAny)
                    {
                        if (encounter.enabledEnemies.Length == 0)
                        {
                            checkAndTriggerVictory();
                            break;
                        }
                    }

                    encounter.CallOnSelfOrChildren("HandleSpare");
                }
                else if (selectedMercy == 1)
                {
                    PlayerController.instance.GetComponent<Image>().enabled = false;
                    AudioClip yay = AudioClipRegistry.GetSound("runaway");
                    AudioSource.PlayClipAtPoint(yay, Camera.main.transform.position);
                    string fittingLine = "";
                    switch (runawayattempts)
                    {
                        case 0:
                            fittingLine = "...[w:15]But you realized\rthe overworld was missing.";
                            break;

                        case 1:
                            fittingLine = "...[w:15]But the overworld was\rstill missing.";
                            break;

                        case 2:
                            fittingLine = "You walked off as if there\rwere an overworld, but you\rran into an invisible wall.";
                            break;

                        case 3:
                            fittingLine = "...[w:15]On second thought, the\rembarassment just now\rwas too much.";
                            break;

                        case 4:
                            fittingLine = "But you became aware\rof the skeleton inside your\rbody, and forgot to run.";
                            break;

                        case 5:
                            fittingLine = "But you needed a moment\rto forget about your\rscary skeleton.";
                            break;

                        case 6:
                            fittingLine = "...[w:15]You feel as if you\rtried this before.";
                            break;

                        case 7:
                            fittingLine = "...[w:15]Maybe if you keep\rsaying that, the\roverworld will appear.";
                            break;

                        case 8:
                            fittingLine = "...[w:15]Or not.";
                            break;

                        default:
                            fittingLine = "...[w:15]But you decided to\rstay anyway.";
                            break;
                    }

                    ActionDialogResult(new TextMessage[]
                        {
                            new RegularMessage("I'm outta here."),
                            new RegularMessage(fittingLine)
                        },
                        UIState.ENEMYDIALOGUE);
                    Camera.main.GetComponent<AudioSource>().Pause();
                    musicPausedFromRunning = true;
                    runawayattempts++;
                }
                playSound(sndConfirm);
                break;

            case UIState.ENEMYDIALOGUE:
                if (!ArenaSizer.instance.isResizeInProgress())
                {
                    doNextMonsterDialogue();
                }
                break;
        }
    }

    private void HandleArrows()
    {
        bool left = InputUtil.Pressed(GlobalControls.input.Left);
        bool right = InputUtil.Pressed(GlobalControls.input.Right);
        bool up = InputUtil.Pressed(GlobalControls.input.Up);
        bool down = InputUtil.Pressed(GlobalControls.input.Down);

        switch (state)
        {
            case UIState.ACTIONSELECT:
                if (!left && !right)
                    break;

                fightBtn.overrideSprite = null;
                actBtn.overrideSprite = null;
                itemBtn.overrideSprite = null;
                mercyBtn.overrideSprite = null;

                int actionIndex = (int)action;

                if (left)
                    actionIndex--;
                if (right)
                    actionIndex++;
                actionIndex = Math.mod(actionIndex, 4);
                action = (Actions)actionIndex;
                setPlayerOnAction(action);
                playSound(sndSelect);
                break;

            case UIState.ENEMYSELECT:
                if (!up && !down)
                    break;
                if (up)
                    selectedEnemy--;
                if (down)
                    selectedEnemy++;
                selectedEnemy = (selectedEnemy + encounter.enabledEnemies.Length) % encounter.enabledEnemies.Length;
                setPlayerOnSelection(selectedEnemy * 2);
                break;

            case UIState.ACTMENU:
                if (!up && !down && !left && !right)
                    return;

                int xCol = selectedAction % 2; // can just use remainder here, xCol will never be negative at this part
                int yCol = selectedAction / 2;

                if (left)
                    xCol--;
                else if (right)
                    xCol++;
                else if (up)
                    yCol--;
                else if (down)
                    yCol++;

                int actionCount = encounter.enabledEnemies[selectedEnemy].ActCommands.Length;
                int leftColSize = (actionCount + 1) / 2;
                int rightColSize = actionCount / 2;

                if (left || right)
                    xCol = Math.mod(xCol, 2);
                if (up || down)
                    yCol = xCol == 0 ? Math.mod(yCol, leftColSize) : Math.mod(yCol, rightColSize);
                int desiredAction = yCol * 2 + xCol;
                if (desiredAction >= 0 && desiredAction < actionCount)
                {
                    selectedAction = desiredAction;
                    setPlayerOnSelection(selectedAction);
                }
                break;

            case UIState.ITEMMENU:
                if (!up && !down && !left && !right)
                    return;

                int xColI = Math.mod(selectedItem, 2);
                int yColI = Math.mod(selectedItem, 4) / 2;

                if (left)
                    xColI--;
                else if (right)
                    xColI++;
                else if (up)
                    yColI--;
                else if (down)
                    yColI++;

                // Debug.Log("xCol after controls " + xColI);
                // Debug.Log("yCol after controls " + yColI);

                int itemCount = 4; // HACK: should do item count based on page number...
                int leftColSizeI = (itemCount + 1) / 2;
                int rightColSizeI = itemCount / 2;
                int desiredItem = (selectedItem / 4) * 4;
                if (xColI == -1)
                {
                    xColI = 1;
                    desiredItem -= 4;
                }
                else if (xColI == 2)
                {
                    xColI = 0;
                    desiredItem += 4;
                }

                if (up || down)
                    yColI = xColI == 0 ? Math.mod(yColI, leftColSizeI) : Math.mod(yColI, rightColSizeI);
                desiredItem += (yColI * 2 + xColI);

                // Debug.Log("xCol after evaluation " + xColI);
                // Debug.Log("yCol after evaluation " + yColI);

                // Debug.Log("Unchecked desired item " + desiredItem);

                if (desiredItem < 0)
                {
                    desiredItem = Math.mod(desiredItem, 4) + (Inventory.container.Count / 4) * 4;
                }
                else if (desiredItem > Inventory.container.Count)
                {
                    desiredItem = Math.mod(desiredItem, 4);
                }

                if (desiredItem != selectedItem && desiredItem < Inventory.container.Count) // 0 check not needed, done before
                {
                    selectedItem = desiredItem;
                    setPlayerOnSelection(Math.mod(selectedItem, 4));
                    int page = selectedItem / 4;
                    textmgr.setText(new SelectMessage(getInventoryPage(page), false));
                }

                // Debug.Log("Desired item index after evaluation " + desiredItem);
                break;

            case UIState.MERCYMENU:
                if (!up && !down)
                {
                    break;
                }
                if (up)
                {
                    selectedMercy--;
                }
                if (down)
                {
                    selectedMercy++;
                }
                if (encounter.CanRun)
                {
                    selectedMercy = Math.mod(selectedMercy, 2);
                }
                else
                {
                    selectedMercy = 0;
                }
                setPlayerOnSelection(selectedMercy * 2);
                break;
        }
    }

    private void HandleCancel()
    {
        switch (state)
        {
            case UIState.ACTIONSELECT:
                if (textmgr.canSkip() && !textmgr.lineComplete())
                {
                    textmgr.skipText();
                }
                break;

            case UIState.DIALOGRESULT: // same as actionselect, but not going to put it under the same case
                if (textmgr.canSkip() && !textmgr.lineComplete())
                {
                    textmgr.skipText();
                }
                break;

            case UIState.ENEMYDIALOGUE:
                bool singleLineAll = true;
                bool cannotSkip = false;
                // why two booleans for the same result? 'cause they're different conditions
                foreach (TextManager mgr in monDialogues)
                {
                    if (!mgr.canSkip())
                    {
                        cannotSkip = true;
                    }

                    if (mgr.lineCount() > 1)
                    {
                        singleLineAll = false;
                    }
                }

                if (cannotSkip || singleLineAll)
                    break;

                foreach (TextManager mgr in monDialogues)
                {
                    mgr.skipText();
                }
                break;

            case UIState.ENEMYSELECT:
                SwitchState(UIState.ACTIONSELECT);
                break;

            case UIState.ACTMENU:
                SwitchState(UIState.ENEMYSELECT);
                break;

            case UIState.ITEMMENU:
                SwitchState(UIState.ACTIONSELECT);
                break;

            case UIState.MERCYMENU:
                SwitchState(UIState.ACTIONSELECT);
                break;
        }
    }

    private void playSound(AudioClip clip)
    {
        if (!uiAudio.clip.Equals(clip))
            uiAudio.clip = clip;
        uiAudio.Play();
    }

    public static void playSoundSeparate(AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.95f);
    }

    private void setPlayerOnAction(Actions action)
    {
        switch (action)
        {
            case Actions.FIGHT:
                fightBtn.overrideSprite = fightB1;
                PlayerController.instance.SetPosition(48, 25, true);
                break;

            case Actions.ACT:
                actBtn.overrideSprite = actB1;
                PlayerController.instance.SetPosition(202, 25, true);
                break;

            case Actions.ITEM:
                itemBtn.overrideSprite = itemB1;
                PlayerController.instance.SetPosition(361, 25, true);
                break;

            case Actions.MERCY:
                mercyBtn.overrideSprite = mercyB1;
                PlayerController.instance.SetPosition(515, 25, true);
                break;
        }
    }

    // visualisation:
    // 0    1
    // 2    3
    // 4    5
    private void setPlayerOnSelection(int selection)
    {
        int xMv = selection % 2; // remainder safe again, selection is never negative
        int yMv = selection / 2;
        // HACK: remove hardcoding of this sometime, ever... probably not happening lmao
        PlayerController.instance.SetPosition(upperLeft.x + xMv * 256, upperLeft.y - yMv * textmgr.Charset.LineSpacing, true);
    }

    // Use this for initialization
    private void Start()
    {
        textmgr = GameObject.Find("TextManager").GetComponent<TextManager>();
        textmgr.setEffect(new TwitchEffect(textmgr));
        encounter = FindObjectOfType<LuaEnemyEncounter>();

        fightBtn = GameObject.Find("FightBt").GetComponent<Image>();
        fightBtn.sprite = SpriteRegistry.Get("UI/Buttons/fightbt_0");
        actBtn = GameObject.Find("ActBt").GetComponent<Image>();
        actBtn.sprite = SpriteRegistry.Get("UI/Buttons/actbt_0");
        itemBtn = GameObject.Find("ItemBt").GetComponent<Image>();
        itemBtn.sprite = SpriteRegistry.Get("UI/Buttons/itembt_0");
        mercyBtn = GameObject.Find("MercyBt").GetComponent<Image>();
        mercyBtn.sprite = SpriteRegistry.Get("UI/Buttons/mercybt_0");

        ArenaSizer.instance.ResizeImmediate(ArenaSizer.UIWidth, ArenaSizer.UIHeight);
        PlayerController.instance.setControlOverride(true);
        fightUI = GameObject.Find("FightUI").GetComponent<FightUIController>();
        fightUI.gameObject.SetActive(false);

        bindEncounterScriptInteraction();
        encounter.CallOnSelfOrChildren("EncounterStarting");
        if (state == UIState.NONE)
        {
            SwitchState(UIState.ACTIONSELECT);
        }
    }

    public void checkAndTriggerVictory()
    {
        if (encounter.enabledEnemies.Length > 0)
        {
            return;
        }
        Camera.main.GetComponent<AudioSource>().Stop();
        ActionDialogResult(new RegularMessage("YOU WON!\nYou earned 0 XP and 0 gold."), UIState.DONE);
        victory = true;
    }

    // Update is called once per frame
    private void Update()
    {
        if (encounterHasUpdate)
        {
            encounter.TryCall("Update");
        }

        if (textmgr.isPaused() && !ArenaSizer.instance.isResizeInProgress())
        {
            textmgr.setPause(false);
        }

        if (state == UIState.ENEMYDIALOGUE)
        {
            foreach (TextManager mgr in monDialogues)
            {
                if (mgr.canAutoSkip())
                {
                    doNextMonsterDialogue();
                }
            }
        }

        if (state == UIState.DEFENDING)
        {
            if (!encounter.waveInProgress())
            {
                foreach (LuaProjectile p in FindObjectsOfType<LuaProjectile>())
                    BulletPool.instance.Requeue(p);
                SwitchState(UIState.ACTIONSELECT);
            }
            else
            {
                encounter.updateWave();
            }
            return;
        }

        if (InputUtil.Pressed(GlobalControls.input.Confirm))
        {
            HandleAction();
        }
        else if (InputUtil.Pressed(GlobalControls.input.Cancel))
        {
            HandleCancel();
        }
        else
        {
            HandleArrows();
        }

        if (state == UIState.ATTACKING)
        {
            if (!fightUI.Finished())
            {
                return;
            }
            int hp = encounter.enabledEnemies[selectedEnemy].HP;
            if (hp == 0)
            {
                // fightUI.disableImmediate();
                // victory = true;
                if (!encounter.enabledEnemies[selectedEnemy].TryCall("OnDeath"))
                {
                    encounter.enabledEnemies[selectedEnemy].DoKill();

                    if (encounter.enabledEnemies.Length > 0)
                    {
                        SwitchState(UIState.ENEMYDIALOGUE);
                    }
                    else
                    {
                        checkAndTriggerVictory();
                    }
                }
            }
            else
            {
                SwitchState(UIState.ENEMYDIALOGUE);
            }
        }
    }
}