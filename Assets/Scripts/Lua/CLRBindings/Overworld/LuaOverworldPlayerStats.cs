using UnityEngine;
using System.Collections;

public class LuaOverworldPlayerStats : MonoBehaviour {

    LuaOverworldPlayer player;

    public LuaOverworldPlayerStats(LuaOverworldPlayer p) {
        player=p;
    }

    public bool controllable {
        get {
            return player.enabled;
        }
        set {
            player.enabled=value;
        }
    }

}
