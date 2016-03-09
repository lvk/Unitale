using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BoxColliderAdapter : MonoBehaviour {

    //-------------------References-------------------

    Image img;
    BoxCollider2D bc;

    //-------------------Variables-------------------

    Vector2 lastSize = new Vector2();

    //-------------------Dynamic Variables-------------------

    //-------------------Monobehaviours-------------------

	// Use this for initialization
	void Start () {
        img=GetComponent<Image>();
        bc=GetComponent<BoxCollider2D>();
        UpdateColliderSize();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (lastSize!=img.sprite.rect.size)
            UpdateColliderSize();
	}

    //-------------------Functions-------------------

    void UpdateColliderSize() {
        bc.size=img.sprite.rect.size;
        lastSize=bc.size;
    }
}
