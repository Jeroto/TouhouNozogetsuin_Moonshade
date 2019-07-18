using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    GameMasterScript gameMaster;
    Transform thisTrans;
    BattleManager battleManager;

    [SerializeField] int statsFadeFrames;

    public Image blackScreen;

    Transform rpgMenuContainer;
    Transform playerStatsContainer;
    PlayerStatPanel[] playerStatPanels;
    Transform enemyStatsContainer;
    EnemyStatPanel[] enemyStatPanels;

    Image[] rpgMenuImages;
    float[] startingRPGImageAlphas;
    TextMeshProUGUI[] rpgMenuText;
    float[] startingRPGTextAlphas;

    [SerializeField] Image[] turnOrderImages;

    [SerializeField] TextMeshProUGUI[] mainOptions;
    [SerializeField] TextMeshProUGUI[] attackOptions;
    [SerializeField] TextMeshProUGUI[] skillOptions;
    [SerializeField] TextMeshProUGUI[] skillCosts;
    [SerializeField] TextMeshProUGUI[] itemOptions;

    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject attacksPanel;
    [SerializeField] GameObject skillPanel;
    [SerializeField] GameObject itemsPanel;
    [Space]
    [SerializeField] SpriteRenderer[] characterCursors;
    [SerializeField] Sprite normalCursor;
    [SerializeField] Sprite smallCursor;
    [Space]
    [SerializeField] Sprite[] defaultEnemyPortraits;

    int pauseFadePassed;

    bool prevPaused;

    struct PlayerStatPanel
    {
        public Transform panel;
        public TextMeshProUGUI charName;
        public TextMeshProUGUI charHp;
        public TextMeshProUGUI charMp;
    }

    struct EnemyStatPanel
    {
        public Transform panel;
        public TextMeshProUGUI enemyName;
        public Transform enemyHp;
        public TextMeshProUGUI enemyLvl;
    }

    private void Awake()
    {
        thisTrans = transform;
        gameMaster = GameMasterScript.gameMaster;
        rpgMenuContainer = thisTrans.GetChild(0);
        rpgMenuContainer.gameObject.SetActive(true);
        playerStatsContainer = rpgMenuContainer.GetChild(1).Find("PlayerStats");
        playerStatPanels = new PlayerStatPanel[4];
        for (int i = 0; i < 4; i++)
        {
            playerStatPanels[i].panel = playerStatsContainer.GetChild(i);
            playerStatPanels[i].charName = playerStatPanels[i].panel.GetChild(0).GetComponent<TextMeshProUGUI>();
            playerStatPanels[i].charHp = playerStatPanels[i].panel.GetChild(1).GetComponent<TextMeshProUGUI>();
            playerStatPanels[i].charMp = playerStatPanels[i].panel.GetChild(2).GetComponent<TextMeshProUGUI>();
        }
        enemyStatsContainer = rpgMenuContainer.GetChild(1).Find("EnemyStats");
        enemyStatPanels = new EnemyStatPanel[4];
        for (int i = 0; i < 4; i++)
        {
            enemyStatPanels[i].panel = enemyStatsContainer.GetChild(i);
            enemyStatPanels[i].enemyName = enemyStatPanels[i].panel.GetChild(0).GetComponent<TextMeshProUGUI>();
            enemyStatPanels[i].enemyHp = enemyStatPanels[i].panel.GetChild(2);
            enemyStatPanels[i].enemyLvl = enemyStatPanels[i].panel.GetChild(3).GetComponent<TextMeshProUGUI>();
        }

        rpgMenuImages = rpgMenuContainer.GetComponentsInChildren<Image>();
        rpgMenuText = rpgMenuContainer.GetComponentsInChildren<TextMeshProUGUI>();
        StoreMenuAlphas();
        HidePauseMenu();
    }

    // Start is called before the first frame update
    void Start()
    {
        battleManager = BattleManager.battleManager;
        UpdateCharacterStatus();
        PreviewTurnOrder();
    }

    // Update is called once per frame
    void Update()
    {

        if (prevPaused != gameMaster.pause)
        {
            UpdateCharacterStatus();
            StartCoroutine(FadeRPGMenu(gameMaster.pause));
        }
        prevPaused = gameMaster.pause;
    }

    public void UpdateCharacterStatus()
    {
        for (int i = 0; i < 4; i++)
        {
            if (gameMaster.playerParty[i].charName != "na")
            {
                playerStatPanels[i].panel.gameObject.SetActive(true);
                playerStatPanels[i].charName.text = gameMaster.playerParty[i].charName.Remove(gameMaster.playerParty[i].charName.IndexOf(" ", 1));
                playerStatPanels[i].charHp.text = "HP: " + gameMaster.playerParty[i].currentHP.ToString() + "/" + gameMaster.playerParty[i].maxHP.ToString();
                playerStatPanels[i].charMp.text = "MP: " + gameMaster.playerParty[i].currentMP.ToString() + "/" + gameMaster.playerParty[i].maxMP.ToString();
            }
            else
                playerStatPanels[i].panel.gameObject.SetActive(false);
        }

        for (int i = 0; i < 4; i++)
        {
            if (battleManager.enemyParty[i].charName != "na")
            {
                enemyStatPanels[i].panel.gameObject.SetActive(true);
                enemyStatPanels[i].enemyName.text = battleManager.enemyParty[i].charName;
                enemyStatPanels[i].enemyHp.localScale = new Vector3(Mathf.Lerp(0, 1, (float)battleManager.enemyParty[i].currentHP / battleManager.enemyParty[i].maxHP), 1, 1);
                enemyStatPanels[i].enemyLvl.text = "Lvl: " + battleManager.enemyParty[i].charLevel;
            }
            else
                enemyStatPanels[i].panel.gameObject.SetActive(false);
        }
    }

    public void PreviewTurnOrder()
    {
        int[] turnOrderPrediction = battleManager.CheckFutureTurns(battleManager.turnTimer);
        for (int i = 0; i < turnOrderPrediction.Length; i++)
        {
            if(turnOrderPrediction[i] < 4)
            {
                turnOrderImages[i].sprite = gameMaster.playerParty[turnOrderPrediction[i]].portrait;
            }
            else
            {
                if (battleManager.enemyParty[turnOrderPrediction[i] - 4].portrait == null)
                    turnOrderImages[i].sprite = defaultEnemyPortraits[turnOrderPrediction[i] - 4];
                else
                    turnOrderImages[i].sprite = battleManager.enemyParty[turnOrderPrediction[i] - 4].portrait;
            }
        }
    }

    public void CharacterCursors(int mainSelection, int[] subSelections)
    {
        for (int i = 0; i < characterCursors.Length; i++)
        {
            characterCursors[i].enabled = false;
        }

        if(mainSelection > -1)
        {
            for (int i = 0; i < characterCursors.Length; i++)
            {
                if(i == mainSelection)
                {
                    characterCursors[i].enabled = true;
                    characterCursors[i].sprite = normalCursor;
                }
            }
        }

        for (int i = 0; i < subSelections.Length; i++)
        {
            characterCursors[subSelections[i]].enabled = true;
            characterCursors[subSelections[i]].sprite = smallCursor;
        }
    }

    void StoreMenuAlphas()
    {
        startingRPGImageAlphas = new float[rpgMenuImages.Length];
        for (int i = 0; i < rpgMenuImages.Length; i++)
        {
            startingRPGImageAlphas[i] = rpgMenuImages[i].color.a;
        }
        startingRPGTextAlphas = new float[rpgMenuText.Length];
        for (int i = 0; i < rpgMenuText.Length; i++)
        {
            startingRPGTextAlphas[i] = rpgMenuText[i].color.a;
        }
    }

    public IEnumerator FadeRPGMenu(bool inOrOut)
    {
        Color currrentColor;
        while (pauseFadePassed < statsFadeFrames)
        {
            pauseFadePassed++;
            for (int i = 0; i < rpgMenuImages.Length; i++)
            {
                currrentColor = rpgMenuImages[i].color;
                if(inOrOut)
                    currrentColor.a = Mathf.Lerp(0, startingRPGImageAlphas[i], (float)pauseFadePassed / statsFadeFrames);
                else
                    currrentColor.a = Mathf.Lerp(startingRPGImageAlphas[i], 0, (float)pauseFadePassed / statsFadeFrames);
                rpgMenuImages[i].color = currrentColor;
            }
            for (int i = 0; i < rpgMenuText.Length; i++)
            {
                currrentColor = rpgMenuText[i].color;
                if (inOrOut)
                    currrentColor.a = Mathf.Lerp(0, startingRPGTextAlphas[i], (float)pauseFadePassed / statsFadeFrames);
                else
                    currrentColor.a = Mathf.Lerp(startingRPGTextAlphas[i], 0, (float)pauseFadePassed / statsFadeFrames);
                rpgMenuText[i].color = currrentColor;
            }
            yield return null;
        }
        pauseFadePassed = 0;
    }

    void HidePauseMenu()
    {
        Color currrentColor;
        for (int i = 0; i < rpgMenuImages.Length; i++)
        {
            currrentColor = rpgMenuImages[i].color;
            currrentColor.a = 0;
            rpgMenuImages[i].color = currrentColor;
        }
        for (int i = 0; i < rpgMenuText.Length; i++)
        {
            currrentColor = rpgMenuText[i].color;
            currrentColor.a = 0;
            rpgMenuText[i].color = currrentColor;
        }
    }


    public void ChangeBattleMenu()
    {
        PreviewTurnOrder();
        switch(battleManager.currentMenu)
        {
            case BattleManager.RpgMenu.Main:
                mainMenu.SetActive(true);
                attacksPanel.SetActive(false);
                skillPanel.SetActive(false);
                itemsPanel.SetActive(false);
                break;

            case BattleManager.RpgMenu.Attack:
                mainMenu.SetActive(false);
                attacksPanel.SetActive(true);
                skillPanel.SetActive(false);
                itemsPanel.SetActive(false);
                break;

            case BattleManager.RpgMenu.Skill:
                mainMenu.SetActive(false);
                attacksPanel.SetActive(false);
                skillPanel.SetActive(true);
                itemsPanel.SetActive(false);
                break;

            case BattleManager.RpgMenu.Item:
                mainMenu.SetActive(false);
                attacksPanel.SetActive(false);
                skillPanel.SetActive(false);
                itemsPanel.SetActive(true);
                break;

            case BattleManager.RpgMenu.SelectChar:
                mainMenu.SetActive(false);
                attacksPanel.SetActive(false);
                skillPanel.SetActive(false);
                itemsPanel.SetActive(false);
                break;

            case BattleManager.RpgMenu.None:
                mainMenu.SetActive(false);
                attacksPanel.SetActive(false);
                skillPanel.SetActive(false);
                itemsPanel.SetActive(false);
                break;
        }
    }


    public void UpdateMenuText(int playerTurn)
    {
        if (playerTurn > 3)
            return;

        //Attack Menu
        for (int i = 0; i < attackOptions.Length; i++)
        {
            if (i < gameMaster.playerParty[playerTurn].damageTypeUnlocks.Length)
            {
                if (gameMaster.playerParty[playerTurn].damageTypeUnlocks[i].levelToUnlock <= gameMaster.playerParty[playerTurn].charLevel)
                {
                    attackOptions[i].text = gameMaster.playerParty[playerTurn].damageTypeUnlocks[i].damageType.ToString();
                }
                else
                {
                    attackOptions[i].text = "";
                }
            }
            else
                attackOptions[i].text = "";
        }

        //Skill Menu
        for (int i = 0; i < skillOptions.Length; i++)
        {
            if (i < gameMaster.playerParty[playerTurn].skills.Length)
            {
                if (gameMaster.playerParty[playerTurn].skills[i].levelToUnlock <= gameMaster.playerParty[playerTurn].charLevel)
                {
                    skillOptions[i].text = gameMaster.playerParty[playerTurn].skills[i].skill.skillName;
                    skillCosts[i].text = gameMaster.playerParty[playerTurn].skills[i].skill.skillCost.ToString();
                }
                else
                {
                    skillOptions[i].text = "";
                    skillCosts[i].text = "";
                }
            }
            else
            {
                skillOptions[i].text = "";
                skillCosts[i].text = "";
            }
        }
    }

    public void HighlightMainOption(int index)
    {
        for (int i = 0; i < mainOptions.Length; i++)
        {
            if (i == index)
                mainOptions[i].color = new Color(1, 1, 1, mainOptions[i].color.a);
            else
                mainOptions[i].color = new Color(0.45f, 0.45f, 0.45f, mainOptions[i].color.a);
        }

        if (battleManager.noEscape)
        {
            mainOptions[3].color = new Color(0.25f, 0f, 0f, mainOptions[3].color.a);
            mainOptions[3].fontStyle = FontStyles.Bold;
        }
        else if (!battleManager.noEscape)
            mainOptions[3].fontStyle = FontStyles.Normal;
    }

    public void HighlightAttackOption(int index)
    {
        for (int i = 0; i < attackOptions.Length; i++)
        {
            if (i == index)
                attackOptions[i].color = new Color(1, 1, 1, attackOptions[i].color.a);
            else
                attackOptions[i].color = new Color(0.45f, 0.45f, 0.45f, attackOptions[i].color.a);
        }
    }

    public void HighlightSkillOption(int index)
    {
        for (int i = 0; i < skillOptions.GetLength(0); i++)
        {
            if (i == index)
            {
                skillOptions[i].color = new Color(1, 1, 1, skillOptions[i].color.a);
                skillCosts[i].color = new Color(1, 1, 1, skillCosts[i].color.a);
            }
            else
            {
                skillOptions[i].color = new Color(0.45f, 0.45f, 0.45f, skillOptions[i].color.a);
                skillCosts[i].color = new Color(0.45f, 0.45f, 0.45f, skillCosts[i].color.a);
            }
        }
    }

    public bool FadeBlackscreen(bool fadeIn)
    {
        blackScreen.color = new Color(0, 0, 0, Mathf.Clamp01(blackScreen.color.a + (gameMaster.timeScale * ((fadeIn) ? 0.025f : -0.025f))));
        if ((fadeIn && blackScreen.color.a >= 1) || (!fadeIn && blackScreen.color.a <= 0))
            return true;
        else
            return false;
    }

    public void InstantBlackscreen(bool turnOn)
    {
        blackScreen.color = new Color(0, 0, 0, (turnOn) ? 1 : 0);
    }
}
