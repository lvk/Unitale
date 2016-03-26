using UnityEngine;
using System.Collections;

public class LuaMonoBehaviour {

    private MonoBehaviour myComponent;
    private LuaTransform _myTransform;

    public LuaMonoBehaviour(MonoBehaviour set) {
        myComponent=set;
        _myTransform=new LuaTransform(set.transform);
    }

    public string name {
        get {
            return myComponent.name;
        }
    }

    public LuaTransform transform {
        get {
            return _myTransform;
        }
    }

    public bool enabled {
        get {
            return myComponent.enabled;
        }
        set {
            myComponent.enabled=value;
        }
    }

}
