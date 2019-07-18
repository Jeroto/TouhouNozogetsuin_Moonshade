using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OverworldUI : MonoBehaviour
{
    GameMasterScript gameMaster;
    Transform thisTrans;

    [SerializeField] int pauseFadeFrames;

    Transform pauseMenuContainer;
    Transform characterPanelContainer;
    BasicCharacterPanel[] characterPanels;

    Image[] pauseMenuImages;
    float[] startingImageAlphas;
    TextMeshProUGUI[] pauseMenuText;
    float[] startingTextAlphas;

    int pauseFadePassed;

    bool prevPaused;

    struct BasicCharacterPanel
    {
        public Transform panel;
        public Image portrait;
        public TextMeshProUGUI charName;
        public TextMeshProUGUI charType;
        public TextMeshProUGUI charHp;
        public TextMeshProUGUI charMp;
        public TextMeshProUGUI charLvl;
        public TextMeshProUGUI charToNext;
        public TextMeshProUGUI charTotalExp;
        public TextMeshProUGUI charStatus;
    }

    private void Awake()
    {
        thisTrans = transform;
        gameMaster = GameMasterScript.gameMaster;
        pauseMenuContainer = thisTrans.GetChild(0);
        pauseMenuContainer.gameObject.SetActive(true);
        characterPanelContainer = pauseMenuContainer.Find("Characters");
        characterPanels = new BasicCharacterPanel[4];
        for (int i = 0; i < 4; i++)
        {
            characterPanels[i].panel = characterPanelContainer.GetChild(i);
            characterPanels[i].portrait = characterPanels[i].panel.GetChild(0).GetComponent<Image>();
            characterPanels[i].charName = characterPanels[i].panel.GetChild(1).GetComponent<TextMeshProUGUI>();
            characterPanels[i].charType = characterPanels[i].panel.GetChild(2).GetComponent<TextMeshProUGUI>();
            characterPanels[i].charHp = characterPanels[i].panel.GetChild(3).GetComponent<TextMeshProUGUI>();
            characterPanels[i].charMp = characterPanels[i].panel.GetChild(4).GetComponent<TextMeshProUGUI>();
            characterPanels[i].charLvl = characterPanels[i].panel.GetChild(5).GetComponent<TextMeshProUGUI>();
            characterPanels[i].charToNext = characterPanels[i].panel.GetChild(6).GetComponent<TextMeshProUGUI>();
            characterPanels[i].charTotalExp = characterPanels[i].panel.GetChild(7).GetComponent<TextMeshProUGUI>();
            characterPanels[i].charStatus = characterPanels[i].panel.GetChild(8).GetComponent<TextMeshProUGUI>();
        }

        pauseMenuImages = transform.GetChild(0).GetComponentsInChildren<Image>();
        pauseMenuText = transform.GetChild(0).GetComponentsInChildren<TextMeshProUGUI>();
        StorePauseScreenAlphas();
        HidePauseMenu();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(prevPaused != gameMaster.pause)
        {
            UpdateCharacterStatus();
            StartCoroutine(FadePauseScreen(gameMaster.pause));
        }
        prevPaused = gameMaster.pause;
    }

    public void UpdateCharacterStatus()
    {
        for (int i = 0; i < 4; i++)
        {
            if (gameMaster.playerParty[i].charName != "na")
            {
                characterPanels[i].panel.gameObject.SetActive(true);
                characterPanels[i].portrait.sprite = gameMaster.playerParty[i].portrait;
                characterPanels[i].charName.text = gameMaster.playerParty[i].charName;
                characterPanels[i].charType.text = "Type: " + gameMaster.playerParty[i].charType + " " + gameMaster.playerParty[i].charSubtype;
                characterPanels[i].charHp.text = "HP: " + gameMaster.playerParty[i].currentHP.ToString() + "/" + gameMaster.playerParty[i].maxHP.ToString();
                characterPanels[i].charMp.text = "MP: " + gameMaster.playerParty[i].currentMP.ToString() + "/" + gameMaster.playerParty[i].maxMP.ToString();
                characterPanels[i].charLvl.text = "Lvl: " + gameMaster.playerParty[i].charLevel.ToString();
                characterPanels[i].charToNext.text = "To Next: " + (gameMaster.CalculateTotalExpNeeded(gameMaster.playerParty[i].charLevel) - gameMaster.playerParty[i].charExp).ToString();
                characterPanels[i].charTotalExp.text = "Total Exp: " + gameMaster.playerParty[i].charExp.ToString();
                if(gameMaster.playerParty[i].statusEffects.Count == 0)
                    characterPanels[i].charStatus.text = "Status: Normal";
                else
                {
                    string statusString = "Status: ";
                    for (int k = 0; k < gameMaster.playerParty[i].statusEffects.Count; k++)
                    {
                        statusString += gameMaster.playerParty[i].statusEffects[k].effectType;
                        if (k != gameMaster.playerParty[i].statusEffects.Count - 1)
                            statusString += ", ";
                    }
                    characterPanels[i].charStatus.text = statusString;
                }
            }
            else
                characterPanels[i].panel.gameObject.SetActive(false);
        }
    }

    void StorePauseScreenAlphas()
    {
        startingImageAlphas = new float[pauseMenuImages.Length];
        for (int i = 0; i < pauseMenuImages.Length; i++)
        {
            startingImageAlphas[i] = pauseMenuImages[i].color.a;
        }
        startingTextAlphas = new float[pauseMenuText.Length];
        for (int i = 0; i < pauseMenuText.Length; i++)
        {
            startingTextAlphas[i] = pauseMenuText[i].color.a;
        }
    }

    IEnumerator FadePauseScreen(bool inOrOut)
    {
        Color currrentColor;
        while (pauseFadePassed < pauseFadeFrames)
        {
            pauseFadePassed++;
            for (int i = 0; i < pauseMenuImages.Length; i++)
            {
                currrentColor = pauseMenuImages[i].color;
                if(inOrOut)
                    currrentColor.a = Mathf.Lerp(0, startingImageAlphas[i], (float)pauseFadePassed / pauseFadeFrames);
                else
                    currrentColor.a = Mathf.Lerp(startingImageAlphas[i], 0, (float)pauseFadePassed / pauseFadeFrames);
                pauseMenuImages[i].color = currrentColor;
            }
            for (int i = 0; i < pauseMenuText.Length; i++)
            {
                currrentColor = pauseMenuText[i].color;
                if (inOrOut)
                    currrentColor.a = Mathf.Lerp(0, startingTextAlphas[i], (float)pauseFadePassed / pauseFadeFrames);
                else
                    currrentColor.a = Mathf.Lerp(startingTextAlphas[i], 0, (float)pauseFadePassed / pauseFadeFrames);
                pauseMenuText[i].color = currrentColor;
            }
            yield return null;
        }
        pauseFadePassed = 0;
    }

    void HidePauseMenu()
    {
        Color currrentColor;
        for (int i = 0; i < pauseMenuImages.Length; i++)
        {
            currrentColor = pauseMenuImages[i].color;
            currrentColor.a = 0;
            pauseMenuImages[i].color = currrentColor;
        }
        for (int i = 0; i < pauseMenuText.Length; i++)
        {
            currrentColor = pauseMenuText[i].color;
            currrentColor.a = 0;
            pauseMenuText[i].color = currrentColor;
        }
    }
}
