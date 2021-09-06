using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    private MouseController _mouseController;
    
    public List<CreatureCard> CreatureCards = new List<CreatureCard>();
    public bool shuffleOnStart;
    public GameObject cardPrefab;

    public PlayerHand playerHand;
    private bool hasShuffled;
    private void Start()
    {
        _mouseController = FindObjectOfType<MouseController>();
        
        if (shuffleOnStart)
        {
            ShuffleDeck();
        } 
    }

    public void ShuffleDeck()
    {
        CreatureCards.Shuffle();
        hasShuffled = true;
    }
    
    //pulls the card at index 0 and removes it from this list, adding it to the player's hand 
    public CreatureCard DrawCard()
    {
        //get card at 0
        CreatureCard card = CreatureCards[0];
        //remove it from the deck list
        CreatureCards.RemoveAt(0);

        //shuffle after first draw!
        if (!shuffleOnStart && !hasShuffled)
        {
            ShuffleDeck();
        }
        
        //return it to player
        return card;
    }

    void SpawnCardObject(GameObject handObject)
    {
        //make sure we have cards left
        if (CreatureCards.Count > 0)
        {
            if (playerHand.FindOpenCardSpot() != null)
            {
                //spawn pos at mouse cursor 
                Vector3 spawnPos = _mouseController.threeDCursor.transform.position;

                //generate obj
                GameObject creatureCard = Instantiate(cardPrefab, spawnPos, Quaternion.identity);

                //get card data
                CreatureCard card = DrawCard();

                //get creature behavior class from generated card obj
                CreatureCardItem creatureCardItem= creatureCard.GetComponent<CreatureCardItem>();
        
                //inject creature card data drawn from deck 
                creatureCardItem.InjectCreatureWithData(card, playerHand);

                //add to player hand
                playerHand.AddCardToHand(creatureCardItem, card);

                //start draw timer, subtract from cards to draw. 
                cardDrawTimer = cardDrawTimeTotal;
                playerHand.myPlayer.cardsToDraw--;
                resetting = true;
            }
        }
    }

    //reset timer 
    private bool resetting;
    private float cardDrawTimer = 0f;
    private float cardDrawTimeTotal = 0.5f;
    private void FixedUpdate()
    {
        if (resetting)
        {
            cardDrawTimer -= Time.deltaTime;

            if (cardDrawTimer < 0)
            {
                resetting = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == playerHand.gameObject && !resetting && playerHand.myPlayer.cardsToDraw > 0)
        {
            SpawnCardObject(other.gameObject);
        }
    }

    public void ReturnCard(CreatureCard card)
    {
        CreatureCards.Add(card);
    }
}
