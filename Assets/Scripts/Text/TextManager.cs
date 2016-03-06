using System;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

// TODO less code duplicate-y way of pulling commands out of the text.
public class TextManager : MonoBehaviour
{
    internal Image[] letterReferences;
    internal Vector2[] letterPositions;

    private UnderFont default_charset = null;
    private AudioSource letterSound;
    private TextMessage[] textQueue;
    private TextEffect textEffect;
    private int currentLine = 0;
    private int currentCharacter = 0;
    private bool displayImmediate = false;
    private bool currentSkippable = true;
    private RectTransform self;
    private Vector2 offset;
    private bool offsetSet = false;
    private float currentX;
    private float currentY;
    private bool paused = false;
    private bool muted = false;
    private bool autoSkip = false;
    internal int hSpacing = 3;

    private Color currentColor = Color.white;
    private Color defaultColor = Color.white;

    private float letterTimer = 0.0f;
    private float timePerLetter;
    private float singleFrameTiming = 1.0f / 30;

    private ScriptWrapper caller;

    public UnderFont Charset { get; private set; }

    public void setCaller(ScriptWrapper s)
    {
        caller = s;
    }

    public void setFont(UnderFont font)
    {
        Charset = font;
        if (default_charset == null)
            default_charset = font;
        letterSound.clip = Charset.Sound;
        currentColor = Charset.DefaultColor;
    }

    public void setHorizontalSpacing(int spacing = 3)
    {
        this.hSpacing = spacing;
    }

