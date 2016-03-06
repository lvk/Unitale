using UnityEngine;

public class TwitchEffect : TextEffect
{
    private int prevChar = 0;
    private int updateCount = 0;
    private float intensity;
    private int minWigFrames = 34;
    private int wigFrameVariety = 30;
    private int nextWigInFrames = 24;

    public TwitchEffect(TextManager textMan, float intensity = 2.0f)
        : base(textMan)
    {
        this.intensity = intensity;
    }

    protected override void updateInternal()
    {
        // move back last character
        if (prevChar >= 0 && textMan.letterReferences.Length > prevChar && textMan.letterReferences[prevChar] != null)
            textMan.letterReferences[prevChar].GetComponent<RectTransform>().anchoredPosition = textMan.letterPositions[prevChar];
        prevChar = -1;

        // don't make it happen too often
        updateCount++;
        if (updateCount < nextWigInFrames)
            return;
        updateCount = 0;

        int selectedChar = UnityEngine.Random.Range(0, textMan.letterReferences.Length);
        if (textMan.letterReferences[selectedChar] == null)
            return;
        float random = UnityEngine.Random.value * 2.0f * Mathf.PI;
        float xWig = Mathf.Sin(random) * intensity;
        float yWig = Mathf.Cos(random) * intensity;
        nextWigInFrames = minWigFrames + (int)(wigFrameVariety * UnityEngine.Random.value);
        RectTransform rt = textMan.letterReferences[selectedChar].GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(textMan.letterPositions[selectedChar].x + xWig, textMan.letterPositions[selectedChar].y + yWig);
        prevChar = selectedChar;
    }
}