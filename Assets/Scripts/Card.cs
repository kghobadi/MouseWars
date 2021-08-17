using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Card : MonoBehaviour
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
    public virtual void ActivateCard(Vector3 worldPos)
    {
        
    }

    //move creature action 
    public virtual void MoveCreature()
    {
        
    }

    //base for creature attack
    public virtual void Attack()
    {
        
    }

    //base for taking damage 
    public virtual void TakeDamage()
    {
        
    }
}
