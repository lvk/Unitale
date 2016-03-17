using System.Collections.Generic;
using System.IO;

public class ScriptRegistry
{
    internal static string WAVE_PREFIX = "wave_";
    internal static string ENCOUNTER_PREFIX = "enc_";
    internal static string MONSTER_PREFIX = "mon_";
    internal static string CHARACTER_PREFIX = "char_";
    internal static string OBJECT_PREFIX = "obj_";
    internal static string LEVEL_PREFIX = "lvl_";
    internal static string NOPREFIX = "";
    private static Dictionary<string, string> dict = new Dictionary<string, string>();

    private static string[] folders = new string[] { "Waves", "Encounters", "Monsters"};
    private static string[] prefixes = new string[] { WAVE_PREFIX, ENCOUNTER_PREFIX, MONSTER_PREFIX };
    private static string[] foldersOv = new string[] { "Characters", "Objects", "Levels", "" };
    private static string[] prefixesOv = new string[] { CHARACTER_PREFIX, OBJECT_PREFIX, LEVEL_PREFIX, NOPREFIX };

    public static string Get(string key)
    {
        key = key.ToLower();
        if (dict.ContainsKey(key))
            return dict[key];
        return null;
    }

    public static void Set(string key, string value)
    {
        dict[key.ToLower()] = value;
    }

    public static void init()
    {
        dict.Clear();

        if (!StaticInits.isOverworld) {
            for (int i = 0; i<folders.Length; i++) {
                string modPath = FileLoader.pathToModFile("Lua/"+folders[i]);
                //string defaultPath = FileLoader.pathToDefaultFile("Lua/" + folders[i]);
                loadAllFrom(modPath, prefixes[i]);
                //loadAllFrom(defaultPath, prefixes[i]);
            }
        } else {
            for (int i = 0; i<foldersOv.Length; i++) {
                string modPath = FileLoader.pathToModFile("Lua/"+foldersOv[i]);
                //string defaultPath = FileLoader.pathToDefaultFile("Lua/" + folders[i]);
                loadAllFrom(modPath, prefixesOv[i]);
                //loadAllFrom(defaultPath, prefixes[i]);
            }
        }
    }

    private static void loadAllFrom(string directoryPath, string script_prefix)
    {
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        FileInfo[] fInfo = dInfo.GetFiles("*.lua", SearchOption.AllDirectories);
        foreach (FileInfo file in fInfo)
        {
            string scriptName = FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower();
            if (dict.ContainsKey(script_prefix + scriptName))
            {
                continue;
            }
            Set(script_prefix + scriptName, FileLoader.getTextFrom(file.FullName));
        }
    }
}