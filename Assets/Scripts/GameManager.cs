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
        PLANNING, ACTIVE, GAMEOVER,
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
    
    
    void Awake()
    {
        mainCam = Camera.main;
        baseCullingMask = mainCam.cullingMask;
        mouseController = FindObjectOfType<MouseController>();
        cameraManager = FindObjectOfType<CameraManager>();
        audioManager = FindObjectOfType<AudioManager>();
        playerTwo.DeactivatePlayer();
        currentPlayer = null;
        SetGamePhase(Phase.PLANNING);
    }

    void Update()
    {
        //when the player hits return/enter
        if (Input.GetKeyDown(KeyCode.Return) )
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
    }

    public void SetGamePhase(Phase nextPhase)
    {
        currentGamePhase = nextPhase;

        //for anyone listening!
        changedPhases.Invoke();
        
        //if planning, start with player 1 
        if (currentGamePhase == Phase.PLANNING)
        {
            //increment phase counter on each planning phase
            phaseCounter++;
            //set player 1 turn
            SetPlayerTurn(playerOne);
        }
        
        //if action, deactivate player 2 and set action camera & timer
        if (currentGamePhase == Phase.ACTIVE)
        {
            currentPlayer.DeactivatePlayer(); 
            cameraManager.Set(actionPhaseCamera);
            actionTimer = actionTime;
            actionPhaseText.FadeIn();
            actionPhaseBegin.Invoke();
        }
        
        //if gameover
        if (currentGamePhase == Phase.GAMEOVER)
        {
            //anything?
            gameOver.Invoke();
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

    public void GameOver(GamePlayer loser)
    {
        SetGamePhase(Phase.GAMEOVER);

        //player 2 wins -- I lose
        if (loser == playerOne)
        {
            playerTwoWinsUI.SetActive(true);
            
            //play loss sound
            //audioManager.audioMainSource.PlayOneShot(lossSound);
        }
        //player 1 wins -- I win
        else if (loser == playerTwo)
        {
            playerOneWinsUI.SetActive(true);
            
            //play win sound
            //audioManager.audioMainSource.PlayOneShot(winSound);
        }
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
}

[Serializable]
public class GamePlayer
{
    private CameraManager cameraManager;
    private GameManager gameManager;
    public GameCamera cameraObj;
    public MouseLook camLook;
    public GameObject cursorObj;
    public PlayerHand playerHand;
    public GameObject bodyObj;
    public int playerLayer;
    public int cardsToDraw;
    public MouseHole mouseHole;
    public FadeUI playerPlanningText;

    [Header("Alcohol Meter")]
    public int totalAlcolol;
    public int currentAlcolol;
    public AlcololMeter alcololMeter;

    private bool init;

    void Start()
    {
        Init();
    }

    void Init()
    {
        if (init)
            return;
        
        gameManager = GameManager.Instance;
        cameraManager =  GameObject.FindObjectOfType<CameraManager>();

        init = true;
    }
    
    public void ActivatePlayer()
    {
        Init();
        
        cameraManager.Set(cameraObj);
        cursorObj.SetActive(true);
        bodyObj.SetActive(false);
        
        //set player hand player ref to me!
        playerHand.myPlayer = this;

        //draw 3 cards on the first turn...
        if (GameManager.Instance.phaseCounter == 1)
        {
            //set cards to draw
            cardsToDraw = gameManager.drawAtStart;
            
            //mousehole placement
            mouseHole.BeginPlacement(this);
        }
        else
        {
            //set cards to draw
            cardsToDraw = gameManager.drawAmount;
        }
        
        //player text
        playerPlanningText.FadeIn();
        
        //set alcolol meter
        totalAlcolol++;
        currentAlcolol = totalAlcolol;
        alcololMeter.gameObject.SetActive(true);
        alcololMeter.SetAlcololAmount(currentAlcolol);

        //set camera layer mask 
        gameManager.mainCam.cullingMask = gameManager.baseCullingMask | (1 << playerLayer);
    }

    public void ReduceAlcolol(int amount)
    {
        currentAlcolol -= amount;
        alcololMeter.SetAlcololAmount(currentAlcolol);
    }

    public void DeactivatePlayer()
    {
        cursorObj.SetActive(false);
        bodyObj.SetActive(true);
        alcololMeter.gameObject.SetActive(false);
    }
}
