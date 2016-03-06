using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class AudioClipRegistry
{
    private static Dictionary<string, AudioClip> dict = new Dictionary<string, AudioClip>();
    private static string[] extensions = new string[] { ".ogg", ".wav" }; // Note: also requires support from FileLoader.getAudioClip().

    public static AudioClip Get(string key)
    {
        key = key.ToLower();
        if (dict.ContainsKey(key))
            return dict[key];
        return null;
    }

    public static AudioClip GetVoice(string key)
    {
        key = "Sounds/Voices/" + key;
        return Get(key);
    }

    public static AudioClip GetSound(string key)
    {
        key = "Sounds/" + key;
        return Get(key);
    }

    public static AudioClip GetMusic(string key)
    {
        key = "Audio/" + key;
        return Get(key);
    }

    public static void Set(string key, AudioClip value)
    {
        dict[key.ToLower()] = value;
    }

    public static void init()
    {
        dict.Clear();
        string modPath = FileLoader.pathToModFile(""); // get root so we can get sounds from all subdirectories
        string defaultPath = FileLoader.pathToDefaultFile(""); // selection between Audio/Sounds folder is done where it makes sense
        loadAllFrom(modPath);
        loadAllFrom(defaultPath);
    }

    private static void loadAllFrom(string directoryPath)
    {
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        FileInfo[] fInfo = dInfo.GetFiles("*.*", SearchOption.AllDirectories).Where(file => extensions.Contains(file.Extension)).ToArray();
        foreach (FileInfo file in fInfo)
        {
            string voiceName = FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower();
            if (dict.ContainsKey(voiceName))
            {
                continue;
            }
            Set(voiceName, FileLoader.getAudioClip(directoryPath, file.FullName));
        }
    }
}