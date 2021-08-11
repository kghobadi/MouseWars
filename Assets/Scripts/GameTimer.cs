using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Responsible for timing the game. 
/// </summary>
public class GameTimer : MonoBehaviour
{
    public bool timeOnStart;
    public float timer;
    public float startTime;
    public bool timing;

    [Header("Timing UI")] 
    public TMP_Text timerText;
    
    void Start()
    {
        if(timeOnStart)
            BeginTimer();
    }

    private void FixedUpdate()
    {
        //timing
        if (timing)
        {
            timer = Time.time - startTime;

            //set text 
            if (timerText)
            {
                timerText.text = timer.ToString();
            }
        }
    }

    public void BeginTimer()
    {
        startTime = Time.time;
        timing = true;
    }

    public void StopTimer()
    {
        timing = false;
    }
}
