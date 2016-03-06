using UnityEngine;

/// <summary>
/// Lua binding to manipulate in-game music and play sounds.
/// </summary>
public class MusicManager
{
    internal static AudioSource src;

    public static float playtime
    {
        get
        {
            return src.time;
        }
    }

    public static float totaltime
    {
        get
        {
            return src.clip.length;
        }
    }

    public static void LoadFile(string name)
    {
        src.Stop();
        src.clip = AudioClipRegistry.GetMusic(name);
        src.Play();
    }

    public static void PlaySound(string name)
    {
        AudioSource.PlayClipAtPoint(AudioClipRegistry.GetSound(name), Camera.main.transform.position, 0.65f);
    }

    public static void Pitch(double value)
    {
        if (value < -3)
            value = -3;
        if (value > 3)
            value = 3;
        src.pitch = (float)value;
    }

    public static void Volume(double value)
    {
        if (value < 0)
            value = 0;
        if (value > 1)
            value = 1;
        src.volume = (float)value;
    }

    public static void Play()
    {
        src.Play();
    }

    public static void Stop()
    {
        src.Stop();
    }

    public static void Pause()
    {
        src.Pause();
    }

    public static void Unpause()
    {
        src.UnPause();
    }
}