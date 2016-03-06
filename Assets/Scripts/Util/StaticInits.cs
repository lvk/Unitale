using System.Diagnostics;
using MoonSharp.Interpreter;
using UnityEngine;

public class StaticInits : MonoBehaviour
{
    public static string MODFOLDER;
    public static string ENCOUNTER;
    public string EDITOR_MODFOLDER;
    public string EDITOR_ENCOUNTER;

    public static bool Initialized { get; private set; }

    private void Awake()
    {
        if (MODFOLDER == null || MODFOLDER == "")
            MODFOLDER = EDITOR_MODFOLDER;
        if (ENCOUNTER == null || ENCOUNTER == "")
            ENCOUNTER = EDITOR_ENCOUNTER;
        initAll();
        Initialized = true;
    }

    public void initAll()
    {
        if (!Initialized)
        {
            Stopwatch sw = new Stopwatch(); //benchmarking terrible loading times
            sw.Start();
            ScriptRegistry.init();
            sw.Stop();
            UnityEngine.Debug.Log("Script registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            sw.Start();
            SpriteRegistry.init();
            sw.Stop();
            UnityEngine.Debug.Log("Sprite registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            sw.Start();
            AudioClipRegistry.init();
            sw.Stop();
            UnityEngine.Debug.Log("Audio clip registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();

            sw.Start();
            SpriteFontRegistry.init();
            sw.Stop();
            UnityEngine.Debug.Log("Sprite font registry loading time: " + sw.ElapsedMilliseconds + "ms");
            sw.Reset();
        }
        LateUpdater.init(); // must be last; lateupdater's initialization is for classes that depend on the above registries
        MusicManager.src = Camera.main.GetComponent<AudioSource>();
    }

    public static void Reset()
    {
        Initialized = false;
        LuaScriptBinder.Clear();
        PlayerCharacter.Reset();
    }
}