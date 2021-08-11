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
    }
    
    //pulls the card at index 0 and removes it from this list, adding it to the player's hand 
    public CreatureCard DrawCard()
    {
        //get card at 0
        CreatureCard card = CreatureCards[0];
        //remove it from the deck list
        CreatureCards.RemoveAt(0);
        
        //return it to player
        return card;
    }

    void SpawnCardObject(GameObject handObject)
    {
        //make sure we have cards left
        if (CreatureCards.Count > 0)
        {
            Vector3 spawnPos = _mouseController.threeDCursor.transform.position;

            GameObject creatureCard = Instantiate(cardPrefab, spawnPos, Quaternion.identity, handObject.transform);

            CreatureCard card = DrawCard();

            CreatureBehavior creatureBehavior = creatureCard.GetComponent<CreatureBehavior>();
        
            creatureBehavior.InjectCreatureWithData(card);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Hand")
        {
            SpawnCardObject(other.gameObject);
        }
    }
}
