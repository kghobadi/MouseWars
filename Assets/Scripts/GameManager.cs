﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Controls all major game events and coordinates classes.
/// Phases - A turn consists of two Phases, Planning Phase and Action Phase.
/// During Planning Phase, each player draws card(s), gives Movement Orders to their Mice, and can play new Mice from their Hand.
/// During the Action Phase, movement orders are played out and Tussles occur according to the Attack Radius of the mice in play. 
/// </summary>
public class GameManager : Singleton<GameManager>
{
    //phases
    public Phase currentGamePhase;
    public enum Phase
    {
        PLANNING, ACTIVE,
    }

    //players
    public GamePlayer playerOne;
    public GamePlayer playerTwo;

    public GamePlayer currentPlayer;
    private MouseController mouseController;

    //events
    public UnityEvent changedPhases;
    public UnityEvent changedPlayers;

    //for creature tussles 
    public GameObject tussleCloudPrefab;
    
    void Start()
    {
        mouseController = FindObjectOfType<MouseController>();
        playerTwo.DeactivatePlayer();
        currentPlayer = null;
        SetGamePhase(Phase.PLANNING);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            //planning?
            if (currentGamePhase == Phase.PLANNING)
            {
                //end player one planning
                if (currentPlayer == playerOne)
                {
                    SetPlayerTurn(playerTwo);
                }
                //end planning, start active phase 
                else
                {
                    SetGamePhase(Phase.ACTIVE);
                }
            }

            //set back to planning when in active
            else if (currentGamePhase == Phase.ACTIVE)
            {
                SetGamePhase(Phase.PLANNING);
            }
        }
    }

    public void SetGamePhase(Phase nextPhase)
    {
        currentGamePhase = nextPhase;

        //for anyone listening!
        changedPhases.Invoke();
        
        //if planning, start with player 1 
        if (currentGamePhase == Phase.PLANNING)
        {
            SetPlayerTurn(playerOne);
        }
    }

    void SetPlayerTurn(GamePlayer nextPlayer)
    {
        //disable current player setup
        if (currentPlayer != null)
        {
            currentPlayer.DeactivatePlayer();
            
            //make cam look at other player's body
            nextPlayer.cameraObj.transform.LookAt(currentPlayer.bodyObj.transform);
        }
        
        //enable nextplayer setup
        nextPlayer.ActivatePlayer();

        //set mouse controller ref to 3d cursor 
        mouseController.threeDCursor = nextPlayer.cursorObj;

        //set new current player
        currentPlayer = nextPlayer;
        
        //invoke changed player event
        changedPlayers.Invoke();
    }

    private void OnDisable()
    {
        changedPhases.RemoveAllListeners();
        changedPlayers.RemoveAllListeners();
    }
}

[System.Serializable]
public class GamePlayer
{
    public GameObject cameraObj;
    public MouseLook camLook;
    public GameObject cursorObj;
    public PlayerHand playerHand;
    public GameObject bodyObj;

    public void ActivatePlayer()
    {
        cameraObj.SetActive(true);
        cursorObj.SetActive(true);
        bodyObj.SetActive(false);
    }

    public void DeactivatePlayer()
    {
        cameraObj.SetActive(false);
        cursorObj.SetActive(false);
        bodyObj.SetActive(true);
    }
}
