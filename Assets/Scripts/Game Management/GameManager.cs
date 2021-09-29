using System;
using System.Collections;
using System.Collections.Generic;
using Cameras;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls all major game events and coordinates classes.
/// Phases - A turn consists of two Phases, Planning Phase and Action Phase.
/// During Planning Phase, each player draws card(s), gives Movement Orders to their Mice, and can play new Mice from their Hand.
/// During the Action Phase, movement orders are played out and Tussles occur according to the Attack Radius of the mice in play. 
/// </summary>
public class GameManager : Singleton<GameManager>
{
    private CameraManager cameraManager;
    private AudioManager audioManager;
    private Announcer announcer;
    [Header("Camera Setup")]
    public Camera mainCam;
    public int baseCullingMask;

    [Header("Phases")]
    //overall phase counter
    public int phaseCounter = 0;

    public int drawAtStart = 3;
    public int drawAmount = 2;
    
    //phases
    public Phase currentGamePhase;
    public enum Phase
    {
        TUTORIAL, PLANNING, ACTIVE, GAMEOVER,
    }

    public GameCamera actionPhaseCamera;
    public float actionTime = 10f;
    public float actionTimer;
    public FadeUI actionPhaseText;

    [Header("Player Setup")]
    public GamePlayer playerOne;
    public GamePlayer playerTwo;

    public GamePlayer currentPlayer;
    private MouseController mouseController;

    [Header("Events")]
    public UnityEvent changedPhases;
    public UnityEvent playerOnePlanning;
    public UnityEvent playerTwoPlanning;
    public UnityEvent actionPhaseBegin;
    public UnityEvent actionPhaseEnd;
    public UnityEvent changedPlayers;
    public UnityEvent gameOver;

    [Header("Effect Prefabs")]
    public GameObject tussleCloudPrefab;
    
    [Header("Game Over")]
    public GameObject playerOneWinsUI;
    public GameObject playerTwoWinsUI;
    public AudioClip winSound;
    public AudioClip lossSound;

    [Header("Tutorial Stuff")] 
    public GameObject jerryObj;
    public MonologueManager jerryTutorialManager;
    public GameCamera jerryCamera;
    public FadeUI pressEnter;
    public bool tutorialEnded;
    
    [Header("Audience Stuff")]
    public MonologueManager audienceMonologues;
    public CreatureAnimation audienceAnimation;

    private float escapeTimer = 0f;
    
    
    void Awake()
    {
        mainCam = Camera.main;
        baseCullingMask = mainCam.cullingMask;
        mouseController = FindObjectOfType<MouseController>();
        cameraManager = FindObjectOfType<CameraManager>();
        audioManager = FindObjectOfType<AudioManager>();
        announcer = FindObjectOfType<Announcer>();
        playerTwo.DeactivatePlayer();
        playerOne.alcololMeter.gameObject.SetActive(false);
        currentPlayer = null;
        playerOne.isFirstPlayer = true;
        SetGamePhase(Phase.TUTORIAL);
    }
    
    private void OnDisable()
    {
        //remove all event listeners
        changedPhases.RemoveAllListeners();
        changedPlayers.RemoveAllListeners();
        playerOnePlanning.RemoveAllListeners();
        playerTwoPlanning.RemoveAllListeners();
        actionPhaseBegin.RemoveAllListeners();
        actionPhaseEnd.RemoveAllListeners();
        gameOver.RemoveAllListeners();
    }

    public CameraManager GetCameraManager()
    {
        return cameraManager;
    }

    public AudioManager GetAudioManager()
    {
        return audioManager;
    }

    public Announcer GetAnnouncer()
    {
        return announcer;
    }

