using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Controls the actual creature's behavior on the board. 
/// </summary>
public class CreatureBehavior : AudioHandler
{
    private bool init;
    private CreatureCard myCardData;
    private PlayerHand teamHand;

    private bool playerIsMovingMe;
    private bool creatureHasMove;
    private Vector3 nextMoveDestination;
    private MovementFlag movementFlag;

    private NavMeshAgent creatureNavMeshAgent;
    
    void Start()
    {
        InitCreature();
    }   

    void InitCreature()
    {
        if(init)
            return;

        creatureNavMeshAgent = GetComponent<NavMeshAgent>();

        init = true;
    }

    void Update()
    {
        if (playerIsMovingMe)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetMoveLocation();
            }
        }

        if (creatureHasMove && GameManager.Instance.currentGamePhase == GameManager.Phase.ACTIVE)
        {
            MoveCreature();
        }
    }

    public void InjectCreatureData(CreatureCard cardData, PlayerHand handSummoner)
    {
        myCardData = cardData;
        teamHand = handSummoner;
        
        InitCreature();

        creatureNavMeshAgent.speed = cardData.moveSpeed;
    }

    //called when player first clicks on the creature
    public virtual void BeginMove()
    {
        playerIsMovingMe = true;
    }

    public virtual void SetMoveLocation()
    {
        nextMoveDestination = teamHand.transform.position;
        movementFlag.ActivateFlag(nextMoveDestination);

        playerIsMovingMe = false;
    }
    
    //move creature action 
    public virtual void MoveCreature()
    {
        creatureNavMeshAgent.SetDestination(nextMoveDestination);

        creatureHasMove = false;
    }

    //base for creature attack
    public virtual void Attack()
    {
        
    }

    //base for taking damage 
    public virtual void TakeDamage()
    {
        
    }

    private void OnTriggerStay(Collider other)
    {
        //when in planning phase if the player's hand touches me
        if (other.gameObject == teamHand.gameObject && GameManager.Instance.currentGamePhase == GameManager.Phase.PLANNING)
        {
            //if the player clicks on me and their hand is not carrying a card. 
            if (Input.GetMouseButtonDown(0) && teamHand.activeCard == null)
            {
                BeginMove();
            }
        }
    }
}
