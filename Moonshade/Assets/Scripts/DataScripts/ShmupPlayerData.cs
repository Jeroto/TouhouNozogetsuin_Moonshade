using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShmupPlayerData", menuName = "ShmupData/ShmupPlayerData", order = 1)]
public class ShmupPlayerData : ScriptableObject
{
    public float hitboxSize = 1;
    public int borderOfLife;

    public float moveSpeed;
    public float focusSpeed;

    public RuntimeAnimatorController animations;
    public GameObject[] extraBits;
}
