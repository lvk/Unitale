using UnityEngine;
using System.Collections;

/// <summary>
/// Controls that should be active on all screens. Pretty much a hack to allow people to reset.
/// </summary>
public class GlobalControls : MonoBehaviour {
    public static UndertaleInput input = new KeyboardInput();
    public static LuaInputBinding luaInput = new LuaInputBinding(input);
    /// <summary>
    /// Control checking.
    /// </summary>
	void Update () {
	    if (Input.GetKeyDown(KeyCode.F4))
        {
            Screen.fullScreen = !Screen.fullScreen;
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            UserDebugger.instance.gameObject.SetActive(!UserDebugger.instance.gameObject.activeSelf);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.LoadLevel("ModSelect");
            StaticInits.Reset();
        }
	}
}
