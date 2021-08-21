using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls all major game events and coordinates classes.
/// Phases - A turn consists of two Phases, Planning Phase and Action Phase.
/// During Planning Phase, each player draws card(s), gives Movement Orders to their Mice, and can play new Mice from their Hand.
/// During the Action Phase, movement orders are played out and Tussles occur according to the Attack Radius of the mice in play. 
/// </summary>
public class GameManager : Singleton<GameManager>
{
    public Phase currentGamePhase;
    public enum Phase
    {
        PLANNING, ACTIVE,
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    
}
