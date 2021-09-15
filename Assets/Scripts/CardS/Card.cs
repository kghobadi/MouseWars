using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : AudioHandler
{
    //called to draw this card from the Deck
    public virtual void DrawCard()
    {
        
    }

    //pick card from player Active Hand
    public virtual void PickCard()
    {
        
    }

    //activates card on the board
    public virtual void ActivateCard(Vector3 worldPos, bool zFlipped)
    {
        
    }

   
}
