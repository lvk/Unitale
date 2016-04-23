using UnityEngine;
using System.Collections;

public class OverworldCameraController : MonoBehaviour {

    public static Transform target;
	
	// Update is called once per frame
	void Update () {
        if (target!=null)
            transform.position=new Vector3(target.position.x,target.position.y,transform.position.z);
	}
}
