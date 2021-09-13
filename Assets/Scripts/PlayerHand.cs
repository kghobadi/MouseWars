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

    public CreatureCardItem activeCard;
    private Transform activeCardSpot;
    public Transform cardGazeLocation;
    public Color teamColor;
    public bool canHoldCard = true;
    public bool zFlipped;
    public float zSpawnAxis = -0.5f;

    [Header("Placement sounds")] 
    public AudioClip[] noAlcololSounds;
    public FadeUI noAlcololFade;
    
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
    }

    private void Update()
    {
        CheckActiveCard();
    }

    void CheckActiveCard()
    {
        if (activeCard)
        {
            //left click mouse to play card on table
            if (Input.GetMouseButtonDown(0))
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
                        Debug.Log("Wrong side of the board!");
                        //play bad sound
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
                        Debug.Log("Wrong side of the board!");
                        //play bad sound
                    }
                }
            }

            //right click mouse to return card to spot
            if (Input.GetMouseButtonDown(1))
            {
                activeCard.ReturnToSpot();
                activeCard = null;
                SetCanHold(true);
            }
            
            //hold space to examine active card
            if (Input.GetKey(KeyCode.Space))
            {
                activeCard.transform.position = cardGazeLocation.position;
            }
            //reset current gaze card location to hand location
            if (Input.GetKeyUp(KeyCode.Space))
            {
                activeCard.transform.localPosition = Vector3.zero;
            }
        }
    }

    void DeployActiveCard()
    {
        if (activeCard.CheckCanPlayCard())
        {
            activeCard.ActivateCard(transform.position, zFlipped);
            activeCard = null;
            SetCanHold(true);
        }
        else
        {
            noAlcololFade.FadeIn();
            Debug.Log("Not enough Alcolol!!!");
            //play bad sound
            PlayRandomSound(noAlcololSounds, 1f);
        }
    }
}
