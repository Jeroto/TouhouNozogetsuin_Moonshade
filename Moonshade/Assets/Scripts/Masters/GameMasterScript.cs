using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMasterScript : MonoBehaviour
{
    
    public static GameMasterScript gameMaster;
    public static InputScript inputScript;
    public static bool overworld;
    public static float frameTime = 1f / 60f;
    static System.Random random = new System.Random(0);
    public float timeScale = 1;

    public int randomSeed;

    public AudioSource bgmSource;

    public Language languageSetting;

    public static uint maxLevel = 200;
    public static ulong maxExp;

    public bool loading;
    public bool pause;
    public bool lockInputs;

    public int previousSceneNum;
    public Vector2 overworldPos;

    public Transform playerTrans;

    public PartyCharacter[] playerParty = new PartyCharacter[4];
    public int[] enemyPartySpawns;

    public ulong[] possiblePartyMemberExp;
    public PartyCharacterData[] possiblePartyMembers;

    public enum DamageTypes {Spiritual, Mental, Physical, Magic, Space, Boundry, Holy, Cursed, Audio, Visual, Recovery, Peace, Chaos, Light, Shadow, Ice, Fire, Water, Air, Earth,
        Metal, Wood, Solar, Lunar, Nature, Poison};

    public enum Language { English };

    private void Awake()
    {
        if (gameMaster == null)
        {
            gameMaster = this;

            ShmupEnemyScript.shmupEnemyRandom = new System.Random();

            bgmSource = transform.GetChild(0).GetComponent<AudioSource>();
            maxExp = CalculateTotalExpNeeded(maxLevel);
            Debug.Log("Max exp is: " + maxExp.ToString());
            DontDestroyOnLoad(gameObject);
            Debug.Log("Game master script set.");
            Application.targetFrameRate = 60;
        }
        else
            Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        possiblePartyMemberExp = new ulong[possiblePartyMembers.Length];
        SetPlayerParty(new PartyCharacterData[2] { possiblePartyMembers[0], possiblePartyMembers[1] }, new int[2] { 0, 1 });
        playerTrans = GameObject.Find("OverworldChar").transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerParty(PartyCharacterData[] newCharacters, int[] charIndexes)
    {
        playerParty = new PartyCharacter[4];
        for (int i = 0; i < 4; i++)
        {
            playerParty[i] = new PartyCharacter();
            if (i < newCharacters.Length)
            {
                playerParty[i].animations = newCharacters[i].animations;
                playerParty[i].hoverAnim = newCharacters[i].hoverAnim;
                playerParty[i].hoverDist = newCharacters[i].hoverDist;
                playerParty[i].hoverTime = newCharacters[i].hoverTime;
                playerParty[i].charName = newCharacters[i].charName;
                playerParty[i].maxHP = newCharacters[i].maxHP;
                playerParty[i].maxMP = newCharacters[i].maxMP;
                playerParty[i].attack = newCharacters[i].attack;
                playerParty[i].subAttack = newCharacters[i].subAttack;
                playerParty[i].defense = newCharacters[i].defense;
                playerParty[i].mentalDef = newCharacters[i].mentalDef;
                playerParty[i].turnSpeed = newCharacters[i].turnSpeed;
                playerParty[i].statusEffects = new List<Character.StatusEffect>();
                playerParty[i].weaknessesAndResistances = newCharacters[i].weaknessesAndResistances;
                playerParty[i].damageTypeUnlocks = newCharacters[i].damageTypeUnlocks;
                playerParty[i].skills = newCharacters[i].skills;

                playerParty[i].possibleCharacterIndex = charIndexes[i];
                playerParty[i].charExp = possiblePartyMemberExp[playerParty[i].possibleCharacterIndex];

                playerParty[i].charType = newCharacters[i].charType;
                playerParty[i].charSubtype = newCharacters[i].charSubtype;
                playerParty[i].movementSpeed = newCharacters[i].movementSpeed;
                playerParty[i].focusSpeed = newCharacters[i].focusSpeed;
                playerParty[i].portrait = newCharacters[i].portrait;
                playerParty[i].charLevel = 1;

                playerParty[i].shmupData = newCharacters[i].shmupData;
                playerParty[i].shotTypeEquip = newCharacters[i].defaultShot;
            }
            else
            {
                playerParty[i].charName = "na";
            }
        }
        FullHealParty();
    }

    public void FullHealParty()
    {
        for (int i = 0; i < 4; i++)
        {
            if(playerParty[i].charName != "na")
            {
                playerParty[i].currentHP = playerParty[i].maxHP;
                playerParty[i].currentMP = playerParty[i].maxMP;
            }
        }
    }

    public void LevelUpCharacters()
    {
        bool fullyLeveledCharacter = false;
        do
        {
            fullyLeveledCharacter = true;
            if (playerParty[0].charExp > CalculateTotalExpNeeded(playerParty[0].charLevel))
            {
                playerParty[0].charLevel++;
                fullyLeveledCharacter = false;
            }
        } while (!fullyLeveledCharacter);

        if (playerParty[1].charName != "na")
        {
            do
            {
                fullyLeveledCharacter = true;
                if (playerParty[1].charExp > CalculateTotalExpNeeded(playerParty[1].charLevel))
                {
                    playerParty[1].charLevel++;
                    fullyLeveledCharacter = false;
                }
            } while (!fullyLeveledCharacter);
        }

        if (playerParty[2].charName != "na")
        {
            do
            {
                fullyLeveledCharacter = true;
                if (playerParty[2].charExp > CalculateTotalExpNeeded(playerParty[2].charLevel))
                {
                    playerParty[2].charLevel++;
                    fullyLeveledCharacter = false;
                }
            } while (!fullyLeveledCharacter);
        }

        if (playerParty[3].charName != "na")
        {
            fullyLeveledCharacter = false;
            do
            {
                fullyLeveledCharacter = true;
                if (playerParty[3].charExp > CalculateTotalExpNeeded(playerParty[3].charLevel))
                {
                    playerParty[3].charLevel++;
                    fullyLeveledCharacter = false;
                }
            } while (!fullyLeveledCharacter);
        }
    }

    public ulong CalculateTotalExpNeeded(uint level)
    {
        ulong exp = 0;
        for (int i = 1; i <= level; i++)
        {
            exp += (ulong)Mathf.Ceil(100 * Mathf.Pow(1.25f, i / 3.9f));
            //Debug.Log("Exp is: " + exp.ToString());
        }
        return exp;
    }

    public void LevelUpStats()
    {

    }

    public void ChangeTrack(AudioClip clip)
    {
        bgmSource.clip = clip;
    }


    //All random functions
    public void GenerateRandomSeed()
    {
        randomSeed = Mathf.RoundToInt((Input.mousePosition.x * 153) + (Input.mousePosition.y * 153) + Mathf.Clamp(Time.realtimeSinceStartup, 25, 158976000) +
            ((Time.unscaledDeltaTime * 10000) * (((inputScript.shootDown) ? 5 : 1) + ((inputScript.bombDown) ? 17 : -5) + ((inputScript.directionalInput.x > 0) ? 1 : 2)
            + ((inputScript.directionalInput.y < 0) ? 11 : 22))));
        UpdateSeed(randomSeed);
    }

    public void UpdateSeed(int newSeed)
    {
        randomSeed = newSeed;
        random = new System.Random(randomSeed);
        Debug.Log("Game's Random is set to " + randomSeed);
    }

    public static int Random(int lower, int upper)
    {
        return random.Next(lower, upper);
    }

    public static float Random(float lower, float upper)
    {

        return (((float)random.NextDouble() * Mathf.Abs(lower - upper)) + lower);
    }
}