    void Update()
    {
        //when the player hits return/enter
        if (Input.GetKeyDown(KeyCode.Return) )
        {
            //in tutorial
            if (currentGamePhase == Phase.TUTORIAL)
            {
                EndTutorial();
            }
            
            //planning?
            else if (currentGamePhase == Phase.PLANNING)
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
            
            //its game over
            else if (currentGamePhase == Phase.GAMEOVER)
            {
                //restart?
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
        
        //action phase.
        if (currentGamePhase == Phase.ACTIVE)
        {
            actionTimer -= Time.deltaTime;

            if (actionTimer < 0)
            {
                actionPhaseEnd.Invoke();
                SetGamePhase(Phase.PLANNING);
            }
        }

        //hold escape to quit game 
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escapeTimer = 0f;
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            escapeTimer += Time.deltaTime;

            if (escapeTimer > 1f)
            {
                Application.Quit();
            }
        }
    }

    #region Phase Logic
    public void SetGamePhase(Phase nextPhase)
    {
        currentGamePhase = nextPhase;

        //for anyone listening!
        changedPhases.Invoke();
        
        //player started tutorial 
        if (currentGamePhase == Phase.TUTORIAL)
        {
            StartTutorial();
        }
        
        //if planning, start with player 1 
        if (currentGamePhase == Phase.PLANNING)
        {
            SetPlanningPhase();
        }
        
        //if action, deactivate player 2 and set action camera & timer
        if (currentGamePhase == Phase.ACTIVE)
        {
            SetActivePhase();
        }
        
        //if gameover
        if (currentGamePhase == Phase.GAMEOVER)
        {
            //anything?
            gameOver.Invoke();
        }
    }
    
    void StartTutorial()
    {
        //enable jerry
        jerryObj.SetActive(true);
        //start his mono
        jerryTutorialManager.SetMonologueSystem(0);
        jerryTutorialManager.EnableMonologue();
        //enable camera for him 
        cameraManager.Set(jerryCamera);
        //press enter text
        pressEnter.FadeIn();
    }

    public void EndTutorial()
    {
        if (tutorialEnded)
        {
            return;
        }
        
        //start first planning phase!
        SetGamePhase(Phase.PLANNING);
        //stop monologue
        jerryTutorialManager.DisableMonologue();   
        //press enter text
        pressEnter.FadeOut();

        //bool
        tutorialEnded = true;
    }

    void SetPlanningPhase()
    {
        //increment phase counter on each planning phase
        phaseCounter++;
        //audio
        audioManager.planningPhases.TransitionTo(1f);
        //set player 1 turn
        SetPlayerTurn(playerOne);
    }

    void SetActivePhase()
    {
        currentPlayer.DeactivatePlayer(); 
        cameraManager.Set(actionPhaseCamera);
        actionTimer = actionTime;
        actionPhaseText.FadeIn();
        actionPhaseBegin.Invoke();
        //audio
        audioManager.actionPhases.TransitionTo(1f);
        //show announcement button
        announcer.activationButton.SetActive(true);
    }

    #endregion
   

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

        //which player is it?
        if (nextPlayer == playerOne)
        {
            playerOnePlanning.Invoke();
        }
        else if (nextPlayer == playerTwo)
        {
            playerTwoPlanning.Invoke();
        }

        //set mouse controller ref to 3d cursor 
        mouseController.threeDCursor = nextPlayer.cursorObj;

        //set new current player
        currentPlayer = nextPlayer;
        
        //invoke changed player event
        changedPlayers.Invoke();
    }

    #region Game Over Logic

    public void GameOver(GamePlayer loser)
    {
        SetGamePhase(Phase.GAMEOVER);

        //player 2 wins -- I lose
        if (loser == playerOne)
        {
            PlayerTwoWins();
        }
        //player 1 wins -- I win
        else if (loser == playerTwo)
        {
            PlayerOneWins();
        }
    }

    void PlayerOneWins()
    {
        playerOneWinsUI.SetActive(true);
            
        //say pickachu Victory mono
        playerOne.playerMonologueManager.SetMonologueSystem(4);
        playerOne.playerMonologueManager.EnableMonologue();
            
        //say mickey Loss mono
        playerTwo.playerMonologueManager.SetMonologueSystem(5);
        playerTwo.playerMonologueManager.EnableMonologue();
            
        //play win sound
        //audioManager.audioMainSource.PlayOneShot(winSound);
    }

    void PlayerTwoWins()
    {
        //player 2 wins!!
        playerTwoWinsUI.SetActive(true);
            
        //say mickey Victory mono
        playerTwo.playerMonologueManager.SetMonologueSystem(4);
        playerTwo.playerMonologueManager.EnableMonologue();
            
        //say pickachu Loss mono
        playerOne.playerMonologueManager.SetMonologueSystem(5);
        playerOne.playerMonologueManager.EnableMonologue();
            
        //play loss sound
        //audioManager.audioMainSource.PlayOneShot(lossSound);
    }

    #endregion
   
    public void EnableAudienceMonologue(int index)
    {
        if (audienceMonologues.inMonologue)
        {
            return;
        }
        
        audienceMonologues.SetMonologueSystem(index);
        audienceMonologues.EnableMonologue();
    }

    
}

