using System;
using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource _effectAudioSource;

    public void PlaySound(AudioClip clip)
    {
        _effectAudioSource.PlayOneShot(clip);
    }
}
