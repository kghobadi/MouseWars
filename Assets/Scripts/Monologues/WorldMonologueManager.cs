using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Monologues
{
    public string monoName;
    public Monologue monologue;
    public MonologueTrigger mTrigger; 
}

public class WorldMonologueManager : MonoBehaviour
{
    public Monologues[] allMonologues;
    public MonologueManager[] allMonoManagers;

    private void Awake()
    {
        if(allMonoManagers.Length == 0)
            allMonoManagers = FindObjectsOfType<MonologueManager>();
    }
}
