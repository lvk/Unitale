using UnityEngine;
using System.Collections;
using System;

public class OverworldUIController : MonoBehaviour {

    [SerializeField]
    GameObject Dialog;
    [SerializeField]
    TextManager DialogManager;

    public static OverworldUIController main;

    void Awake() {
        main=this;

        DialogManager.setOffset(15,175);
    }

    void LateUpdate() {
        if (Dialog.activeSelf) {
            if (InputUtil.Pressed(GlobalControls.input.Confirm)&&DialogManager.lineComplete()) {
                if (DialogManager.hasNext())
                    DialogManager.nextLine();
                else if (LinesCompleted)
                    CloseDialog();
            }
        }
    }

    internal void SetDialog(string[] lines) {
        if (Dialog.activeSelf)
            return;
        OpenDialog();

        TextMessage[] linesTxt = new TextMessage[lines.Length];

        for (int i = 0; i<lines.Length; i++)
            linesTxt[i]=new TextMessage(lines[i],true,false);

        DialogManager.setTextQueue(linesTxt);
    }

    public void SetDialog(string text) {
        if (Dialog.activeSelf)
            return;
        OpenDialog();
        DialogManager.setText(new TextMessage(text,true,false));
    }

    public void OpenDialog() {
        Dialog.SetActive(true);
        OverworldPlayerController.main.enabled=false;
        DialogManager.setText(new TextMessage("", false, true));
    }

    public bool LinesCompleted {
        get {
            return DialogManager.allLinesComplete();
        }
    }

    public void CloseDialog() {
        OverworldPlayerController.main.enabled=true;
        Dialog.SetActive(false);
    }

}
