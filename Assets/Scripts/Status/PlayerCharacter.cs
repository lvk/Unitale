using UnityEngine;
using System.Collections;

public static class PlayerCharacter {
    public static int LV = 1;
    public static int HP = 20;
    public static int MaxHP = 20;
    public static string Name = "Unity";
    public static int WeaponATK = 0; // not relevant, no equipment yet
    public static int ATK = 10; // internally, ATK is what Undertale's menu shows + 10
    public static int DEF = 0; // unused

    private static string[] names = new string[]{
        "Chara",
        "Jerry",
        "Dog",
        "MTT",
        "Papyru",
        "Bones",
        "Almond",
        "Ninten",
        "Lucas",
        "Claus",
        "Maxim",
        "Saturn"
    };

    static PlayerCharacter()
    {
        Reset();
    }

    public static void Reset()
    {
        Name = names[Math.randomRange(0, names.Length)];
        SetLevel(1);
    }

    public static void SetLevel(int level)
    {
        if (level < 1 || level > 20)
        {
            return;
        }
        MaxHP = 16 + 4 * level;
        ATK = 8 + 2 * level;
        LV = level;

        if (LV == 20)
        {
            MaxHP = 99;
        }
    }
}
