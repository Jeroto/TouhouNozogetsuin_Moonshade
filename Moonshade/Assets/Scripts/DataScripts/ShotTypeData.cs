using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerShotTypeData", menuName = "ShmupData/PlayerShotTypeData", order = 1)]
public class ShotTypeData : ScriptableObject
{
    public AudioClip mainShootSound;
    public AudioClip subShootSound;
    public int bursts;
    public PlayerShotData[] mainShots;
    public PlayerShotData[] subShots;
    [Space]
    public PlayerShotData[] mainFocusShots;
    public PlayerShotData[] subFocusShots;

    [System.Serializable]
    public struct PlayerShotData
    {
        public GameObject[] bullets;
        public float[] angles;
        public Vector2[] positionOffsets;
        public float fireWait;
    }
}
