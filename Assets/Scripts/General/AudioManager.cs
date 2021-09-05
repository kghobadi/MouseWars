using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource audioMainSource;

    private void Start()
    {
        if (audioMainSource == null)
        {
            audioMainSource = GetComponent<AudioSource>();
        }
    }
}
