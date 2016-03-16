using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TiledSharp;

public class OverworldCharacterController : MonoBehaviour {

    public CharacterSpriteController spriteController;
    public string characterName;

    //Used to add scripts and the like to the character.
    public void UpdateToObject(TmxObjectGroup.TmxObject obj) {
        foreach(KeyValuePair<string,string> kvp in obj.Properties) {
            string key = kvp.Key;
            string value = kvp.Value;
        }
    }
}
