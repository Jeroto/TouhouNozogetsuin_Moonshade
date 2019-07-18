using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PartyCharacter : Character
{
    public CharType charType;
    public CharSubtype charSubtype;
    
    public int movementSpeed;
    public int focusSpeed;

    public int possibleCharacterIndex;

    public ShmupPlayerData shmupData;
    public ShotTypeData shotTypeEquip;
    //Bomb equip

    public void GiveExperience(ulong expValue)
    {
        charExp += expValue;
        if (charExp > GameMasterScript.maxExp)
            charExp = GameMasterScript.maxExp;
    }
}
