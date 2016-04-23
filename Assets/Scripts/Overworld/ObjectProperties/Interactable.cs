using UnityEngine;
using System.Collections;

public class Interactable : MonoBehaviour {

    [SerializeField]
    bool inRange=false;

    void Start() {
        BoxCollider2D trigger = gameObject.AddComponent<BoxCollider2D>();

        trigger.isTrigger=true;
    }

    void Update() {
        if (inRange) {
            if (InputUtil.Pressed(GlobalControls.input.Confirm)) {
                gameObject.SendMessage("OnInteract",SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.transform.parent.tag=="Player")
            inRange=true;
    }
    void OnTriggerExit2D(Collider2D col) {
        if (col.transform.parent.tag=="Player")
            inRange=false;
    }

}
