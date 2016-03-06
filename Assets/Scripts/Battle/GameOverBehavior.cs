using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// The fairly hacky and somewhat unmaintainable Game Over behaviour class. Written in a hurry as it probably wasn't going to get replaced anytime soon.
/// This script is attached to the Player object to make it persist on scene switch, and immediately switches to the Game Over scene upon attachment.
/// There, the GameOverInit behaviour takes care of calling StartDeath() on this behaviour.
/// </summary>
public class GameOverBehavior : MonoBehaviour {
    private GameObject brokenHeartPrefab;
    private GameObject heartShardPrefab;
    private string[] heartShardAnim = new string[] { "UI/Battle/heartshard_0", "UI/Battle/heartshard_1", "UI/Battle/heartshard_2", "UI/Battle/heartshard_3" };
    private TextManager gameOverTxt;
    private Image gameOverImage;
    private RectTransform[] heartShardInstances = new RectTransform[0];
    private Vector2[] heartShardRelocs;
    private LuaSpriteController[] heartShardCtrl;

    private AudioClip heartbreak;
    private AudioClip heartsplode;
    private AudioSource gameOverMusic;

    private float breakHeartAfter = 1.0f;
    private float explodeHeartAfter = 2.5f;
    private float gameOverAfter = 4.5f;
    private float fluffybunsAfter = 6.5f;
    private float internalTimer = 0.0f;
    private float gameOverFadeTimer = 0.0f;
    private bool started = false;
    private bool done = false;
    private bool exiting = false;

    private Vector2 heartPos;
    private Color heartColor;

    public void StartDeath()
    {
        brokenHeartPrefab = Resources.Load<GameObject>("Prefabs/heart_broken");
        heartShardPrefab = SpriteRegistry.GENERIC_SPRITE_PREFAB.gameObject;
        gameOverTxt = GameObject.Find("TextParent").GetComponent<TextManager>();
        heartbreak = AudioClipRegistry.GetSound("heartbeatbreaker");
        heartsplode = AudioClipRegistry.GetSound("heartsplosion");
        gameOverImage = GameObject.Find("GameOver").GetComponent<Image>();
        heartPos = gameObject.GetComponent<RectTransform>().position;
        heartColor = gameObject.GetComponent<Image>().color;
        gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
        gameOverMusic = Camera.main.GetComponent<AudioSource>();
        started = true;
    }

    void Awake()
    {
        Application.LoadLevel("GameOver");
        this.gameObject.GetComponent<Image>().enabled = true; // abort the blink animation if it was playing
    }

	// Update is called once per frame
	void Update () {
        if (!started)
        {
            return;
        }

        if (internalTimer > breakHeartAfter)
        {
            AudioSource.PlayClipAtPoint(heartbreak, Camera.main.transform.position, 0.75f);
            brokenHeartPrefab = Instantiate(brokenHeartPrefab);
            brokenHeartPrefab.transform.SetParent(this.gameObject.transform);
            brokenHeartPrefab.GetComponent<RectTransform>().position = heartPos;
            brokenHeartPrefab.GetComponent<Image>().color = heartColor;
            gameObject.GetComponent<Image>().enabled = false;
            breakHeartAfter = 999.0f;
        }

        if (internalTimer > explodeHeartAfter)
        {
            AudioSource.PlayClipAtPoint(heartsplode, Camera.main.transform.position, 0.75f);
            brokenHeartPrefab.GetComponent<Image>().enabled = false;
            heartShardInstances = new RectTransform[6];
            heartShardRelocs = new Vector2[6];
            heartShardCtrl = new LuaSpriteController[6];
            for (int i = 0; i < heartShardInstances.Length; i++)
            {
                heartShardInstances[i] = Instantiate(heartShardPrefab).GetComponent<RectTransform>();
                heartShardCtrl[i] = new LuaSpriteController(heartShardInstances[i].GetComponent<Image>());
                heartShardInstances[i].transform.SetParent(this.gameObject.transform);
                heartShardInstances[i].GetComponent<RectTransform>().position = heartPos;
                heartShardInstances[i].GetComponent<Image>().color = heartColor;
                heartShardRelocs[i] = UnityEngine.Random.insideUnitCircle * 100.0f;
                heartShardCtrl[i].Set(heartShardAnim[0]);
                heartShardCtrl[i].SetAnimation(heartShardAnim, 1 / 5f);
            }
            explodeHeartAfter = 999.0f;
        }

        if (internalTimer > gameOverAfter)
        {
            gameOverMusic.Play();
            gameOverAfter = 999.0f;
        }

        if (internalTimer > fluffybunsAfter)
        {
            gameOverTxt.setHorizontalSpacing(7);
            gameOverTxt.setTextQueue(new TextMessage[]{
                new TextMessage("", false, false), // initial blank message to force pressing Z
                new TextMessage("[color:ffffff][voice:v_fluffybuns][waitall:2]You cannot give\nup just yet...", false, false),
                new TextMessage("[color:ffffff][voice:v_fluffybuns][waitall:2]" + PlayerCharacter.Name + "!\n[w:15]Stay determined...", false, false),
                new TextMessage("", false, false), // ending with a double blank message, because the text manager is considered complete
                new TextMessage("", false, false) // when you're on the last line, and the last line is done writing out too - we fade at this point
            });
            fluffybunsAfter = 999.0f;
        }

        for (int i = 0; i < heartShardInstances.Length; i++)
        {
            heartShardInstances[i].position += (Vector3)heartShardRelocs[i]*Time.deltaTime;
            heartShardRelocs[i].y -= 100f * Time.deltaTime;
        }

        if (!done)
        {
            gameOverImage.color = new Color(1, 1, 1, gameOverFadeTimer);
            if (gameOverAfter >= 999.0f && gameOverFadeTimer < 1.0f)
            {
                gameOverFadeTimer += Time.deltaTime / 2;
                if (gameOverFadeTimer >= 1.0f)
                {
                    gameOverFadeTimer = 1.0f;
                    done = true;
                }
            }
            internalTimer += Time.deltaTime; // this is actually dangerous because done can be true before everything's done if timers are modified
        }
        else if (!exiting && !gameOverTxt.allLinesComplete())
        {
            // Note: [noskip] only affects the UI controller's ability to skip, so we have to redo that here.
            if (InputUtil.Pressed(GlobalControls.input.Confirm) && gameOverTxt.lineComplete())
            {
                gameOverTxt.nextLine();
            }
        }
        else if (!exiting && gameOverTxt.allLinesComplete())
        {
            exiting = true;
            gameOverFadeTimer = 1.0f;
        }
        else if (exiting && gameOverFadeTimer > 0.0f)
        {
            gameOverImage.color = new Color(1, 1, 1, gameOverFadeTimer);
            if (gameOverFadeTimer > 0.0f)
            {
                gameOverFadeTimer -= Time.deltaTime / 2;
                if (gameOverFadeTimer <= 0.0f)
                {
                    gameOverFadeTimer = 0.0f;
                }
            }
        }
        else if (exiting) 
        {
            // repurposing the timer as a reset delay
            gameOverFadeTimer -= Time.deltaTime;
            if (gameOverMusic.volume - Time.deltaTime > 0.0f)
            {
                gameOverMusic.volume -= Time.deltaTime;
            } else {
                gameOverMusic.volume = 0.0f;
            }

            if (gameOverFadeTimer < -1.5f)
            {
                StaticInits.Reset();
                Destroy(this.gameObject);
                Application.LoadLevel("ModSelect");
            }
        }
	}
}
