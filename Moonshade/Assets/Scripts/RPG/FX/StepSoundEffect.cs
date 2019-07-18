using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepSoundEffect : MonoBehaviour
{
    AudioSource stepSource;

    [SerializeField] AudioClip[] grassSounds;
    [SerializeField] AudioClip[] stoneSounds;
    [SerializeField] AudioClip[] echoStoneSounds;
    
    public enum TileType {Grass, Stone, EchoingStone};

    public void Awake()
    {
        stepSource = GetComponent<AudioSource>();
    }

    public void PlayStep(TileType tileType)
    {
        switch(tileType)
        {
            case TileType.Grass:
                stepSource.clip = grassSounds[Random.Range(0, grassSounds.Length)];
                break;
        }
        stepSource.Play();
    }
}
