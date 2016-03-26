using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TiledSharp;

public class OverworldCharacterController : MonoBehaviour {

    //---Script managment
    public CharacterSpriteController spriteController;
    public string characterName;

    //Used to add scripts and the like to the character.
    public void UpdateToObject(TmxObjectGroup.TmxObject obj) {

        foreach (KeyValuePair<string,string> kvp in obj.Properties) {
            string key = kvp.Key;
            string value = kvp.Value;

            if (key=="tag")
                gameObject.tag=value;
        }
    }

    //------Monobehaviours------

    public virtual void Update() {

        if(pursue && state==1)
            MovementFrame();
    }

    //------Movement------

    Vector2 target;
    bool pursue;

    //State of movement. 0 is none, 1 is moving to point, 2 is reached point.
    public int state;

    //The movement speed, in units per second
    public float moveSpeed = 3f;

    public void MoveToPoint(Vector2 point) {
        state=1;
        target=point;
        pursue=true;
    }

    public virtual void OnReachedTarget() {
        state=2;
        pursue=false;
    }

    void MovementFrame() {
        state=1;

        Vector2 move = (target-(Vector2)transform.position).normalized;

        transform.position+=new Vector3(move.x,move.y,0)*moveSpeed*Time.deltaTime;

        if (Vector2.Distance(target, (Vector2)transform.position)<=0.25f) {
            OnReachedTarget();
        }
    }



}
