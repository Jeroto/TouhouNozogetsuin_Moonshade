using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerBombData", menuName = "ShmupData/PlayerBombData", order = 1)]
public class PlayerBombData : ScriptableObject
{
    public GameObject bombObject;
    public int bombTime;
    public int bombCost;
}
