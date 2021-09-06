using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioMainSource;
    public AudioMixer mainMixer;

    public AudioMixerSnapshot planningPhases;
    public AudioMixerSnapshot actionPhases;

    private void Start()
    {
        if (audioMainSource == null)
        {
            audioMainSource = GetComponent<AudioSource>();
        }
    }
}
