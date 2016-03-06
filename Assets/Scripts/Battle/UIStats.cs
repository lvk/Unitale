using UnityEngine;
using UnityEngine.UI;

public class UIStats : MonoBehaviour
{
    public static UIStats instance;

    private GameObject nameLevelTextManParent;
    private TextManager nameLevelTextMan;
    private GameObject hpTextManParent;
    private TextManager hpTextMan;
    private LifeBarController lifebar;
    private RectTransform lifebarRt;
    

    private int hpCurrent = PlayerCharacter.MaxHP;
    private int hpMax = PlayerCharacter.MaxHP;
    private int lvCurrent = PlayerCharacter.LV;
    private string nameCurrent = PlayerCharacter.Name;

    private bool initialized = false;

    void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        GameObject statsObj = GameObject.Find("Stats");
        lifebar = statsObj.GetComponentInChildren<LifeBarController>();
        lifebarRt = lifebar.GetComponent<RectTransform>();

        nameLevelTextManParent = GameObject.Find("NameLv");
        hpTextManParent = GameObject.Find("HPTextParent");

        nameLevelTextMan = nameLevelTextManParent.AddComponent<TextManager>();
        hpTextMan = hpTextManParent.AddComponent<TextManager>();

        hpTextMan.setFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME));
        initialized = true;
        setMaxHP();
        setHP(hpCurrent);
        setPlayerInfo(nameCurrent, lvCurrent);
    }

    public void setPlayerInfo(string name, int lv)
    {
        nameCurrent = name;
        lvCurrent = lv;
        if (initialized)
        {
            nameLevelTextMan.enabled = true;
            nameLevelTextMan.setFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_SMALLTEXT_NAME));
            nameLevelTextMan.setText(new TextMessage(name.ToUpper() + "  LV " + lv, false, true));
            nameLevelTextMan.enabled = false;
        }
    }

    public void setHP(int newHP)
    {
        hpCurrent = newHP;
        if (initialized)
        {
            float hpFrac = (float)hpCurrent / (float)hpMax;
            lifebar.setInstant(hpFrac);
            string sHpCurrent = hpCurrent < 10 ? "0" + hpCurrent : "" + hpCurrent;
            string sHpMax = hpMax < 10 ? "0" + hpMax : "" + hpMax;
            hpTextMan.setText(new TextMessage(sHpCurrent + " / " + sHpMax, false, true));
        }
    }

    public void setMaxHP()
    {
        if (initialized)
        {
            lifebarRt.sizeDelta = new Vector2(PlayerCharacter.MaxHP * 1.2f, lifebarRt.sizeDelta.y);
            hpMax = PlayerCharacter.MaxHP;
            setHP(hpCurrent);
        }
    }
}