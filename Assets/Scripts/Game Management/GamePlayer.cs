
using System;
using Cameras;
using UnityEngine;

[Serializable]
public class GamePlayer
{
    private CameraManager cameraManager;
    private GameManager gameManager;
    public bool isFirstPlayer;
    public GameCamera cameraObj;
    public MouseLook camLook;
    public GameObject cursorObj;
    public PlayerHand playerHand;
    public Deck playerDeck;
    public GameObject bodyObj;
    public int playerLayer;
    public int cardsToDraw;
    public MouseHole mouseHole;
    public FadeUI playerPlanningText;
    public MonologueManager playerMonologueManager;

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
            
            //say opener
            playerMonologueManager.SetMonologueSystem(0);
            playerMonologueManager.EnableMonologue();
        }
        else
        {
            //set cards to draw
            cardsToDraw = gameManager.drawAmount;
            
            //say change turn
            playerMonologueManager.SetMonologueSystem(1);
            playerMonologueManager.EnableMonologue();
        }
        
        //set deck draw text
        playerDeck.SetDrawCardsText(cardsToDraw);
        
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