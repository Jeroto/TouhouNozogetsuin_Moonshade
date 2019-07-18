using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PartyCharacterData", menuName = "CharacterData/PartyCharacterData", order = 1)]
public class PartyCharacterData : CharacterData
{
    public Character.CharType charType;
    public Character.CharSubtype charSubtype;

    public int movementSpeed;
    public int focusSpeed;

    public ShmupPlayerData shmupData;
    public ShotTypeData defaultShot;
}
