using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioSourceExtensions
{
    public static AudioSource PlayClip2D(AudioClip clip, float volume, float pitch)
    {
        AudioSource newSource = new GameObject("OneShot2DAudio").AddComponent<AudioSource>();
        newSource.clip = clip;
        newSource.spatialBlend = 0;
        newSource.volume = volume;
        newSource.pitch = pitch;
        newSource.Play();
        GameObject.Destroy(newSource.gameObject, clip.length);
        return newSource;
    }

    public static AudioSource PlayClip2D(AudioClip clip, float volume)
    {
        return PlayClip2D(clip, volume, 1);
    }

    public static AudioSource PlayClip2D(AudioClip clip)
    {
        return PlayClip2D(clip, 1, 1);
    }
}
