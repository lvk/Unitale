using UnityEngine;
using System.Collections;
using System.IO;

public class DialogProvider : MonoBehaviour {

    string[] lines;

	// Use this for initialization
	void Start () {
        lines=File.ReadAllLines(FileLoader.ModDataPath+"/Lua/Objects/"+gameObject.name+"/"+gameObject.name+"_Text.txt");
	}

    void OnInteract() {
        OverworldUIController.main.SetDialog(lines);
    }
}
