using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SpriteSheet {

    public string name;
    public Sprite[] sprites;

    public SpriteSheet(Sprite[] _sprites, string _name) {
        name=_name;
        sprites=_sprites;
    }

}
