using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    //Dialogue data here
    public DialogueLanguageOption[] dialogueLines;
    [Space]
    [Space]
    [SerializeField] DialogueLineSettings[] textSettings;
    [Space]
    [Space]
    [SerializeField] PortraitMovements[] portraitMovements;
    [Space]
    [Space]

    public TextMeshPro dialogueText;
    TMP_TextInfo textInfo;
    public AnimationCurve bounceCurve;
    public AnimationCurve scaleCurve;

    [Space]

    public GameObject portraitSpawn;
    public Transform[] portraits;
    public Image[] portraitSprites;
    public Vector2[] portraitPosition;
    Vector2[] portraitSmoothing;

    public enum EffectType {None, Bounce, Shake, WindWave, Dance, ScaleWave};
    public enum ColorType {Color, Flash, Wave, VertWave};
    public enum PortraitExpression {General, Smile, Laugh, Frown, Disappointed, Angry, Flustered, Sad, Crying, AngryCry, Sigh, Drained};

    int textTimer;
    int textIndex = -1;
    int basicsIndex;

    int lineIndex;

    GameMasterScript gameMaster;
    InputScript input;
    int languageSetting;

    [System.Serializable]
    public struct DialogueLanguageOption
    {
        public string[] dialogue;
    }

    [System.Serializable]
    public struct DialogueLineSettings
    {
        [Space]
        [Space]
        public TextBasics[] textBasics;
        [Space]
        [Space]
        public TextEffects[] effectList;
        [Space]
        [Space]
        public ColorEffects[] colorList;
    }

    [System.Serializable]
    public struct TextBasics
    {
        public int textDelay;
        public AudioClip textSound;
        public int startIndex;
        public int endIndex;
    }

    [System.Serializable]
    public struct PortraitMovements
    {
        public PortraitExpression[] expression;
        public Vector2[] positions;
        public int partyChar;
        public Sprite[] portraits;
        public bool[] flipped;
        public float[] smoothSpeed;
        public float[] maxSpeed;
    }

    [System.Serializable]
    public struct TextEffects
    {
        public EffectType effectType;
        public float distance;
        public float speed;
        public int[] startIndex;
        public int[] endIndex;
    }

    [System.Serializable]
    public struct ColorEffects
    {
        public ColorType effectType;
        public Color32[] colors;
        public float speed;
        public int[] startIndex;
        public int[] endIndex;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameMaster = GameMasterScript.gameMaster;
        languageSetting = (int)gameMaster.languageSetting;
        input = GameMasterScript.inputScript;

        dialogueText = GetComponentInChildren<TextMeshPro>();
        dialogueText.enableWordWrapping = true;
        dialogueText.text = dialogueLines[languageSetting].dialogue[0];

        dialogueText.ForceMeshUpdate();
        textInfo = dialogueText.textInfo;
        Debug.Log(textInfo.characterCount);

        PortraitSetup();
    }

    private void Update()
    {
        languageSetting = (int)gameMaster.languageSetting;
        textTimer++;
        if (textIndex < textInfo.characterCount - 1 && textTimer >= textSettings[lineIndex].textBasics[basicsIndex].textDelay)
        {
            do
            {
                do
                    textIndex++;
                while (textInfo.characterInfo[textIndex].character == ' ');

                if (textSettings[lineIndex].textBasics[basicsIndex].endIndex < textIndex)
                    if (textSettings[lineIndex].textBasics.Length > basicsIndex + 1)
                        basicsIndex++;
            } while (textSettings[lineIndex].textBasics[basicsIndex].textDelay < 0);
            

            if(textSettings[lineIndex].textBasics[basicsIndex].textSound != null)
                AudioSourceExtensions.PlayClip2D(textSettings[lineIndex].textBasics[basicsIndex].textSound);
            textTimer = 0;
        }

        if(textIndex >= textInfo.characterCount - 1)
        {
            if(input.shootDown && lineIndex < dialogueLines[languageSetting].dialogue.Length - 1)
            {
                NextLine();
            }
        }

        PortraitProcessing();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        TextEffectProcessing();
        TextVisibility(textIndex);
    }

    void NextLine()
    {
        lineIndex++;
        textIndex = -1;
        textTimer = 0;
        basicsIndex = 0;

        dialogueText.text = dialogueLines[languageSetting].dialogue[lineIndex];

        PortraitUpdate();
    }

    void TextVisibility(int endVisibility)
    {
        Color32[] colors = dialogueText.mesh.colors32;
        int vertIndex;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            if (i > endVisibility)
            {
                if (textInfo.characterInfo[i].character == ' ')
                    continue;

                vertIndex = textInfo.characterInfo[i].vertexIndex;
                for (int k = 0; k < 4; k++)
                {
                    colors[vertIndex + k] = new Color32(0, 0, 0, 0);
                }
            }
        }
        dialogueText.mesh.colors32 = colors;
    }

    void PortraitSetup()
    {
        portraits = new Transform[portraitMovements.Length];
        portraitSprites = new Image[portraitMovements.Length];
        portraitPosition = new Vector2[portraitMovements.Length];
        portraitSmoothing = new Vector2[portraitPosition.Length];
        for (int i = 0; i < portraitMovements.Length; i++)
        {
            portraits[i] = Instantiate(portraitSpawn, dialogueText.transform.parent.parent).transform;
            portraits[i].localPosition = portraitMovements[i].positions[0];
            portraitPosition[i] = portraitMovements[i].positions[0];
            portraitSprites[i] = portraits[i].GetChild(0).GetComponent<Image>();
            portraitSprites[i].sprite = portraitMovements[i].portraits[0];
            portraits[i].localScale = new Vector3(portraitMovements[i].flipped[0] ? -1 : 1, 1, 1);
        }
    }

    void PortraitUpdate()
    {
        for (int i = 0; i < portraitMovements.Length; i++)
        {
            portraitPosition[i] = portraitMovements[i].positions[lineIndex];
            portraitSprites[i].sprite = portraitMovements[i].portraits[(int)portraitMovements[i].expression[lineIndex]];
            portraits[i].localScale = new Vector3(portraitMovements[i].flipped[lineIndex] ? -1 : 1, 1, 1);
        }
    }

    void PortraitProcessing()
    {
        for (int i = 0; i < portraitMovements.Length; i++)
        {
            portraits[i].localPosition = Vector2.SmoothDamp(portraits[i].localPosition, portraitPosition[i], ref portraitSmoothing[i],
                portraitMovements[i].smoothSpeed[lineIndex], portraitMovements[i].maxSpeed[lineIndex]);
        }
    }

    void TextEffectProcessing()
    {
        dialogueText.ForceMeshUpdate();
        Vector3[] vertPos;
        int vertIndex;
        vertPos = dialogueText.mesh.vertices;

        float scaleSize;

        for (int i = 0; i < textSettings[lineIndex].effectList.Length; i++)
        {
            switch(textSettings[lineIndex].effectList[i].effectType)
            {
                case EffectType.Bounce:
                    for (int a = textSettings[lineIndex].effectList[i].startIndex[languageSetting];
                        a < textSettings[lineIndex].effectList[i].endIndex[languageSetting] + 1; a++)
                    {
                        if (textInfo.characterInfo[a].character == ' ')
                            continue;

                        vertIndex = textInfo.characterInfo[a].vertexIndex;
                        for (int b = 0; b < 4; b++)
                        {
                            vertPos[vertIndex + b].y += bounceCurve.Evaluate(Mathf.Repeat(a / 5f + (Time.time * textSettings[lineIndex].effectList[i].speed), 1))
                                * textSettings[lineIndex].effectList[i].distance;
                        }
                    }
                    break;

                case EffectType.Shake:
                    Vector3 newOffset;
                    for (int a = textSettings[lineIndex].effectList[i].startIndex[languageSetting];
                        a < textSettings[lineIndex].effectList[i].endIndex[languageSetting] + 1; a++)
                    {
                        if (textInfo.characterInfo[a].character == ' ')
                            continue;

                        vertIndex = textInfo.characterInfo[a].vertexIndex;
                        newOffset = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)).normalized * textSettings[lineIndex].effectList[i].distance;
                        
                        for (int b = 0; b < 4; b++)
                        {
                            vertPos[vertIndex + b] += newOffset;
                        }
                    }
                    break;

                case EffectType.WindWave:
                    for (int a = textSettings[lineIndex].effectList[i].startIndex[languageSetting];
                        a < textSettings[lineIndex].effectList[i].endIndex[languageSetting] + 1; a++)
                    {
                        if (textInfo.characterInfo[a].character == ' ')
                            continue;

                        vertIndex = textInfo.characterInfo[a].vertexIndex;
                        
                        for (int b = 0; b < 2; b += 2)
                        {
                            scaleSize = scaleCurve.Evaluate(Mathf.Repeat((a) / 5f + (Time.time * textSettings[lineIndex].effectList[i].speed), 1))
                                * textSettings[lineIndex].effectList[i].distance;
                            vertPos[vertIndex + b].x += scaleSize * Mathf.Sign(b - 1);
                            vertPos[vertIndex + b].y += scaleSize * Mathf.Sign(b - 1);
                            vertPos[vertIndex + b + 1].x += scaleSize * Mathf.Sign(b - 1);
                            vertPos[vertIndex + b + 1].y += scaleSize * Mathf.Sign(b - 1);
                        }
                    }
                    break;

                case EffectType.Dance:
                    for (int a = textSettings[lineIndex].effectList[i].startIndex[languageSetting];
                        a < textSettings[lineIndex].effectList[i].endIndex[languageSetting] + 1; a++)
                    {
                        if (textInfo.characterInfo[a].character == ' ')
                            continue;

                        vertIndex = textInfo.characterInfo[a].vertexIndex;
                        
                        for (int b = 0; b < 4; b++)
                        {
                            scaleSize = scaleCurve.Evaluate(Mathf.Repeat((b) / 5f + (Time.time * textSettings[lineIndex].effectList[i].speed), 1))
                                * textSettings[lineIndex].effectList[i].distance;
                            vertPos[vertIndex + b].x += scaleSize * Mathf.Sign(b - 1);
                            vertPos[vertIndex + b].y += scaleSize * Mathf.Sign(b - 1);
                        }
                    }
                    break;

                case EffectType.ScaleWave:
                    for (int a = textSettings[lineIndex].effectList[i].startIndex[languageSetting];
                        a < textSettings[lineIndex].effectList[i].endIndex[languageSetting] + 1; a++)
                    {
                        if (textInfo.characterInfo[a].character == ' ')
                            continue;

                        vertIndex = textInfo.characterInfo[a].vertexIndex;

                        for (int b = 0; b < 4; b++)
                        {
                            scaleSize = scaleCurve.Evaluate(Mathf.Repeat((b) / 5f + (Time.time * textSettings[lineIndex].effectList[i].speed), 1))
                                * textSettings[lineIndex].effectList[i].distance;
                            vertPos[vertIndex + b].x += scaleSize * Mathf.Sign(b - 1);
                            vertPos[vertIndex + b].y += scaleSize * Mathf.Sign(b - 1);
                        }
                    }
                    break;
            }
        }

        dialogueText.mesh.vertices = vertPos;

        Color32[] colors = dialogueText.mesh.colors32;
        float colorValue = 0;
        for (int i = 0; i < textSettings[lineIndex].colorList.Length; i++)
        {
            switch(textSettings[lineIndex].colorList[i].effectType)
            {
                case ColorType.Color:
                    for (int a = textSettings[lineIndex].colorList[i].startIndex[languageSetting];
                        a < textSettings[lineIndex].colorList[i].endIndex[languageSetting] + 1; a++)
                    {
                        if (textInfo.characterInfo[a].character == ' ')
                            continue;

                        vertIndex = textInfo.characterInfo[a].vertexIndex;

                        for (int b = 0; b < 4; b++)
                        {
                            colors[vertIndex + b] = textSettings[lineIndex].colorList[i].colors[Mathf.Clamp(b, 0, textSettings[lineIndex].colorList[i].colors.Length - 1)];
                        }
                    }
                    break;

                case ColorType.Flash:
                    for (int a = textSettings[lineIndex].colorList[i].startIndex[languageSetting];
                        a < textSettings[lineIndex].colorList[i].endIndex[languageSetting] + 1; a++)
                    {
                        if (textInfo.characterInfo[a].character == ' ')
                            continue;

                        vertIndex = textInfo.characterInfo[a].vertexIndex;
                        colorValue = bounceCurve.Evaluate(Mathf.Repeat(Time.time * textSettings[lineIndex].colorList[i].speed, 1));
                        for (int b = 0; b < 4; b++)
                        {
                            colors[vertIndex + b] = Color32.Lerp(textSettings[lineIndex].colorList[i].colors[0], textSettings[lineIndex].colorList[i].colors[1], colorValue);
                        }
                    }
                    break;

                case ColorType.Wave:
                    for (int a = textSettings[lineIndex].colorList[i].startIndex[languageSetting];
                        a < textSettings[lineIndex].colorList[i].endIndex[languageSetting] + 1; a++)
                    {
                        if (textInfo.characterInfo[a].character == ' ')
                            continue;

                        vertIndex = textInfo.characterInfo[a].vertexIndex;
                        colorValue = bounceCurve.Evaluate(Mathf.Repeat(a / 5f + (Time.time * textSettings[lineIndex].colorList[i].speed), 1));
                        for (int b = 0; b < 4; b++)
                        {
                            colors[vertIndex + b] = Color32.Lerp(textSettings[lineIndex].colorList[i].colors[0], textSettings[lineIndex].colorList[i].colors[1], colorValue);
                        }
                    }
                    break;

                case ColorType.VertWave:
                    for (int a = textSettings[lineIndex].colorList[i].startIndex[languageSetting];
                        a < textSettings[lineIndex].colorList[i].endIndex[languageSetting] + 1; a++)
                    {
                        if (textInfo.characterInfo[a].character == ' ')
                            continue;

                        vertIndex = textInfo.characterInfo[a].vertexIndex;

                        colorValue = bounceCurve.Evaluate(Mathf.Repeat(a / 5f + (Time.time * textSettings[lineIndex].colorList[i].speed), 1));
                        colors[vertIndex] = Color32.Lerp(textSettings[lineIndex].colorList[i].colors[0], textSettings[lineIndex].colorList[i].colors[1], colorValue);
                        colors[vertIndex + 1] = Color32.Lerp(textSettings[lineIndex].colorList[i].colors[0], textSettings[lineIndex].colorList[i].colors[1], colorValue);
                        
                        colorValue = bounceCurve.Evaluate(Mathf.Repeat(1 - Mathf.Repeat((a + 1) / 5f + (Time.time * textSettings[lineIndex].colorList[i].speed), 1), 1));
                        colors[vertIndex + 2] = Color32.Lerp(textSettings[lineIndex].colorList[i].colors[0], textSettings[lineIndex].colorList[i].colors[1], colorValue);
                        colors[vertIndex + 3] = Color32.Lerp(textSettings[lineIndex].colorList[i].colors[0], textSettings[lineIndex].colorList[i].colors[1], colorValue);
                    }
                    break;
            }
        }

        dialogueText.mesh.colors32 = colors;
    }
}
