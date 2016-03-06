using UnityEngine;
using UnityEngine.UI;

public class FightUIController : MonoBehaviour
{
    public RectTransform targetRt;
    //private Animator line;
    //private Animator slice;
    private LuaSpriteController line;
    private LuaSpriteController slice;
    private LifeBarController lifeBar;
    private float borderX;
    private float xSpeed = -450.0f;
    private bool stopped = false;
    private bool shakeInProgress = false;
    private bool finishingFade = false;
    private int[] shakeX = new int[] { 12, -12, 7, -7, 3, -3, 1, -1, 0 };
    private EnemyController enemy;
    private TextManager damageText;
    private RectTransform damageTextRt;
    private int shakeIndex = -1;
    private float shakeTimer = 0.0f;
    private float totalShakeTime = 1.5f;
    private Vector2 enePos;
    private string[] lineAnim = new string[] { "UI/Battle/spr_targetchoice_0", "UI/Battle/spr_targetchoice_1" };
    private string[] sliceAnim = new string[] { 
        "UI/Battle/spr_slice_o_0",
        "UI/Battle/spr_slice_o_1",
        "UI/Battle/spr_slice_o_2",
        "UI/Battle/spr_slice_o_3",
        "UI/Battle/spr_slice_o_4",
        "UI/Battle/spr_slice_o_5"
    };

    public int Damage { get; private set; } // retrieve after finishing to get real damage number

