using System.Collections.Generic;
using UnityEngine;

public class UnderFont
{
    public UnderFont(Dictionary<char, Sprite> letters, AudioClip sound)
    {
        Letters = letters;
        Sound = sound;
        LineSpacing = Letters[' '].rect.height * 1.5f;
        DefaultColor = Color.white;
    }

    public Dictionary<char, Sprite> Letters { get; private set; }
    public AudioClip Sound { get; private set; }
    public Color DefaultColor { get; set; }
    public float LineSpacing { get; set; }
}