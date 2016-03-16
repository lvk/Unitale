using UnityEngine;
using System.Collections;

public class SpriteDepthSorting : MonoBehaviour {

    Renderer spr;

    public int offset = 0;

    void Start() {
        spr=GetComponent<Renderer>();
    }
	
	// Update is called once per frame
	void Update () {
        spr.sortingOrder=15-((int)transform.position.y+offset);
        Debug.Log(transform.position.y);
	}
}
