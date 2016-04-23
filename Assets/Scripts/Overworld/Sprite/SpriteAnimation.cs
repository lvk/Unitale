using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

[System.Serializable]
public class SpriteAnimation {

    string sheetKey;

    public Sprite[] spr;

    public float AnimSpeed;
    public bool isAnimating=true;
    public string name;

    float _time;
    int _spriteIndex;

    internal Sprite GetSprite() {
        return spr[_spriteIndex];
    }

    internal void Progress() {
        if (!isAnimating)
            return;

        _time+=Time.deltaTime;

        if (_time>AnimSpeed) {
            _spriteIndex++;
            _spriteIndex=(int)Mathf.Repeat(_spriteIndex,spr.Length);

            _time=0;
        }
    }

    internal void SetSprites(Sprite[] _spr) {
        spr=_spr;
    }

    public void ResetProgress() {
        _time=0;
        _spriteIndex=0;
    }
}
