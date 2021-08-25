using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls player's current hand of playable cards. 
/// </summary>
public class PlayerHand : MonoBehaviour
{
    private Camera mainCam;
    
    public Deck myDeck;
    public GamePlayer myPlayer;
    public CardSpot[] cardSpots;

    public CreatureCardItem activeCard;
    private Transform activeCardSpot;
    public Transform cardGazeLocation;
    public bool canHoldCard = true;
    public bool zFlipped;
    
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
    }

    private void Update()
    {
        if (activeCard)
        {
            //left click mouse to play card on table
            if (Input.GetMouseButtonDown(0))
            {
                DeployActiveCard();
            }

            //right click mouse to look at it up close
            if (Input.GetMouseButton(1) && activeCard)
            {
                activeCard.transform.position = cardGazeLocation.position;
            }
            //reset current gaze card location to hand location
            if (Input.GetMouseButtonUp(1) && activeCard)
            {
                activeCard.transform.localPosition = Vector3.zero;
            }
        }
    }

    void DeployActiveCard()
    {
        activeCard.ActivateCard(transform.position, zFlipped);
        activeCard = null;
    }
}
