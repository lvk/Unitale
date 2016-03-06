using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

public static class SpriteFontRegistry
{
    public const string UI_DEFAULT_NAME = "uidialog";
    public const string UI_DAMAGETEXT_NAME = "uidamagetext";
    public const string UI_MONSTERTEXT_NAME = "monster";
    public const string UI_SMALLTEXT_NAME = "uibattlesmall";

    public static GameObject LETTER_OBJECT;
    public static GameObject BUBBLE_OBJECT;

    private static Dictionary<string, UnderFont> dict = new Dictionary<string, UnderFont>();
    private static bool initialized;

    public static void init()
    {
        if (initialized)
            return;
        LETTER_OBJECT = Resources.Load("Fonts/letter") as GameObject;
        BUBBLE_OBJECT = Resources.Load("Prefabs/DialogBubble") as GameObject;

        string modPath = FileLoader.pathToModFile("Sprites/UI/Fonts");
        string defaultPath = FileLoader.pathToDefaultFile("Sprites/UI/Fonts");
        loadAllFrom(modPath);
        loadAllFrom(defaultPath);

        initialized = true;
    }

    private static void loadAllFrom(string directoryPath)
    {
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        if (!dInfo.Exists)
        {
            return;
        }
        FileInfo[] fInfo = dInfo.GetFiles("*.png", SearchOption.TopDirectoryOnly);
        foreach (FileInfo file in fInfo)
        {
            string fontName = Path.GetFileNameWithoutExtension(file.FullName);
            if (dict.ContainsKey(fontName.ToLower()))
            {
                continue;
            }
            UnderFont underfont = getUnderFont(fontName);
            if (underfont == null)
            {
                continue;
            }
            dict[fontName.ToLower()] = underfont;
        }
    }

    public static UnderFont Get(string fontName)
    {
        fontName = fontName.ToLower();
        if(!dict.ContainsKey(fontName)){
            return null;
        }
        return dict[fontName];
    }

    private static UnderFont getUnderFont(string fontName)
    {
        XmlDocument xml = new XmlDocument();
        string fontPath = FileLoader.requireFile("Sprites/UI/Fonts/" + fontName + ".png");
        string xmlPath = FileLoader.requireFile("Sprites/UI/Fonts/" + fontName + ".xml", false);
        if (xmlPath == null)
        {
            return null;
        }
        xml.Load(xmlPath);
        Dictionary<char, Sprite> fontMap = loadBuiltinFont(xml["font"]["spritesheet"], fontPath);
        AudioClip defaultVoice = null;
        if (xml["font"]["voice"] != null)
        {
            defaultVoice = AudioClipRegistry.GetVoice(xml["font"]["voice"].InnerText);
        }

        UnderFont underfont = new UnderFont(fontMap, defaultVoice);

        if (xml["font"]["linespacing"] != null)
        {
            underfont.LineSpacing = ParseUtil.getFloat(xml["font"]["linespacing"].InnerText);
        }

        if (xml["font"]["color"] != null)
        {
            underfont.DefaultColor = ParseUtil.getColor(xml["font"]["color"].InnerText);
        }

        return underfont;
    }

    private static Dictionary<char, Sprite> loadBuiltinFont(XmlNode sheetNode, string fontPath)
    {
        Sprite[] letterSprites = SpriteUtil.atlasFromXml(sheetNode, SpriteUtil.fromFile(fontPath));
        Dictionary<char, Sprite> letters = new Dictionary<char, Sprite>();
        foreach (Sprite s in letterSprites)
        {
            string name = s.name;
            if (name.Length == 1)
            {
                letters.Add(name[0], s);
                continue;
            }
            else
            {
                switch (name)
                {
                    case "slash":
                        letters.Add('/', s);
                        break;

                    case "dot":
                        letters.Add('.', s);
                        break;

                    case "pipe":
                        letters.Add('|', s);
                        break;

                    case "backslash":
                        letters.Add('\\', s);
                        break;

                    case "colon":
                        letters.Add(':', s);
                        break;

                    case "questionmark":
                        letters.Add('?', s);
                        break;

                    case "doublequote":
                        letters.Add('"', s);
                        break;

                    case "asterisk":
                        letters.Add('*', s);
                        break;

                    case "space":
                        letters.Add(' ', s);
                        break;

                    case "lt":
                        letters.Add('<', s);
                        break;

                    case "rt":
                        letters.Add('>', s);
                        break;

                    case "ampersand":
                        letters.Add('&', s);
                        break;
                }
            }
        }
        return letters;
    }
}