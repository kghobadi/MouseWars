using System;
using System.Collections;
using System.Collections.Generic;
using Cameras;
using UnityEngine;
using UnityEngine.AI;
using Random = System.Random;

/// <summary>
/// Controls the actual creature's behavior on the board. 
/// </summary>
public class CreatureBehavior : AudioHandler
{
    private GameManager gameManager;
    private CameraManager cameraManager;
    private bool init;
    [HideInInspector]
    public CreatureCard myCardData;
    [HideInInspector]
    public PlayerHand teamHand;

    private bool playerIsMovingMe;
    private float moveTimer; //so there aren't overlapping click checks 
    public float mouseMoveWait = 0.25f;
    private bool creatureHasMove;
    private Vector3 nextMoveDestination;
    private MovementFlag movementFlag;

    [SerializeField] private Renderer rend;

    private NavMeshAgent creatureNavMeshAgent;
    private CreatureAnimation _creatureAnimation;
    public GameCamera mouseCamera;
    public Transform cardSpot;
    private CreatureCardItem cardItem;
    public int creatureHP;

    public CreatureStates creatureState;
    public enum CreatureStates
    {
        IDLE, MOVING, ATTACKING,
    }

    public float tussleEnd;
    private GameObject tussleCloudClone;
    private GameObject moveRadiusCyl;
    private int damageToTake;
    private CreatureBehavior myKiller;

    public AudioClip[] tussleSounds;
    
    void Start()
    {
        InitCreature();
    }
    
