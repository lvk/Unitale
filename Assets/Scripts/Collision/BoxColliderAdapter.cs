using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BoxColliderAdapter : MonoBehaviour {

    //-------------------References-------------------

    Image img;
    SpriteRenderer spr;
    BoxCollider2D bc;

    public bool isHalf;

    //-------------------Variables-------------------

    Vector2 lastSize = new Vector2();

    //-------------------Dynamic Variables-------------------

    //-------------------Monobehaviours-------------------

	// Use this for initialization
	void Start () {
        img=GetComponent<Image>();
        spr=GetComponent<SpriteRenderer>();
        bc=GetComponent<BoxCollider2D>();
        UpdateColliderSize();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if (img!=null) {
            if (lastSize!=img.sprite.rect.size)
                UpdateColliderSize();
        } else if (spr!=null) {
            if (spr.sprite!=null)
                if (lastSize!=spr.sprite.rect.size)
                    UpdateColliderSize();
        }
	}

    //-------------------Functions-------------------

    void UpdateColliderSize() {
        if(img!=null)
            bc.size=img.sprite.rect.size;
        if (spr!=null) {
            if (spr.sprite!=null) {
                if (!isHalf) {
                    bc.size=spr.sprite.rect.size/spr.sprite.pixelsPerUnit;
                } else {
                    bc.size=new Vector2(spr.sprite.rect.width,spr.sprite.rect.height/2)/spr.sprite.pixelsPerUnit;
                    bc.offset=(new Vector2(0, -spr.sprite.rect.height/2)/spr.sprite.pixelsPerUnit)+new Vector2(0,bc.size.y/2);
                }
            }
        }
        lastSize=bc.size;
    }
}
