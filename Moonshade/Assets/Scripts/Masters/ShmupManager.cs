using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShmupManager : MonoBehaviour
{
    public static ShmupManager shmupManager;

    GameMasterScript gameMaster;

    int freezeDuration = -1;

    public bool captureMode;
    public uint captures;
    public uint neededCaptures;
    public bool captureSuccess;

    GameObject[] captureIcons;
    public Sprite captureIconOff;
    public Sprite captureIconLit;

    public float patternTimeLeft;
    public float enemyHpChange;
    public bool spellCap;

    public bool spellcardDisplayed;

    public bool endSTG;
    public bool inSTG;
    public bool startingSTG;
    [Space]
    [SerializeField] GameObject shmupCharSpawn;
    [Space]
    public Vector2 spawnPos;

    public Vector2 bulletDestroyX = new Vector2(-226f, 226f);
    public Vector2 bulletDestroyY = new Vector2(-30, 500);

    public Vector2 playerXExtents = new Vector2(-155, 155);
    public Vector2 playerYExtents = new Vector2(18, 382);

    public int skinWidth = 5;
    public LayerMask collisionMask;

    public LayerMask bulletDestroyLayer;

    public Transform bulletContainer;
    public Transform enemyContainer;

    public Transform shmupCam;
    
    public Transform playerTransform;

    public Sprite normalBorder;
    public Sprite captureBorder;

    public Image border;
    public Image blackScreen;

    [Space]

    public AudioClip captureGet;

    public Animator spellcardDeclareAnim;
    public TMPro.TextMeshProUGUI spellcardText;
    public RectTransform spellcardNameBox;
    public Image spellcardPortrait;

    public TMPro.TextMeshProUGUI patternTimer;
    
    bool prevStartSTG;

    GameObject guageContainer;
    SpriteMask[] healthbar;
    SpriteMask[] manaBar;

    private void Awake()
    {
        shmupManager = this;
        shmupCam = GameObject.Find("ShmupCamera").transform;
        blackScreen = shmupCam.GetChild(0).GetChild(0).Find("BlackScreen").GetComponent<Image>();
        border = shmupCam.GetChild(0).GetChild(0).Find("Border").GetComponent<Image>();
        spellcardDeclareAnim = border.transform.parent.Find("SpellcardDisplay").GetComponent<Animator>();

        guageContainer = border.transform.Find("Guages").gameObject;

        healthbar = guageContainer.transform.GetChild(0).GetComponentsInChildren<SpriteMask>();
        manaBar = guageContainer.transform.GetChild(1).GetComponentsInChildren<SpriteMask>();

        shmupCam.gameObject.SetActive(false);
        GameObject containerFound = GameObject.Find("ShmupBulletContainer");
        if (containerFound != null)
            bulletContainer = containerFound.transform;
        else
        {
            bulletContainer = new GameObject("ShmupBulletContainer").transform;
            bulletContainer.parent = transform;
            bulletContainer.position = transform.position;
        }

        if (bulletContainer.GetComponent<SlowDelete>() == null)
            bulletContainer.gameObject.AddComponent<SlowDelete>();

        containerFound = GameObject.Find("ShmupEnemyContainer");
        if (containerFound != null)
            enemyContainer = containerFound.transform;
        else
        {
            enemyContainer = new GameObject("ShmupEnemyContainer").transform;
            enemyContainer.parent = transform;
            enemyContainer.position = transform.position;
        }

        Transform captureIconContainer = border.transform.Find("CaptureIcons");
        captureIcons = new GameObject[captureIconContainer.childCount];

        for (int i = 0; i < captureIcons.Length; i++)
        {
            captureIcons[i] = captureIconContainer.GetChild(i).gameObject;
        }

        spellcardNameBox = spellcardDeclareAnim.transform.GetChild(0).GetComponent<RectTransform>();
        spellcardText = spellcardDeclareAnim.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        spellcardPortrait = spellcardDeclareAnim.transform.GetChild(2).GetComponent<Image>();
        patternTimer = spellcardDeclareAnim.transform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameMaster = GameMasterScript.gameMaster;
        ExitCaptureMode();
    }

    // Update is called once per frame
    void Update()
    {
        if(inSTG)
        {
            patternTimer.text = Mathf.Clamp(Mathf.FloorToInt(patternTimeLeft / 60f), 0, 99).ToString("00");

            if (patternTimeLeft <= 0)
            {
                endSTG = true;
                spellCap = false;
            }

            if(freezeDuration > 0)
            {
                gameMaster.timeScale = 0;
                Time.timeScale = 0;
                freezeDuration--;
            }
            else if(freezeDuration == 0)
            {
                gameMaster.timeScale = 1;
                Time.timeScale = 1;
                freezeDuration--;
            }
        }

        if(endSTG && spellCap)
        {
            spellCap = false;
            enemyHpChange *= 1.15f;
        }

        if (endSTG && spellcardDisplayed)
            EndSpellcard();
    }

    public void UpdateGuages(int maxHP, int hp, int maxMP, int mp)
    {
        float healthPercent = (float)hp / maxHP;
        healthbar[0].alphaCutoff = 1 - Mathf.Clamp01((healthPercent - .666f) / .333f);
        healthbar[1].alphaCutoff = 1 - Mathf.Clamp01((healthPercent - .333f) / .333f);
        healthbar[2].alphaCutoff = 1 - Mathf.Clamp01(healthPercent / .333f);

        float mpPercent = (float)mp / maxMP;
        manaBar[0].alphaCutoff = 1 - Mathf.Clamp01((mpPercent - .5f) / .5f);
        manaBar[1].alphaCutoff = 1 - Mathf.Clamp01(mpPercent / .5f);
    }

    public void DeclareSpellcard(float nameBoxLength, string spellName, Sprite portraitSprite)
    {
        spellcardDisplayed = true;

        spellcardText.text = spellName;
        StartCoroutine(ExtendSpellNameBox(nameBoxLength));
        spellcardDeclareAnim.SetBool("Spellcard", true);

        if(portraitSprite != null)
        {
            spellcardPortrait.sprite = portraitSprite;
            spellcardDeclareAnim.SetBool("Portrait", true);
        }
    }

    public void EndSpellcard()
    {
        spellcardDisplayed = false;

        spellcardText.text = "Unknown \"Missing No.\"";
        spellcardDeclareAnim.SetBool("Spellcard", false);

        spellcardPortrait.sprite = null;
        spellcardDeclareAnim.SetBool("Portrait", false);
    }

    IEnumerator ExtendSpellNameBox(float nameBoxLength)
    {
        float percentage = 0;
        do
        {
            percentage += 0.02f;
            spellcardNameBox.anchorMin = new Vector2(Mathf.Lerp(0.99f, .99f - nameBoxLength, percentage), spellcardNameBox.anchorMin.y);
            yield return null;
        } while (percentage < 1);
    }

    public void ResetCaptureIcons(bool disableAll)
    {
        for (int i = 0; i < captureIcons.Length; i++)
        {
            captureIcons[i].transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            captureIcons[i].GetComponent<Image>().sprite = captureIconOff;
            if (i > neededCaptures - 1 || disableAll)
                captureIcons[i].SetActive(false);
            else
                captureIcons[i].SetActive(true);
        }
    }

    public void SetCaptureIcon()
    {
        captureIcons[captures - 1].transform.GetChild(0).GetComponent<ParticleSystem>().Play();
        captureIcons[captures - 1].GetComponent<Image>().sprite = captureIconLit;
    }

    public void UpdateCaptures()
    {
        captures++;
        SetCaptureIcon();
        AudioSourceExtensions.PlayClip2D(captureGet);
        CaptureFreeze();
        if (captures >= neededCaptures)
        {
            captureSuccess = true;
            endSTG = true;
        }
    }

    public void CaptureFreeze()
    {
        freezeDuration = 30;
    }

    public void ActivateCaptureMode(uint newCaptureCount)
    {
        captureMode = true;
        captureSuccess = false;
        neededCaptures = newCaptureCount;
        captures = 0;
        border.sprite = captureBorder;
        ResetCaptureIcons(false);
        guageContainer.SetActive(false);
    }

    public void ExitCaptureMode()
    {
        captureMode = false;
        captureSuccess = false;
        neededCaptures = 0;
        border.sprite = normalBorder;
        ResetCaptureIcons(true);
        guageContainer.SetActive(true);
    }

    public void SpawnPlayer(ShmupPlayerData playerData, ShotTypeData shotData, int partyIndex, GameMasterScript.DamageTypes damageType, Character.StatusEffect[] effects)
    {
        spellCap = true;
        playerTransform = Instantiate(shmupCharSpawn, transform.parent).transform;
        ShmupChar charScript = playerTransform.GetComponent<ShmupChar>();
        charScript.SetCharacter(playerData, shotData, partyIndex, damageType, effects);
        charScript.Respawn();
    }

    public void SpawnEnemy(GameObject enemySpawn, Character.WeaknessAndResistance[] weaknessAndResistances, float initialDelay, Vector2 position, int patternIndex, bool spell)
    {
        Transform newEnemy = Instantiate(enemySpawn, enemyContainer).transform;
        newEnemy.localPosition = position;
        ShmupEnemyScript newScript = newEnemy.GetComponent<ShmupEnemyScript>();
        newScript.patternInitialCountdown = initialDelay;
        newScript.patternIndex = patternIndex;
        newScript.spellcard = spell;
        newScript.weaknessAndResistances = weaknessAndResistances;
    }

    public void DestroySTG()
    {
        Destroy(playerTransform.gameObject);
        for (int i = 0; i < enemyContainer.childCount; i++)
        {
            Destroy(enemyContainer.GetChild(i).gameObject);
        }
        for (int i = 0; i < bulletContainer.childCount; i++)
        {
            bulletContainer.GetChild(i).gameObject.SetActive(false);
        }
    }
}
