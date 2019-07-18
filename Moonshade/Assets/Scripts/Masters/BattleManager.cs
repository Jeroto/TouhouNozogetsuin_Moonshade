using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public static BattleManager battleManager;
    public ShmupManager shmupManager;

    [SerializeField] RPGCharacter[] playerPartyChars;
    [SerializeField] RPGCharacter[] enemyPartyChars;

    public bool switchToSTG;
    public bool switchingToSTG;
    public bool inSTG;
    public bool returningToRpg;

    public bool usingSkill;

    public RpgEnemy[] enemyParty = new RpgEnemy[4];

    public EnemyCharData[] possibleEnemies;

    GameMasterScript gameMaster;
    InputScript input;

    [SerializeField] BattleUI battleUI;
    Transform mainCam;
    Animator camAnimations;

    public bool noEscape;

    public RpgMenu currentMenu;
    public RpgMenu prevMenu;

    public int[] turnTimer = new int[8];
    public int currentTurn = -1;
    int turnValue;

    [SerializeField] int rpgEntranceDelay;
    int entranceDelayPassed;

    int menuInteractionDelay;

    int mainIndex;
    int attackSelectIndex;
    int skillSelectIndex;
    int itemSelectIndex;
    public int charSelectIndex = -1;
    public int[] charsSelected = new int[0];

    public enum RpgMenu {Main, Attack, Skill, Item, Escape, SelectChar, BulletHell, None};

    public enum EnemyCharSelection { First, Second, Third, Fourth, LowestHP, HighestHP, LowestDefense, HighestDefense };

    [System.Serializable]
    struct RPGCharacter
    {
        public Transform character;
        public Transform sprite;
        public SpriteRenderer renderer;
        public Animator animator;
        public HoverScript hoverScript;
    }

    private void Awake()
    {
        battleManager = this;
        mainCam = Camera.main.transform;
        camAnimations = mainCam.GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameMaster = GameMasterScript.gameMaster;
        shmupManager = ShmupManager.shmupManager;
        gameMaster.loading = false; //VERY TEMPORARY
        input = GameMasterScript.inputScript;
        SpawnEnemies(gameMaster.enemyPartySpawns);
        DetectCharacters();
        battleUI.CharacterCursors(charSelectIndex, charsSelected);
        CalculateTurnValue();
        TurnChange();
    }

    // Update is called once per frame
    void Update()
    {
        if (entranceDelayPassed == -1)
        {
            battleUI.UpdateCharacterStatus();
            if (switchToSTG)
                StartCoroutine(SwitchToSTGCam());

            if (!(inSTG || switchingToSTG || returningToRpg))
            {
                if (shmupManager.endSTG && !usingSkill)
                {
                    shmupManager.endSTG = false;
                    if (currentTurn > 3)
                        enemyParty[currentTurn - 4].currentHP -= Mathf.FloorToInt(shmupManager.enemyHpChange);
                    else
                        enemyParty[charSelectIndex - 4].currentHP -= Mathf.FloorToInt(shmupManager.enemyHpChange);

                    TurnChange();
                }

                if (currentTurn > 3)
                    EnemyTurn();

                if (Input.GetKeyDown(KeyCode.C))
                    TurnChange();

                if(usingSkill)
                {
                    if(!gameMaster.playerParty[currentTurn].skills[skillSelectIndex].skill.captureRequired || (shmupManager.captureMode && shmupManager.captureSuccess))
                    {
                        usingSkill = false;
                        int[] targetInts = TargetHandling();
                        Character[] allTargets = new Character[targetInts.Length];
                        for (int i = 0; i < targetInts.Length; i++)
                        {
                            if (targetInts[i] > 3)
                                allTargets[i] = enemyParty[targetInts[i] - 4];
                            else
                                allTargets[i] = gameMaster.playerParty[targetInts[i]];
                        }
                        UseSkill(gameMaster.playerParty[currentTurn], allTargets, gameMaster.playerParty[currentTurn].skills[skillSelectIndex].skill);
                        shmupManager.ExitCaptureMode();
                        TurnChange();
                    }
                    else if (!shmupManager.captureMode)
                    {
                        shmupManager.ActivateCaptureMode(3);
                        switchToSTG = true;
                    }
                    else if (!shmupManager.captureSuccess && !switchToSTG)
                    {
                        usingSkill = false;
                        shmupManager.ExitCaptureMode();
                        TurnChange();
                    }
                }

                if (menuInteractionDelay <= 0)
                {
                    MenuNavigation();
                }
                else
                    menuInteractionDelay--;
            }
            else if (inSTG && shmupManager.endSTG)
            {
                StartCoroutine(SwitchToRPGCam());
            }
        }
        else
        {
            entranceDelayPassed++;
            if (entranceDelayPassed >= rpgEntranceDelay)
            {
                entranceDelayPassed = -1;
                StartCoroutine(battleUI.FadeRPGMenu(true));
            }
        }
    }

    void SpawnEnemies(int[] spawns)
    {
        int enemyIndex;
        for (int i = 0; i < 4; i++)
        {
            enemyParty[i] = new RpgEnemy();
            if (spawns[i] == -1)
            {
                enemyParty[i].charName = "na";
                enemyPartyChars[i].character.gameObject.SetActive(false);
                continue;
            }

            enemyPartyChars[i].character.gameObject.SetActive(true);
            enemyIndex = spawns[i];

            enemyParty[i].animations = possibleEnemies[enemyIndex].animations;
            enemyParty[i].hoverAnim = possibleEnemies[enemyIndex].hoverAnim;
            enemyParty[i].hoverDist = possibleEnemies[enemyIndex].hoverDist;
            enemyParty[i].hoverTime = possibleEnemies[enemyIndex].hoverTime;
            enemyParty[i].charName = possibleEnemies[enemyIndex].charName;
            enemyParty[i].maxHP = possibleEnemies[enemyIndex].maxHP;
            enemyParty[i].currentHP = possibleEnemies[enemyIndex].currentHP;
            enemyParty[i].maxMP = possibleEnemies[enemyIndex].maxMP;
            enemyParty[i].currentMP = possibleEnemies[enemyIndex].currentMP;

            enemyParty[i].attack = possibleEnemies[enemyIndex].attack;
            enemyParty[i].subAttack = possibleEnemies[enemyIndex].subAttack;
            enemyParty[i].defense = possibleEnemies[enemyIndex].defense;
            enemyParty[i].mentalDef = possibleEnemies[enemyIndex].mentalDef;
            enemyParty[i].turnSpeed = possibleEnemies[enemyIndex].turnSpeed;
            enemyParty[i].statusEffects = new List<Character.StatusEffect>();
            enemyParty[i].weaknessesAndResistances = possibleEnemies[enemyIndex].weaknessesAndResistances;
            enemyParty[i].damageTypeUnlocks = possibleEnemies[enemyIndex].damageTypeUnlocks;
            enemyParty[i].skills = possibleEnemies[enemyIndex].skills;


            enemyParty[i].charExp = possibleEnemies[enemyIndex].charExp;

            //Spellcards

            enemyParty[i].charLevel = possibleEnemies[enemyIndex].charLevel;

            enemyParty[i].portrait = possibleEnemies[enemyIndex].portrait;

            enemyParty[i].shmupSpawn = possibleEnemies[enemyIndex].shmupSpawn;

            enemyPartyChars[i].animator.runtimeAnimatorController = enemyParty[i].animations;
            enemyPartyChars[i].hoverScript.active = enemyParty[i].hoverAnim;
            enemyPartyChars[i].hoverScript.distance = enemyParty[i].hoverDist;
            enemyPartyChars[i].hoverScript.frames = enemyParty[i].hoverTime;
        }
    }

    void DetectCharacters()
    {
        for (int i = 0; i < 4; i++)
        {
            if (gameMaster.playerParty[i].charName == "na")
            {
                playerPartyChars[i].character.gameObject.SetActive(false);
                continue;
            }

            playerPartyChars[i].character.gameObject.SetActive(true);

            playerPartyChars[i].animator.runtimeAnimatorController = gameMaster.playerParty[i].animations;
            playerPartyChars[i].hoverScript.active = gameMaster.playerParty[i].hoverAnim;
            playerPartyChars[i].hoverScript.distance = gameMaster.playerParty[i].hoverDist;
            playerPartyChars[i].hoverScript.frames = gameMaster.playerParty[i].hoverTime;

        }
    }

    void CalculateTurnValue()
    {
        int charactersFactored = 0;
        for (int i = 0; i < 4; i++)
        {
            if (gameMaster.playerParty[i].charName != "na" && gameMaster.playerParty[i].currentHP > 0)
            {
                turnValue += gameMaster.playerParty[i].turnSpeed;
                charactersFactored++;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            if (enemyParty[i].charName != "na" && enemyParty[i].currentHP > 0)
            {
                turnValue += enemyParty[i].turnSpeed;
                charactersFactored++;
            }
        }
        turnValue /= charactersFactored;
    }

    void StatusTimers()
    {
        if (currentTurn < 0)
            return;

        Character currentChar;
        if (currentTurn > 3)
            currentChar = enemyParty[currentTurn - 4];
        else
            currentChar = gameMaster.playerParty[currentTurn];

        Character.StatusEffect currentEffect;

        for (int i = 0; i < currentChar.statusEffects.Count; i++)
        {
            currentEffect = currentChar.statusEffects[i];
            currentEffect.turnDuration--;
            currentChar.statusEffects.RemoveAt(i);
            if (currentEffect.turnDuration > 0)
                currentChar.statusEffects.Insert(i, currentEffect);
        }
    }

    void TurnChange()
    {
        StatusTimers();
        if (currentTurn > -1)
            turnTimer[currentTurn] -= turnValue;
        CalculateTurnValue();
        currentTurn = -1;
        do
        {
            if (currentTurn >= 0)
                AddToTimers(ref turnTimer);
            currentTurn = 0;
            for (int i = 1; i < turnTimer.Length; i++)
            {
                if (turnTimer[i] > turnTimer[currentTurn])
                    currentTurn = i;
            }
        }
        while (turnTimer[currentTurn] < turnValue);

        currentMenu = RpgMenu.Main;
        battleUI.UpdateMenuText(currentTurn);
        battleUI.ChangeBattleMenu();
    }

    public int[] CheckFutureTurns(int[] turnTimerData)
    {
        int[] tempTurnTimers = new int[turnTimerData.Length];
        for (int i = 0; i < tempTurnTimers.Length; i++)
        {
            tempTurnTimers[i] = turnTimerData[i];
        }
        int[] projectedTurnOrder = new int[6] { -1, -1, -1, -1, -1, -1 };
        while(projectedTurnOrder[5] < 0)
        {
            int highestTimer = 0;
            for (int i = 1; i < tempTurnTimers.Length; i++)
            {
                if (tempTurnTimers[i] > tempTurnTimers[highestTimer])
                    highestTimer = i;
            }
            if (tempTurnTimers[highestTimer] < turnValue)
                AddToTimers(ref tempTurnTimers);
            else
            {
                for (int i = 0; i < projectedTurnOrder.Length; i++)
                {
                    if(projectedTurnOrder[i] < 0)
                    {
                        projectedTurnOrder[i] = highestTimer;
                        tempTurnTimers[highestTimer] -= turnValue;
                        break;
                    }
                }
            }
        }
        Debug.Log(projectedTurnOrder[0] + ", " + projectedTurnOrder[1] + ", " + projectedTurnOrder[2] + ", " + projectedTurnOrder[3] + ", " + projectedTurnOrder[4] + 
            ", " + projectedTurnOrder[5]);
        return projectedTurnOrder;
    }

    public void AddToTimers(ref int[] timersToAdd)
    {
        for (int i = 0; i < 4; i++)
        {
            timersToAdd[i] += gameMaster.playerParty[i].turnSpeed;
        }
        for (int i = 0; i < 4; i++)
        {
            timersToAdd[i + 4] += enemyParty[i].turnSpeed;
        }
    }

    void MenuNavigation()
    {
        int maxIndex;
        switch (currentMenu)
        {
            case RpgMenu.Main:
                mainIndex += (input.directionDown.x != 0) ? (int)Mathf.Sign(input.directionDown.x) : 0;
                mainIndex = Mathf.Clamp(mainIndex, 0, (noEscape) ? 2 : 3);
                battleUI.HighlightMainOption(mainIndex);
                break;

            case RpgMenu.Attack:
                maxIndex = gameMaster.playerParty[currentTurn].damageTypeUnlocks.Length - 1;
                for (int i = 0; i < gameMaster.playerParty[currentTurn].damageTypeUnlocks.Length; i++)
                {
                    if (gameMaster.playerParty[currentTurn].damageTypeUnlocks[i].levelToUnlock > gameMaster.playerParty[currentTurn].charLevel)
                    {
                        maxIndex = i - 1;
                        break;
                    }
                }
                attackSelectIndex += (input.directionDown.y != 0) ? (int)Mathf.Sign(-input.directionDown.y) : 0;
                attackSelectIndex = Mathf.Clamp(attackSelectIndex, 0, maxIndex);
                battleUI.HighlightAttackOption(attackSelectIndex);
                break;

            case RpgMenu.Skill:
                maxIndex = gameMaster.playerParty[currentTurn].skills.Length - 1;
                for (int i = 0; i < gameMaster.playerParty[currentTurn].skills.Length; i++)
                {
                    if (gameMaster.playerParty[currentTurn].skills[i].levelToUnlock > gameMaster.playerParty[currentTurn].charLevel)
                    {
                        maxIndex = i - 1;
                        break;
                    }
                }
                skillSelectIndex += (input.directionDown.y != 0) ? (int)Mathf.Sign(-input.directionDown.y) : 0;
                skillSelectIndex = Mathf.Clamp(skillSelectIndex, 0, maxIndex);
                battleUI.HighlightSkillOption(skillSelectIndex);
                break;

            case RpgMenu.SelectChar:
                battleUI.CharacterCursors(charSelectIndex, charsSelected);
                CharacterSelection();
                break;
        }

        if (input.shootDown)
            MenuConfirm();
        else if (input.bombDown)
            MenuBack();
    }

    void CharacterSelection()
    {
        switch(prevMenu)
        {
            case RpgMenu.Attack:
                SelectEnemy();
                break;

            case RpgMenu.Item:
                SelectAlly(false);
                break;

            case RpgMenu.Skill:
                Skill.SkillType skillType = gameMaster.playerParty[currentTurn].skills[skillSelectIndex].skill.skillType;

                switch(skillType)
                {
                    case Skill.SkillType.EnemyDebuff:
                        SelectEnemy();
                        break;

                    case Skill.SkillType.EnemyPartyDebuff:
                        SelectEnemyParty();
                        break;

                    case Skill.SkillType.EnemyAreaDebuff:
                        SelectEnemy();
                        SelectArea(false);
                        break;

                    case Skill.SkillType.AllyBuff:
                        SelectAlly(false);
                        break;

                    case Skill.SkillType.AllyPartyBuff:
                        SelectAllyParty(false);
                        break;

                    case Skill.SkillType.AllyAreaBuff:
                        SelectAlly(false);
                        SelectArea(false);
                        break;

                    case Skill.SkillType.FieldWide:
                        SelectAll(false);
                        break;

                    case Skill.SkillType.Selfcast:
                        charSelectIndex = currentTurn;
                        break;
                }
                break;
        }
    }

    void SelectEnemy()
    {
        charSelectIndex += (input.directionDown.y != 0) ? (int)Mathf.Sign(-input.directionDown.y) : 0;
        charSelectIndex = Mathf.Clamp(charSelectIndex, 4, 7);

        while (enemyParty[charSelectIndex - 4].charName == "na" || enemyParty[charSelectIndex - 4].currentHP <= 0)
        {
            charSelectIndex++;
            if (charSelectIndex > 7)
                charSelectIndex = 4;
        }
    }

    void SelectEnemyParty()
    {
        bool[] enemiesAlive = new bool[4];
        int enemiesAliveCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (enemyParty[i].charName != "na" && enemyParty[i].currentHP > 0)
            {
                enemiesAliveCount++;
                enemiesAlive[i] = true;
            }
        }
        charsSelected = new int[enemiesAliveCount];
        for (int i = 0; i < charsSelected.Length; i++)
        {
            for (int k = 0; k < enemiesAlive.Length; k++)
            {
                if(enemiesAlive[k])
                {
                    charsSelected[i] = k+4;
                    enemiesAlive[k] = false;
                    break;
                }
            }
        }
    }

    void SelectAllyParty(bool livingOnly)
    {
        bool[] playersSelectable = new bool[4];
        int playersSelectableCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (gameMaster.playerParty[i].charName != "na" && (!livingOnly || gameMaster.playerParty[i].currentHP > 0))
            {
                playersSelectableCount++;
                playersSelectable[i] = true;
            }
        }
        charsSelected = new int[playersSelectableCount];
        for (int i = 0; i < charsSelected.Length; i++)
        {
            for (int k = 0; k < playersSelectable.Length; k++)
            {
                if (playersSelectable[k])
                {
                    charsSelected[i] = k;
                    playersSelectable[k] = false;
                    break;
                }
            }
        }
    }

    void SelectAlly(bool livingOnly)
    {
        charSelectIndex += (input.directionDown.y != 0) ? (int)Mathf.Sign(-input.directionDown.y) : 0;
        charSelectIndex = Mathf.Clamp(charSelectIndex, 0, 3);

        while (gameMaster.playerParty[charSelectIndex].charName == "na" && (!livingOnly || gameMaster.playerParty[charSelectIndex].currentHP > 0))
        {
            charSelectIndex++;
            if (charSelectIndex > 3)
                charSelectIndex = 0;
        }
    }

    void SelectArea(bool playersMustLive)
    {
        bool charAbove = false;
        bool charBelow = false;
        if (charSelectIndex < 4)
        {
            if(charSelectIndex > 0)
            {
                if (gameMaster.playerParty[charSelectIndex - 1].charName != "na" && (!playersMustLive || gameMaster.playerParty[charSelectIndex - 1].currentHP > 0))
                    charAbove = true;
            }
            if(charSelectIndex < 3)
            {
                if (gameMaster.playerParty[charSelectIndex + 1].charName != "na" && (!playersMustLive || gameMaster.playerParty[charSelectIndex + 1].currentHP > 0))
                    charBelow = true;
            }
        }
        else
        {
            if (charSelectIndex > 4)
            {
                if (enemyParty[charSelectIndex - 5].charName != "na" && enemyParty[charSelectIndex - 5].currentHP > 0)
                    charAbove = true;
            }
            if (charSelectIndex < 7)
            {
                if (enemyParty[charSelectIndex - 3].charName != "na" && enemyParty[charSelectIndex - 3].currentHP > 0)
                    charBelow = true;
            }
        }

        if (charAbove && charBelow)
            charsSelected = new int[2] { charSelectIndex - 1, charSelectIndex + 1 };
        else
        {
            if (charAbove)
                charsSelected = new int[1] { charSelectIndex - 1 };
            else
                charsSelected = new int[1] { charSelectIndex + 1 };
        }
    }

    void SelectAll(bool playersMustLive)
    {
        bool[] charsSelected = new bool[8];
        int charsAliveCount = 0;
        for (int i = 0; i < 8; i++)
        {
            if(i < 4)
            {
                if (gameMaster.playerParty[i].charName != "na" && (!playersMustLive || gameMaster.playerParty[i].currentHP > 0))
                {
                    charsAliveCount++;
                    charsSelected[i] = true;
                }
            }
            else
            {
                if (enemyParty[i-4].charName != "na" && enemyParty[i-4].currentHP > 0)
                {
                    charsAliveCount++;
                    charsSelected[i] = true;
                }
            }
        }
        this.charsSelected = new int[charsAliveCount];
        for (int i = 0; i < this.charsSelected.Length; i++)
        {
            for (int k = 0; k < charsSelected.Length; k++)
            {
                if (charsSelected[k])
                {
                    this.charsSelected[i] = k;
                    charsSelected[k] = false;
                    break;
                }
            }
        }
    }

    void MenuConfirm()
    {
        if(currentMenu != RpgMenu.SelectChar)
            prevMenu = currentMenu;

        switch (currentMenu)
        {
            case RpgMenu.Main:
                switch(mainIndex)
                {
                    case 0:
                        currentMenu = RpgMenu.Attack;
                        break;

                    case 1:
                        currentMenu = RpgMenu.Skill;
                        break;

                    case 2:
                        currentMenu = RpgMenu.Item;
                        break;

                    case 3:
                        currentMenu = RpgMenu.Escape;
                        break;
                }
                break;

            case RpgMenu.Attack:
                currentMenu = RpgMenu.SelectChar;
                break;

            case RpgMenu.Skill:
                currentMenu = RpgMenu.SelectChar;
                break;

            case RpgMenu.Item:
                currentMenu = RpgMenu.SelectChar;
                break;

            case RpgMenu.SelectChar:
                switch(prevMenu)
                {
                    case RpgMenu.Attack:
                        switchToSTG = true;
                        break;

                    case RpgMenu.Skill:
                        usingSkill = true;
                        break;
                }

                battleUI.CharacterCursors(-1, new int[0]);
                break;
        }
        menuInteractionDelay = 10;
        battleUI.ChangeBattleMenu();
    }

    int[] TargetHandling()
    {
        int[] allTargets = new int[charsSelected.Length + ((charSelectIndex != -1) ? 1 : 0)];
        if (charSelectIndex != -1)
            allTargets[0] = charSelectIndex;
        for (int i = 0; i < charsSelected.Length; i++)
        {
            allTargets[i + ((charSelectIndex != -1) ? 1 : 0)] = charsSelected[i];
        }
        return allTargets;
    }

    void MenuBack()
    {
        switch (currentMenu)
        {
            case RpgMenu.Attack:
                currentMenu = RpgMenu.Main;
                break;
            case RpgMenu.Skill:
                currentMenu = RpgMenu.Main;
                break;
            case RpgMenu.Item:
                currentMenu = RpgMenu.Main;
                break;
            case RpgMenu.SelectChar:
                currentMenu = prevMenu;
                charsSelected = new int[0];
                charSelectIndex = -1;
                battleUI.CharacterCursors(charSelectIndex, charsSelected);
                break;
        }
        menuInteractionDelay = 10;
        battleUI.ChangeBattleMenu();
    }


    void UseSkill(Character caster, Character[] targets, SkillData skill)
    {
        Skill.UseSkill(skill, caster, targets);
    }


    void EnemyTurn()
    {
        currentMenu = RpgMenu.None;
        battleUI.ChangeBattleMenu();
        Debug.Log("Enemy " + (currentTurn - 4).ToString() + " (" + enemyParty[currentTurn - 4].charName + ")'s turn");
        switchToSTG = true;
    }

    void SwitchToSTG(int playerTarget, RpgEnemy enemyAttacker, int patternIndex, bool spell)
    {
        camAnimations.SetBool("BulletHell", true);
        shmupManager.endSTG = false;
        shmupManager.SpawnPlayer(gameMaster.playerParty[playerTarget].shmupData, gameMaster.playerParty[playerTarget].shotTypeEquip, playerTarget, gameMaster.playerParty[playerTarget].damageTypeUnlocks[attackSelectIndex].damageType, gameMaster.playerParty[playerTarget].statusEffects.ToArray());
        shmupManager.SpawnEnemy(enemyAttacker.shmupSpawn, enemyAttacker.weaknessesAndResistances, 120, new Vector2(0, 275), patternIndex, spell);
        gameMaster.lockInputs = true;

        switchToSTG = false;
        switchingToSTG = true;
    }

    void InitializeSTG(int player, GameMasterScript.DamageTypes damageType, RpgEnemy enemy, int patternIndex, bool spell)
    {
        shmupManager.SpawnPlayer(gameMaster.playerParty[player].shmupData, gameMaster.playerParty[player].shotTypeEquip, player, damageType, gameMaster.playerParty[player].statusEffects.ToArray());
        shmupManager.SpawnEnemy(enemy.shmupSpawn, enemy.weaknessesAndResistances, 120, new Vector2(0, 275), patternIndex, spell);
    }

    void SwitchToRPG()
    {
        camAnimations.SetBool("BulletHell", false);
        mainCam.gameObject.SetActive(true);
        inSTG = false;
        returningToRpg = true;
    }

    IEnumerator SwitchToSTGCam()
    {
        switchToSTG = false;
        switchingToSTG = true;
        shmupManager.startingSTG = true;
        shmupManager.endSTG = false;
        camAnimations.SetBool("BulletHell", true);
        StartCoroutine(battleUI.FadeRPGMenu(false));
        gameMaster.lockInputs = true;

        do
        {
            yield return null;
        } while (!battleUI.FadeBlackscreen(true));

        int delayCounter = 5;
        do
        {
            delayCounter--;
            yield return null;
        } while (delayCounter > 0);

        mainCam.gameObject.SetActive(false);
        shmupManager.blackScreen.color = new Color(0, 0, 0, 1);
        shmupManager.shmupCam.gameObject.SetActive(true);
        inSTG = true;
        switchingToSTG = false;

        if (currentTurn > 3)
        {
            int player = Random.Range(0, 2);
            InitializeSTG(player, gameMaster.playerParty[player].damageTypeUnlocks[0].damageType, enemyParty[currentTurn - 4], 0, true);
        } 
        else
            InitializeSTG(currentTurn, gameMaster.playerParty[currentTurn].damageTypeUnlocks[attackSelectIndex].damageType, enemyParty[charSelectIndex - 4], 0, usingSkill);

        do
        {
            shmupManager.blackScreen.color = new Color(0, 0, 0, shmupManager.blackScreen.color.a - (gameMaster.timeScale * 0.025f));
            yield return null;
        } while (shmupManager.blackScreen.color.a > 0);

        gameMaster.lockInputs = false;
        shmupManager.startingSTG = false;
        shmupManager.inSTG = true;
    }


    IEnumerator SwitchToRPGCam()
    {

        inSTG = false;
        shmupManager.inSTG = false;
        returningToRpg = true;
        gameMaster.lockInputs = true;

        do
        {
            shmupManager.blackScreen.color = new Color(0, 0, 0, shmupManager.blackScreen.color.a + (gameMaster.timeScale * 0.025f));
            yield return null;
        } while (shmupManager.blackScreen.color.a < 1);

        shmupManager.DestroySTG();

        int delayCounter = 5;
        do
        {
            delayCounter--;
            yield return null;
        } while (delayCounter > 0);

        camAnimations.SetBool("BulletHell", false);
        mainCam.gameObject.SetActive(true);
        battleUI.InstantBlackscreen(true);
        shmupManager.shmupCam.gameObject.SetActive(false);
        returningToRpg = false;
        entranceDelayPassed = 0;

        do
        {
            yield return null;
        } while (!battleUI.FadeBlackscreen(false));

        StartCoroutine(battleUI.FadeRPGMenu(true));

        gameMaster.lockInputs = false;
    }
}
