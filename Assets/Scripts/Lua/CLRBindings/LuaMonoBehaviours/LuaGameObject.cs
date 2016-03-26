using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LuaGameObject {

    private GameObject g;
    private LuaTransform t;

    private List<LuaMonoBehaviour> behaviours = new List<LuaMonoBehaviour>();

    public LuaGameObject(GameObject set) {
        g=set;
        t=new LuaTransform(g.transform);

        foreach (MonoBehaviour m in set.GetComponents<MonoBehaviour>())
            behaviours.Add(new LuaMonoBehaviour(m));
    }

    public LuaTransform transform {
        get {
            return t;
        }
    }

    //TODO - convert into a dictionary/table for lua users
    public LuaMonoBehaviour[] components {
        get {
            return behaviours.ToArray();
        }
    }

    //Object creation/destruction
    public LuaGameObject NewObject(string name) {
        GameObject newObj = new GameObject();

        newObj.name=name;
        return new LuaGameObject(newObj);
    }

    public LuaGameObject DuplicateObject(LuaGameObject dupe) {
        GameObject newObject = GameObject.Instantiate(dupe.g);

        return new LuaGameObject(newObject);
    }

    public void Destroy() {
        GameObject.Destroy(g);
    }

    public LuaGameObject GetTemplate(int index) {
        return DuplicateObject(templatesLua[index]);
    }

    public static LuaGameObject[] templatesLua;
}