    public void InjectCreatureData(CreatureCard cardData, CreatureCardItem cItem, PlayerHand handSummoner)
    {
        myCardData = cardData;
        cardItem = cItem;
        teamHand = handSummoner;
        gameObject.name = myCardData.cardName;
        creatureHP = cardData.health;
        moveRadiusCyl = GameObject.Find("MovementRadius");
        
        InitCreature();

        //set move flag to my player layer 
        movementFlag.SetLayers(teamHand.myPlayer.playerLayer);
        //change movement flag sprite 
        movementFlag.spriteFlag.sprite = myCardData.cardSprite;

        //generate model body
        GameObject cBody = Instantiate(myCardData.creatureModelPrefab, transform.position , Quaternion.identity, transform);
        //zero local rot
        cBody.transform.localRotation = Quaternion.Euler(Vector3.zero);
        //get animation component
        _creatureAnimation = cBody.GetComponent<CreatureAnimation>();
        
        //set creature to idle state 
        SetIdle();
        
        //set nav mesh speed
        creatureNavMeshAgent.speed = cardData.moveSpeed;

        //find renderer
        rend = GetComponentInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < rend.materials.Length; i++)
        {
            rend.materials[i].SetColor("_MouseColor", teamHand.teamColor);
            rend.materials[i].SetFloat("_TailLength", myCardData.tailLength);
            rend.materials[i].SetFloat("_RotateAmount", UnityEngine.Random.Range(0f, 10f));
        }
    }
    
    void InitCreature()
    {
        if(init)
            return;

        //get and set refs
        gameManager = GameManager.Instance;
        cameraManager = gameManager.GetCameraManager();
        creatureNavMeshAgent = GetComponent<NavMeshAgent>();
        movementFlag = GetComponentInChildren<MovementFlag>();
        movementFlag.DeactivateFlag();

        //flip the y axis of the mouse camera so it faces correct board direction.
        if (teamHand.zFlipped)
        {
            //mouseCamera.transform.Rotate(0f, 180f, 0f);
        }
        
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
            
            //disable my card item and reset its pos
            cardItem.gameObject.SetActive(false);
            cardItem.transform.localPosition = Vector3.zero;
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
            
            //disable my card item and reset its pos
            cardItem.gameObject.SetActive(false);
            cardItem.transform.localPosition = Vector3.zero;
        }

        //stop moving me 
        if (playerIsMovingMe)
        {
            CancelMove();
        }
    }

    void Update()
    {
        if (playerIsMovingMe)
        {
            //inc timer
            moveTimer += Time.deltaTime;

            //calc distance from hand to mouse being moved 
            float distFromMouse = Vector3.Distance(transform.position, teamHand.transform.position);

            //only move movement flag when player hand is within my move radius 
            if (distFromMouse < myCardData.moveRadius )
            {
                //move line following cursor
                movementFlag.ActivateFlag(teamHand.transform.position + new Vector3(0f, 1f, 0f));
                
                //left click to set move -- only within radius :)
                if (Input.GetMouseButtonDown(0) && moveTimer > mouseMoveWait)
                {
                    SetMoveLocation();
                }
            }
            
            //cancel move w/ right click 
            if (Input.GetMouseButtonDown(1))
            {
                CancelMove();
            }
        }

        //i have a move and it is now the active phase!
        if (creatureHasMove && GameManager.Instance.currentGamePhase == GameManager.Phase.ACTIVE && GameManager.Instance.actionTimer < 8f)
        {
            MoveCreature();
        }
        
        //are we moving?
        if (creatureState == CreatureStates.MOVING)
        {
            float distFromTarget = Vector3.Distance(transform.position, nextMoveDestination);

            if (distFromTarget <= creatureNavMeshAgent.stoppingDistance)
            {
                SetIdle();
            }
        }
        
        //tussling
        if (creatureState == CreatureStates.ATTACKING)
        {
            //play tussle sounds
            if (myAudioSource.isPlaying == false)
            {
                PlayRandomSound(tussleSounds, 1f);
            }
            
            //is it time for the tussle end yet?
            if (Time.time > tussleEnd)
            {
                TakeDamage(damageToTake);
            }
        }
    }

    //called when player first clicks on the creature
    public virtual void BeginMove()
    {
        moveTimer = 0;
        playerIsMovingMe = true;
        teamHand.SetCanHold(false);
        
        //draw move circle. 
        //gameObject.DrawCircle(myCardData.moveRadius, .02f);
        //create cylinder
        moveRadiusCyl.transform.position = transform.position;
        moveRadiusCyl.transform.localScale = new Vector3(myCardData.moveRadius*2f, 1, myCardData.moveRadius*2f);
        //hand is grabbing 
        teamHand.playerHandPose.SetAnimator("directions");

        //set camera
        cameraManager.Set(mouseCamera);
        
        Debug.Log(teamHand.name + " is now moving " + name);
    }

    public void CancelMove()
    {
        movementFlag.DeactivateFlag();
        //gameObject.DeleteCircle();
        moveRadiusCyl.transform.position = new Vector3(1000, 1000, 1000);
        teamHand.SetCanHold(true);
        playerIsMovingMe = false;
        //return to prev camera
        cameraManager.ReturnToPrevCamera();
        //hand is  not grabbing 
        teamHand.playerHandPose.SetAnimator("idle");
    }

    public virtual void SetMoveLocation()
    {
        nextMoveDestination = teamHand.transform.position; 
        movementFlag.ActivateFlag(nextMoveDestination + new Vector3(0f, 1f, 0f));

        playerIsMovingMe = false;
        teamHand.SetCanHold(true);
        //gameObject.DeleteCircle();
        moveRadiusCyl.transform.position = new Vector3(1000, 1000, 1000);

        //return to prev camera
        cameraManager.ReturnToPrevCamera();
        
        //hand is  not grabbing 
        teamHand.playerHandPose.SetAnimator("idle");

        creatureHasMove = true;
    }
    
    //move creature action 
    public virtual void MoveCreature()
    {
        creatureNavMeshAgent.SetDestination(nextMoveDestination);
        creatureNavMeshAgent.isStopped = false;
        movementFlag.DeactivateFlag();
        
        _creatureAnimation.SetAnimator("moving");
        creatureState = CreatureStates.MOVING;
        
        creatureHasMove = false;
    }

    //called when creature safely reaches its move destination
    public void SetIdle()
    {
        creatureNavMeshAgent.isStopped = true;
        _creatureAnimation.SetAnimator("idle");
        creatureState = CreatureStates.IDLE;
    }

    //base for creature attack
    public void Tussle(CreatureBehavior enemyCreature, bool triggeredTussle)
    {
        //did i trigger the tussle?
        if (triggeredTussle)
        {
            //get midpoint between the 2 objects
            Vector3 midpoint = (transform.position + enemyCreature.transform.position) / 2;
            //spawn tussle cloud 
            tussleCloudClone = Instantiate(GameManager.Instance.tussleCloudPrefab, midpoint + new Vector3(0, 0.7f, 0), Quaternion.identity);
            
            //see who has bigger attack value
            if (myCardData.damage > enemyCreature.myCardData.damage)
            {
                //set duration of tussle to my damage val
                tussleEnd = Time.time + myCardData.damage;
            }
            else
            {
                //set duration of tussle to enemy damage val
                tussleEnd = Time.time + enemyCreature.myCardData.damage;
            }
            //set enemy tussle end too!
            enemyCreature.tussleEnd = tussleEnd;
            
            //audience reaction mono
            int randomTussleMono = UnityEngine.Random.Range(0, 2);
            GameManager.Instance.EnableAudienceMonologue(randomTussleMono);
            GameManager.Instance.audienceAnimation.SetAnimator("lean");
            
            //announce tussle
            gameManager.GetAnnouncer().TussleAnnouncement(this, enemyCreature);
            
            Debug.Log(gameObject.name + " started fight with " + enemyCreature.gameObject.name);
        }
        else
        {
            Debug.Log(gameObject.name + " is now tussling with " + enemyCreature.gameObject.name);
        }
        
        //set damage to take
        damageToTake = enemyCreature.myCardData.damage;
        myKiller = enemyCreature;
        
        //stop moving
        creatureNavMeshAgent.isStopped = true;
        creatureNavMeshAgent.velocity = Vector3.zero;
        
        //set attack anim 
        _creatureAnimation.SetAnimator("attacking");
        
        //set my creature state
        creatureState = CreatureStates.ATTACKING;
    }

    //base for taking damage -- ends tussle
    public virtual void TakeDamage(int amount)
    {
        //subtract value from my hp
        creatureHP -= amount;

        //destroy tussle cloud 
        if (tussleCloudClone)
        {
            Destroy(tussleCloudClone);
            tussleCloudClone = null;
        }

        //call death if i am out of hp
        if (creatureHP <= 0)
        {
            Death(false);
        }
        //set idle if i am still alive!
        else
        {
            SetIdle();
        }
    }

    public void Death(bool byMousehole)
    {
        Debug.Log(gameObject.name + " is now declared dead!");

        //death by enemy
        if (!byMousehole)
        {
            gameManager.GetAnnouncer().DeathAnnouncement(myKiller, this);
        }

        //set audience reaction
        int randomDeathMono = UnityEngine.Random.Range(2, 4);
        gameManager.EnableAudienceMonologue(randomDeathMono);
        gameManager.audienceAnimation.SetAnimator("react");
        
        Destroy(gameObject);
    }

    #region Trigger Logic
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Creature"))
        {
            //get other creature's behavior
            CreatureBehavior otherCreature = other.gameObject.GetComponent<CreatureBehavior>();
            //is it on my team?
            if (otherCreature.teamHand != teamHand)
            {
                Debug.Log("My mouse");

                //is it not already fighting? 
                if (otherCreature.creatureState != CreatureStates.ATTACKING)
                {
                    //begin tussle 
                    Tussle(otherCreature, true);
                    otherCreature.Tussle(this, false);
                }
            }

        }
        
        //if my team hand touches me during planning phase
        if (other.gameObject == teamHand.gameObject && GameManager.Instance.currentGamePhase == GameManager.Phase.PLANNING)
        {
            //highlight effect
            for(int i = 0; i < rend.materials.Length; i++)
            {
                rend.materials[i].SetFloat("_FresnelAlpha", 1);
            }
            //rend.material.SetFloat("_FresnelAlpha", 1);
            Debug.Log("Moused");
            
            //enable card item
            cardItem.gameObject.SetActive(true);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        //when in planning phase if the player's hand touches me
        if (other.gameObject == teamHand.gameObject && GameManager.Instance.currentGamePhase == GameManager.Phase.PLANNING)
        {
            //if the player clicks on me and their hand is not carrying a card. 
            if (Input.GetMouseButtonDown(0) && teamHand.activeCard == null && teamHand.canHoldCard)
            {
                BeginMove();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Creature"))
        {
            //get other creature's behavior
            CreatureBehavior otherCreature = other.gameObject.GetComponent<CreatureBehavior>();
            //is it on my team?
            if (otherCreature.teamHand != teamHand)
            {
                Debug.Log("mouse off");
                //add highlight to fresnel shader
            }
        }
        
        //if my team hand leaves my trigger during planning phase
        if (other.gameObject == teamHand.gameObject && GameManager.Instance.currentGamePhase == GameManager.Phase.PLANNING)
        {
            Debug.Log("Mouse off");
            //rend.material.SetFloat("_FresnelAlpha", 0);
            
            //highlight effect
            for (int i = 0; i < rend.materials.Length; i++)
            {
                rend.materials[i].SetFloat("_FresnelAlpha", 0);
            }
            
            //disable card item
            cardItem.gameObject.SetActive(false);
        }
    }
    #endregion
}