    public void resetFont()
    {
        if (Charset == null || default_charset == null)
        {
            setFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DEFAULT_NAME));
        }
        Charset = default_charset;
        letterSound.clip = default_charset.Sound;
    }

    private void Awake()
    {
        self = gameObject.GetComponent<RectTransform>();
        letterSound = gameObject.AddComponent<AudioSource>();
        letterSound.playOnAwake = false;
        // setFont(SpriteFontRegistry.F_UI_DIALOGFONT);
        timePerLetter = singleFrameTiming;
    }

    private void Start()
    {
        // setText("the quick brown fox jumps over\rthe lazy dog.\nTHE QUICK BROWN FOX JUMPS OVER\rTHE LAZY DOG.\nJerry.", true, true);
        // setText(new TextMessage("Here comes Napstablook.", true, false));
        // setText(new TextMessage(new string[] { "Check", "Compliment", "Ignore", "Steal", "trow temy", "Jerry" }, false));
    }

    public void setPause(bool pause)
    {
        this.paused = pause;
    }

    public bool isPaused()
    {
        return this.paused;
    }

    public void setMute(bool muted)
    {
        this.muted = muted;
    }

    public void setText(TextMessage text)
    {
        setTextQueue(new TextMessage[] { text });
    }

    public void setTextQueue(TextMessage[] textQueue)
    {
        resetFont();
        this.textQueue = textQueue;
        currentLine = 0;
        showLine(0);
    }

    public bool canSkip()
    {
        return currentSkippable;
    }

    public bool canAutoSkip()
    {
        return autoSkip;
    }

    public int lineCount()
    {
        return textQueue.Length;
    }

    public void setOffset(float xOff, float yOff)
    {
        offset = new Vector2(xOff, yOff);
        offsetSet = true;
    }

    public bool lineComplete()
    {
        return displayImmediate || currentCharacter == letterReferences.Length;
    }

    public bool allLinesComplete()
    {
        return currentLine == textQueue.Length - 1 && lineComplete();
    }

    public void showLine(int line)
    {
        if (!offsetSet)
            setOffset(0, 0);
        currentColor = Charset.DefaultColor;
        currentSkippable = true;
        autoSkip = false;
        letterSound.clip = Charset.Sound;
        timePerLetter = singleFrameTiming;
        letterTimer = 0.0f;
        destroyText();
        currentX = self.position.x + offset.x;
        currentY = self.position.y + offset.y - Charset.LineSpacing;
        currentCharacter = 0;
        this.displayImmediate = textQueue[line].ShowImmediate;
        spawnText();
    }

    public bool hasNext()
    {
        return currentLine + 1 < lineCount();
    }

    public void nextLine()
    {
        showLine(++currentLine);
    }

    public void skipText()
    {
        while (currentCharacter < letterReferences.Length)
        {
            if (letterReferences[currentCharacter] != null && Charset.Letters.ContainsKey(textQueue[currentLine].Text[currentCharacter]))
            {
                letterReferences[currentCharacter].enabled = true;
            }
            currentCharacter++;
        }
    }

    public void setEffect(TextEffect effect)
    {
        this.textEffect = effect;
    }

    public void destroyText()
    {
        foreach (Transform child in gameObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void spawnText()
    {
        string currentText = textQueue[currentLine].Text;
        letterReferences = new Image[currentText.Length];
        letterPositions = new Vector2[currentText.Length];
        for (int i = 0; i < currentText.Length; i++)
        {
            if (currentText[i] == '[')
            {
                string command = parseCommandInline(currentText, ref i);
                preCreateControlCommand(command);
                continue;
            }

            if (currentText[i] == '\n')
            {
                currentX = self.position.x + offset.x;
                currentY -= Charset.LineSpacing;
            }
            else if (currentText[i] == '\t')
            {
                currentX = 356; // HACK: bad tab usage
            }

            if (!Charset.Letters.ContainsKey(currentText[i]))
                continue;

            GameObject singleLtr = Instantiate(SpriteFontRegistry.LETTER_OBJECT);
            RectTransform ltrRect = singleLtr.GetComponent<RectTransform>();
            Image ltrImg = singleLtr.GetComponent<Image>();

            ltrRect.SetParent(gameObject.transform);

            ltrImg.sprite = Charset.Letters[currentText[i]];

            letterReferences[i] = ltrImg;

            if (Charset.Letters.ContainsKey(currentText[i]))
            {
                ltrRect.position = new Vector3((int)currentX, (int)(currentY + Charset.Letters[currentText[i]].border.w - Charset.Letters[currentText[i]].border.y), 0);
            }
            else
            {
                ltrRect.position = new Vector3((int)currentX, (int)currentY, 0);
            }
            letterPositions[i] = ltrRect.anchoredPosition;
            ltrImg.SetNativeSize();
            ltrImg.color = currentColor;
            ltrImg.enabled = displayImmediate;

            currentX += ltrRect.rect.width + hSpacing; // TODO remove hardcoded letter offset
        }
    }

    private void Update()
    {
        if (textQueue == null || textQueue.Length == 0)
            return;
        if (paused)
            return;

        if (textEffect != null)
            textEffect.updateEffects();

        if (displayImmediate)
            return;
        if (currentCharacter >= letterReferences.Length)
            return;

        letterTimer += Time.deltaTime;

        if (letterTimer > timePerLetter)
        {
            if (currentCharacter < letterReferences.Length)
            {
                while (textQueue[currentLine].Text[currentCharacter] == '[')
                {
                    string command = parseCommandInline(textQueue[currentLine].Text, ref currentCharacter);
                    currentCharacter++; // we're not in a continuable loop so move to the character after the ] manually

                    float lastLetterTimer = letterTimer; // kind of a dirty hack so we can at least release 0.2.0 sigh
                    float lastTimePerLetter = timePerLetter; // i am so sorry
                    inUpdateControlCommand(command);
                    if (lastLetterTimer != letterTimer || lastTimePerLetter != timePerLetter)
                    {
                        return;
                    }
                    if (currentCharacter >= textQueue[currentLine].Text.Length)
                    {
                        return;
                    }
                }
                if (letterReferences[currentCharacter] != null)
                {
                    letterReferences[currentCharacter].enabled = true;
                    if (letterSound != null && !muted)
                        letterSound.Play();
                }
                currentCharacter++;
            }
            letterTimer = 0.0f;
        }
    }

    private string parseCommandInline(string input, ref int currentChar)
    {
        currentChar++; // skip past found bracket
        checkCharInBounds(currentChar, input.Length);
        string control = "";
        while (input[currentChar] != ']')
        {
            control += input[currentChar];
            currentChar++;
            checkCharInBounds(currentChar, input.Length);
        }
        return control;
    }

    private void checkCharInBounds(int i, int length)
    {
        if (i >= length)
        {
            throw new InvalidOperationException("Went out of bounds looking for arguments after control character.");
        }
    }

    private void preCreateControlCommand(string command)
    {
        string[] cmds = command.Split(':');
        string[] args = new string[0];
        if (cmds.Length == 2)
        {
            args = cmds[1].Split(',');
            cmds[1] = args[0];
        }
        switch (cmds[0].ToLower())
        {
            case "noskip":
                currentSkippable = false;
                break;

            case "instant":
                this.displayImmediate = true;
                break;

            case "color":
                currentColor = ParseUtil.getColor(cmds[1]);
                break;

            case "starcolor":
                Color starColor = ParseUtil.getColor(cmds[1]);
                if (textQueue[currentLine].Text[0] == '*')
                    letterReferences[0].color = starColor;
                if (textQueue[currentLine] is SelectMessage)
                {
                    int indexOfStar = textQueue[currentLine].Text.IndexOf('*'); // HACK oh my god lol
                    if (indexOfStar > -1)
                        letterReferences[indexOfStar].color = starColor;
                }
                break;

            case "font":
                AudioClip oldClip = letterSound.clip;
                setFont(SpriteFontRegistry.Get(cmds[1]));
                letterSound.clip = oldClip;
                break;

            case "effect":
                switch (cmds[1].ToUpper())
                {
                    case "NONE":
                        textEffect = null;
                        break;

                    case "TWITCH":
                        if (args.Length > 1)
                        {
                            textEffect = new TwitchEffect(this, ParseUtil.getFloat(args[1]));
                        }
                        else
                        {
                            textEffect = new TwitchEffect(this);
                        }
                        break;

                    case "SHAKE":
                        if (args.Length > 1)
                        {
                            textEffect = new ShakeEffect(this, ParseUtil.getFloat(args[1]));
                        }
                        else
                        {
                            textEffect = new ShakeEffect(this);
                        }
                        break;

                    case "ROTATE":
                        if (args.Length > 1)
                        {
                            textEffect = new RotatingEffect(this, ParseUtil.getFloat(args[1]));
                        }
                        else
                        {
                            textEffect = new RotatingEffect(this);
                        }
                        break;
                }
                break;
        }
    }

    private void inUpdateControlCommand(string command)
    {
        string[] cmds = command.Split(':');
        string[] args = new string[0];
        if (cmds.Length == 2)
        {
            args = cmds[1].Split(',');
            cmds[1] = args[0];
        }
        switch (cmds[0].ToLower())
        {
            case "w":
                letterTimer = timePerLetter - (singleFrameTiming * ParseUtil.getInt(cmds[1]));
                break;

            case "waitall":
                timePerLetter = singleFrameTiming * ParseUtil.getInt(cmds[1]);
                break;

            case "voice":
                if (cmds[1].ToLower() == "default")
                    letterSound.clip = SpriteFontRegistry.Get(SpriteFontRegistry.UI_DEFAULT_NAME).Sound;
                else
                    letterSound.clip = AudioClipRegistry.GetVoice(cmds[1].ToLower());
                break;

            case "font":
                letterSound.clip = SpriteFontRegistry.Get(cmds[1].ToLower()).Sound;
                break;

            case "novoice":
                letterSound.clip = null;
                break;

            case "next":
                autoSkip = true;
                break;

            case "func":
                if (caller == null)
                    UnitaleUtil.displayLuaError("???", "Func called but no script to reference. This is the engine's fault, not yours.");
                if (args.Length > 1)
                    caller.Call(args[0], DynValue.NewString(args[1]));
                else
                    caller.Call(cmds[1]);
                break;
        }
    }
}