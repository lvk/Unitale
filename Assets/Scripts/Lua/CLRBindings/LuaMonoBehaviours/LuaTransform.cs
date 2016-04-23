using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LuaTransform {

    private Transform t;
    private Dictionary<string,LuaTransform> _children = new Dictionary<string, LuaTransform>();

    public LuaTransform(Transform set) {
        t=set;

        for (int i = 0; i<set.childCount; i++)
            _children.Add(set.GetChild(i).name,new LuaTransform(set.GetChild(i)));
    }

    public float x {
        get {
            return t.position.y;
        }
        set {
            t.position=new Vector3(value, t.position.y, t.position.z);
        }
    }

    public float y {
        get {
            return t.position.x;
        }
        set {
            t.position=new Vector3(t.position.x,value,t.position.z);
        }
    }

    public float rotation {
        get {
            return t.eulerAngles.z;
        }
        set {
            t.rotation=Quaternion.Euler(0,0,value);
        }
    }

    public LuaTransform[] children {
        get {
            return new List<LuaTransform>(_children.Values).ToArray();
        }
    }

    public LuaTransform FindChild(string s) {
        return new LuaTransform(t.FindChild(s));
    }

}
