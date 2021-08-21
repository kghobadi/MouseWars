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
    private float moveTimer; //so there aren't overlapping click checks 
    private bool creatureHasMove;
    private Vector3 nextMoveDestination;
    private MovementFlag movementFlag;

    private NavMeshAgent creatureNavMeshAgent;
    private CreatureAnimation _creatureAnimation;
    void Start()
    {
        InitCreature();
    }
    
    public void InjectCreatureData(CreatureCard cardData, PlayerHand handSummoner)
    {
        myCardData = cardData;
        teamHand = handSummoner;
        gameObject.name = myCardData.cardName;
        
        InitCreature();

        //change movement flag sprite 
        movementFlag.spriteFlag.sprite = myCardData.cardSprite;

        //generate model body
        GameObject cBody = Instantiate(myCardData.creatureModelPrefab, transform.position , Quaternion.identity, transform);
        //get animation component
        _creatureAnimation = cBody.GetComponent<CreatureAnimation>();
        
        //set nav mesh speed
        creatureNavMeshAgent.speed = cardData.moveSpeed;
    }
    
    void InitCreature()
    {
        if(init)
            return;

        //get and set refs
        creatureNavMeshAgent = GetComponent<NavMeshAgent>();
        movementFlag = GetComponentInChildren<MovementFlag>();
        movementFlag.DeactivateFlag();
        
        //add event listeners
        GameManager.Instance.changedPhases.AddListener(OnChangedPhases);
        GameManager.Instance.changedPlayers.AddListener(OnChangedPlayer);

        init = true;
    }
    
    void OnChangedPhases()
    {
        if (GameManager.Instance.currentGamePhase == GameManager.Phase.ACTIVE)
        {
            gameObject.layer = 0; //set to default layer 
        }
        else if (GameManager.Instance.currentGamePhase == GameManager.Phase.PLANNING)
        {
            gameObject.layer = 8;  //set playable layer 
        }
    }

    void OnChangedPlayer()
    {
        if (GameManager.Instance.currentPlayer.playerHand == teamHand)
        {
            gameObject.layer = 8;  //set playable layer 
        }
        else
        {
            gameObject.layer = 0; //set to default layer 
        }
    }

    void Update()
    {
        if (playerIsMovingMe)
        {
            moveTimer += Time.deltaTime;
            if (Input.GetMouseButtonDown(0) && moveTimer > 0.1f)
            {
                //move line following cursor
                movementFlag.ActivateFlag(teamHand.transform.position + new Vector3(0f, 1f, 0f));
                SetMoveLocation();
            }

            //cancel move
            if (Input.GetMouseButtonDown(1))
            {
                movementFlag.DeactivateFlag();
                playerIsMovingMe = false;
            }
        }

        if (creatureHasMove && GameManager.Instance.currentGamePhase == GameManager.Phase.ACTIVE)
        {
            MoveCreature();
        }
    }

    //called when player first clicks on the creature
    public virtual void BeginMove()
    {
        moveTimer = 0;
        playerIsMovingMe = true;
        teamHand.SetCanHold(false);
        
        Debug.Log(teamHand.name + " is now moving " + name);
    }

    public virtual void SetMoveLocation()
    {
        nextMoveDestination = teamHand.transform.position; 
        movementFlag.ActivateFlag(nextMoveDestination + new Vector3(0f, 1f, 0f));

        playerIsMovingMe = false;
        teamHand.SetCanHold(true);

        creatureHasMove = true;
    }
    
    //move creature action 
    public virtual void MoveCreature()
    {
        creatureNavMeshAgent.SetDestination(nextMoveDestination);
        creatureNavMeshAgent.isStopped = false;
        movementFlag.DeactivateFlag();
        
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
