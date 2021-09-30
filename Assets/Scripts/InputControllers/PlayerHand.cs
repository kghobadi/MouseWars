using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls player's current hand of playable cards. 
/// </summary>
public class PlayerHand : AudioHandler
{
    private Camera mainCam;
    
    [Header("Player Hand Settings")]
    public Deck myDeck;
    public GamePlayer myPlayer;
    public CardSpot[] cardSpots;
    public CreatureAnimation playerHandPose;

    public CreatureCardItem activeCard;
    private Transform activeCardSpot;
    public Transform cardGazeLocation;
    public Color teamColor;
    public bool canHoldCard = true;
    public bool zFlipped;
    public float zSpawnAxis = -0.5f;
    public float mouseCheckRadius = 2f;

    private float timeBeforePlay = 0f;
    [Tooltip("Timer after selecting a card to prevent from immediate board placement")]
    public float timeNecToPlay = 0.1f;

    [Header("Placement sounds")] 
    public AudioClip[] noAlcololSounds;
    public FadeUI noAlcololFade;
    public FadeUI miceTooCloseFade;
    private void Start()
    {
        mainCam = Camera.main;
        activeCardSpot = transform.GetChild(0);
        canHoldCard = true;
    }

    public void SetCanHold(bool canHold)
    {
        canHoldCard = canHold;
    }

    /// <summary>
    /// Called by drawing a card from the Deck. 
    /// </summary>
    /// <param name="creatureB"></param>
    /// <param name="cCard"></param>
    public void AddCardToHand(CreatureCardItem creatureB, CreatureCard cCard)
    {
        CardSpot cardSpot = FindOpenCardSpot();

        if (cardSpot != null)
        {
            //move card to card spot and look at camera 
            creatureB.cardSpot = cardSpot;
            creatureB.transform.parent = cardSpot.spot;
            creatureB.transform.position = cardSpot.spot.position;
            creatureB.transform.LookAt(mainCam.transform);
            
            //set cardspot values
            cardSpot.creature = creatureB;
            cardSpot.card = cCard;
            cardSpot.occupied = true;
        }
        else
        {
            Debug.Log("no unoccupied card spots! returned card to deck...");
            
            ReturnCardToDeck(cCard);
        }
    }

    //returns an open card spot if there is one, otherwise null
    public CardSpot FindOpenCardSpot()
    {
        for (int i = 0; i < cardSpots.Length; i++)
        {
            if (cardSpots[i].occupied == false)
                return cardSpots[i];
        }

        return null;
    }

    public void ReturnCardToDeck(CreatureCard cardToReturn)
    {
        myDeck.ReturnCard(cardToReturn);
    }

    public void SelectActiveCard(CreatureCardItem card)
    {
        if (!canHoldCard)
            return;
        
        //return prev active card 
        if (activeCard)
        {
            activeCard.ReturnToSpot();
        }

        //move to cursor pos and parent, set default layer
        card.transform.parent = activeCardSpot;
        card.transform.localPosition = Vector3.zero;
        card.gameObject.layer = 0;
        
        //set it as active card 
        activeCard =  card;
        SetCanHold(false);
        timeBeforePlay = 0f;
        playerHandPose.SetAnimator("grabbing");
    }

    private void Update()
    {
        CheckActiveCard();
    }

    void CheckActiveCard()
    {
        if (activeCard)
        {
            //inc time before play
            timeBeforePlay += Time.deltaTime;
            
            //left click mouse to play card on table
            if (Input.GetMouseButtonDown(0) && timeBeforePlay > timeNecToPlay)
            {
                //check so we can only spawn on our side of the board.
                if (zFlipped)
                {
                    if (transform.position.z > zSpawnAxis)
                    {
                        DeployActiveCard();
                    }
                    else
                    {
                        WrongSideOfBoard();
                    }
                }
                else
                {
                    if (transform.position.z < zSpawnAxis)
                    {
                        DeployActiveCard();
                    }
                    else
                    {   
                        WrongSideOfBoard();
                    }
                }
            }

            //right click mouse to return card to spot
            if (Input.GetMouseButtonDown(1))
            {
                PutCardBack();
            }
            
            //hold space to examine active card
            if (Input.GetKey(KeyCode.Space))
            {
                ExamineCard();
            }

            //reset current gaze card location to hand location
            if (Input.GetKeyUp(KeyCode.Space))
            {
                StopExamine();
            }
        }
    }

    void PutCardBack()
    {
        activeCard.ReturnToSpot();
        activeCard = null;
        SetCanHold(true);
        playerHandPose.SetAnimator("idle");
    }

    void ExamineCard()
    {
        activeCard.transform.position = cardGazeLocation.position;
        activeCard.transform.LookAt(mainCam.transform);
        activeCard.transform.SetParent(mainCam.transform);
    }

    void StopExamine()
    {
        activeCard.transform.parent = activeCardSpot;
        activeCard.transform.localPosition = Vector3.zero;
        activeCard.transform.LookAt(mainCam.transform);
    }

    bool CheckForNearbyMice()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, mouseCheckRadius);
        foreach (var hitCollider in hitColliders)
        {
            //OH NO A MOUSE IS NEARBY!
            if (hitCollider.gameObject.CompareTag("Creature"))
            {
                return false;
            }
        }

        //no nearby mice :-)
        return true;
    }
    
    void DeployActiveCard()
    {
        if (activeCard.CheckCanPlayCard())
        {
            if (CheckForNearbyMice())
            {
                PlayCardToBoard();
            }
            else
            {
                TooManyMice();
            }
        }
        else
        {
            NotEnoughAlcolol();
        }
    }

    void PlayCardToBoard()
    {
        activeCard.ActivateCard(transform.position, zFlipped);
        activeCard = null;
        SetCanHold(true);
        playerHandPose.SetAnimator("idle");
    }

    void WrongSideOfBoard()
    {
        Debug.Log("Wrong side of the board!");
        //play bad sound
        PlayRandomSound(noAlcololSounds, 1f);
    }

    void TooManyMice()
    {
        miceTooCloseFade.FadeIn();
        Debug.Log("Too many mice near that spot!");
        //play bad sound
        PlayRandomSound(noAlcololSounds, 1f);
    }

    void NotEnoughAlcolol()
    {
        noAlcololFade.FadeIn();
        Debug.Log("Not enough Alcolol!!!");
        //play bad sound
        PlayRandomSound(noAlcololSounds, 1f);
    }
    
}
