using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterData), true), CanEditMultipleObjects]
public class EditorPartyCharacterData : Editor
{
    CharacterData targetScript;
    public override void OnInspectorGUI()
    {
        targetScript = (CharacterData)target;
        if(targetScript.portrait != null)
        {
            EditorGUI.DrawPreviewTexture(new Rect(new Vector2(0, 0), new Vector2(128, 128)), targetScript.portrait.texture);
            GUILayout.Space(128);
        }
        DrawDefaultInspector();
    }
}
