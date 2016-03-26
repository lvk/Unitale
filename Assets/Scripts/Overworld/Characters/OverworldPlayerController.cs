using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OverworldPlayerController : MonoBehaviour {

    Vector2 input;
    Rigidbody2D rb;

    OverworldCharacterController cont;

    public float playerSpeed = 1;

    public static OverworldPlayerController main;

    public void Start() {
        main=this;
        rb=GetComponent<Rigidbody2D>();
        cont=GetComponent<OverworldCharacterController>();
        OverworldCameraController.target=transform;

        animationDirections.Add(Vector2.down, "Down");
        animationDirections.Add(Vector2.up, "Up");
        animationDirections.Add(Vector2.left, "Left");
        animationDirections.Add(Vector2.right, "Right");
    }

    Dictionary<Vector2, string> animationDirections = new Dictionary<Vector2, string>();

    Vector2 lastVec = Vector2.down;

    public void Update() {
        input=new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        input=Vector2.ClampMagnitude(input,1);

        if (input.magnitude<=0.25f) {
            cont.spriteController.SetCurrentAnimation(cont.spriteController.idleAnimation+animationDirections[lastVec]);
        } else {
            float lastAngle = Mathf.Infinity;
            foreach(KeyValuePair<Vector2,string> kvp in animationDirections) {
                float currAngle = Vector2.Angle(input, kvp.Key);
                if (currAngle<=lastAngle) {
                    lastAngle=currAngle;
                    lastVec=kvp.Key;
                }
            }

            cont.spriteController.SetCurrentAnimation(cont.spriteController.walkAnimation+animationDirections[lastVec]);
        }
    }

    public void FixedUpdate () {
        rb.velocity=(input*3)*playerSpeed;
	}
}