    private void Awake()
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.name == "FightUILine")
            {
                line = new LuaSpriteController(child.GetComponent<Image>());
            }
            else if (child.name == "SliceAnim")
            {
                slice = new LuaSpriteController(child.GetComponent<Image>());
            }
            else if (child.name == "DamageNumber")
            {
                damageText = child.GetComponent<TextManager>();
            }

            else if (child.name == "HPBar")
            {
                lifeBar = child.GetComponent<LifeBarController>();
            }
        }

        damageTextRt = damageText.GetComponent<RectTransform>();
    }

    private void Start()
    {
        damageText.setFont(SpriteFontRegistry.Get(SpriteFontRegistry.UI_DAMAGETEXT_NAME));
        damageText.setMute(true);
        line.Set("UI/Battle/spr_targetchoice_0");
    }

    public void Init(EnemyController target)
    {
        gameObject.SetActive(true);
        line.StopAnimation();
        line.img.gameObject.SetActive(true);
        line.img.enabled = true;
        lifeBar.setVisible(false);
        finishingFade = false;
        enemy = target;
        enePos = enemy.GetComponent<RectTransform>().position;
        targetRt.anchoredPosition = new Vector2(GetComponent<RectTransform>().rect.width/2, 0);
        Vector3 slicePos = target.GetComponent<RectTransform>().position;
        slicePos.Set(slicePos.x, slicePos.y + target.GetComponent<RectTransform>().rect.height / 2 - 55, slicePos.z); // lol hardcoded offsets
        slice.img.GetComponent<RectTransform>().position = slicePos;
        borderX = -GetComponent<RectTransform>().rect.width / 2;
        stopped = false;
        shakeInProgress = false;
        shakeTimer = 0;
        // damageTextRt.position = target.GetComponent<RectTransform>().position;
        setAlpha(1.0f);
    }

    public void setAlpha(float a)
    {
        Color c = Color.white;
        c.a = a;
        GetComponent<Image>().color = c;
    }

    public void StopAction()
    {
        if (stopped)
            return;
        stopped = true;
        Damage = getDamage();
        line.SetAnimation(lineAnim, 1 / 12f);
        slice.SetAnimation(sliceAnim, 1 / 6f);
        slice.loop = KeyframeCollection.LoopMode.ONESHOT;
        UIController.playSoundSeparate(AudioClipRegistry.GetSound("slice"));
    }

    private int getDamage()
    {
        float atkMult = getAtkMult();
        if (atkMult < 0)
            return -1;
        int damage = (int)Mathf.Round(((PlayerCharacter.WeaponATK + PlayerCharacter.ATK - enemy.Defense) + UnityEngine.Random.value * 2) * atkMult);
        if (damage < 0)
            return 0;
        return damage;
    }

    public float getAtkMult()
    {
        if (stopped)
        {
            if (Mathf.Abs(targetRt.anchoredPosition.x) <= 12)
                return 2.2f;
            else
            {
                float mult = 2.0f - 2.0f * Mathf.Abs(targetRt.anchoredPosition.x * 2.0f / GetComponent<RectTransform>().rect.width);
                if (mult < 0)
                    mult = 0;
                return mult;
            }
        }
        else
        {
            return -1.0f;
        }
    }

    public bool Finished()
    {
        if (shakeTimer > 0)
            return shakeTimer >= totalShakeTime;
        return targetRt.anchoredPosition.x < borderX;
    }

    public void initFade()
    {
        if (!finishingFade)
        {
            finishingFade = true;
            line.img.enabled = false;
            line.StopAnimation();
            slice.StopAnimation();
            // Arena resizes to a small default size in most regular battles before entering actual defense state
        }
    }

    public void disableImmediate()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    private void Update()
    {
        if (finishingFade)
        {
            float resizeProg = 1.0f - ArenaSizer.instance.getProgress();
            setAlpha(resizeProg);
            if (resizeProg == 0.0f)
            {
                damageText.destroyText();
                gameObject.SetActive(false);
            }
            return;
        }

        if (shakeInProgress)
        {
            int shakeidx = (int)Mathf.Floor(shakeTimer * shakeX.Length / totalShakeTime);

            if (Damage > 0 && shakeIndex != shakeidx)
            {
                shakeIndex = shakeidx;
                if (shakeIndex >= shakeX.Length)
                    shakeIndex = shakeX.Length - 1;
                Vector2 localEnePos = enemy.GetComponent<RectTransform>().anchoredPosition; // get local position to do the shake
                enemy.GetComponent<RectTransform>().anchoredPosition = new Vector2(localEnePos.x + shakeX[shakeIndex], localEnePos.y);
            }

            damageTextRt.position = new Vector2(damageTextRt.position.x, 80 + enePos.y + 40 * Mathf.Sin(shakeTimer * Mathf.PI * 0.75f));

            shakeTimer += Time.deltaTime;
            if (shakeTimer >= totalShakeTime)
            {
                shakeInProgress = false;
                initFade();
            }
            return;
        }

        if (!shakeInProgress && slice.animcomplete)
        {
            slice.StopAnimation();
            if (Damage > 0)
            {
                AudioSource aSrc = GetComponent<AudioSource>();
                aSrc.clip = AudioClipRegistry.GetSound("hitsound");
                aSrc.Play();
            }
            // set damage numbers and positioning
            string damageTextStr = "";
            if (Damage <= 0)
            {
                damageTextStr = "[color:c0c0c0]MISS";
            }
            else
            {
                damageTextStr = "[color:ff0000]" + Damage;
            }
            // the -14 is to compensate for the 14 characters that [color:rrggbb] is worth until commands no longer count for text length. soon
            int damageTextWidth = (damageTextStr.Length - 14) * 29 + (damageTextStr.Length - 1 - 14) * 3; // lol hardcoded offsets
            foreach (char c in damageTextStr)
                if (c == '1')
                    damageTextWidth -= 12; // lol hardcoded offsets
            damageTextRt.position = new Vector2(enePos.x - 0.5f * damageTextWidth, 80 + enePos.y);
            damageText.setText(new TextMessage(damageTextStr, false, true));

            // initiate lifebar and set lerp to its new health value
            if (Damage > 0)
            {
                int newHP = enemy.HP - Damage;
                if (newHP < 0)
                    newHP = 0;
                lifeBar.GetComponent<RectTransform>().position = new Vector2(enePos.x, enePos.y + 20);
                lifeBar.GetComponent<RectTransform>().sizeDelta = new Vector2(enemy.GetComponent<RectTransform>().rect.width, 13);
                lifeBar.setInstant((float)enemy.HP / (float)enemy.getMaxHP());
                lifeBar.setLerp((float)enemy.HP / (float)enemy.getMaxHP(), (float)newHP / (float)enemy.getMaxHP());
                lifeBar.setVisible(true);
                enemy.doDamage(Damage);
            }

            // finally, damage enemy and call its attack handler in case you wanna stop music on death or something
            shakeInProgress = true;
            enemy.HandleAttack(Damage);
        }

        if (stopped)
            return;
        float mv = xSpeed * Time.deltaTime;
        targetRt.anchoredPosition = new Vector2(targetRt.anchoredPosition.x + mv, 0);
        if (Finished()) // you didn't press Z or you wouldn't be here
        {
            enemy.HandleAttack(-1);
            StationaryMissScript smc = Resources.Load<StationaryMissScript>("Prefabs/StationaryMiss");
            smc = Instantiate(smc);
            smc.transform.SetParent(GameObject.Find("Canvas").transform);
            smc.setXPosition(320 - enePos.x);
            initFade();
        }
    }
}