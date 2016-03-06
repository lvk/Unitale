using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Initiates the death sequence. Used in the Game Over scene to make sure the player doesn't go looking for objects before the Game Over scene has loaded.
/// </summary>
public class GameOverInit : MonoBehaviour {
	void Start () {
        Camera.main.GetComponent<AudioSource>().clip = AudioClipRegistry.GetMusic("mus_gameover");
        GameObject.Find("GameOver").GetComponent<Image>().sprite = SpriteRegistry.Get("UI/spr_gameoverbg_0");
        GameObject.FindObjectOfType<GameOverBehavior>().StartDeath();
	}
}
