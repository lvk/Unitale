using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public static class SpriteRegistry
{
    private static Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
    public static Image GENERIC_SPRITE_PREFAB;
    public static Sprite EMPTY_SPRITE;

    public static Sprite Get(string key)
    {
        key = key.ToLower();
        if (dict.ContainsKey(key))
        {
            return dict[key];
        }
        return null;
    }

    public static void Set(string key, Sprite value)
    {
        dict[key.ToLower()] = value;
    }

    public static void init()
    {
        dict.Clear();
        GENERIC_SPRITE_PREFAB = Resources.Load<Image>("Prefabs/generic_sprite");
        EMPTY_SPRITE = Sprite.Create(new Texture2D(1, 1), new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        EMPTY_SPRITE.name = "blank";
        string modPath = FileLoader.pathToModFile("Sprites");
        string defaultPath = FileLoader.pathToDefaultFile("Sprites");
        loadAllFrom(modPath);
        loadAllFrom(defaultPath);
    }

    private static void loadAllFrom(string directoryPath)
    {
        DirectoryInfo dInfo = new DirectoryInfo(directoryPath);
        FileInfo[] fInfo = dInfo.GetFiles("*.png", SearchOption.AllDirectories);
        foreach (FileInfo file in fInfo)
        {
            string imageName = FileLoader.getRelativePathWithoutExtension(directoryPath, file.FullName).ToLower();
            if (dict.ContainsKey(imageName))
            {
                continue;
            }
            dict.Add(imageName, SpriteUtil.fromFile(file.FullName));
        }
    }
}