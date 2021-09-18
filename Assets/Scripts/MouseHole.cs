using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Controls mouse hole placement, behavior, and hitpoints.
/// </summary>
public class MouseHole : AudioHandler
{
    //dependencies
    private GamePlayer myPlayer;
    private GameManager gameManager;
    private PlayerHand _playerHand;
    
    [Header("Mousehole Data")]
    public int hitpoints;
    public int totalHealth = 20;
    public bool placing;
    public float xMin = -4f, xMax = 4f;
    public TMP_Text holeHPtext;
    
    
    private void Start()
    {
        gameManager = GameManager.Instance;
        holeHPtext = GetComponentInChildren<TMP_Text>();
    }

    public GamePlayer GetPlayer()
    {
        return myPlayer;
    }

    public void BeginPlacement(GamePlayer player)
    {
        myPlayer = player;
        _playerHand = player.playerHand;
        placing = true;
    }
    
    void Update()
    {
        if (placing)
        {
            transform.position = new Vector3(Mathf.Clamp(_playerHand.transform.position.x, xMin, xMax), transform.position.y, transform.position.z);

            if (Input.GetMouseButtonDown(0))
            {
                SetPlacement();
            }
        }
    }

    public void SetPlacement()
    {
        placing = false;
        hitpoints = totalHealth;
        
        //set hp on hole
        holeHPtext.SetText(hitpoints.ToString() + "HP");
        //play sound?
    }

    private void OnTriggerEnter(Collider other)
    {
        //when a creature touches me
        if (other.CompareTag("Creature"))
        {
            //get other creature's behavior
            CreatureBehavior otherCreature = other.gameObject.GetComponent<CreatureBehavior>();
            //is it on my team?
            if (otherCreature.teamHand != _playerHand)
            {
                //take damage from it
                TakeDamage(otherCreature);
            }
        }
    }
    
    public virtual void TakeDamage(CreatureBehavior enemyCreature)
    {
        //subtract value from my hp
        hitpoints -= enemyCreature.myCardData.damage;
        //set hp on hole
        holeHPtext.SetText(hitpoints.ToString() + "HP");
        
        //announce damage to mousehole
        gameManager.GetAnnouncer().MouseHoleDamagedAnnouncement(enemyCreature, this);
        
        //enemy death
        enemyCreature.Death(true);
        
        //say took damage
        myPlayer.playerMonologueManager.SetMonologueSystem(2);
        myPlayer.playerMonologueManager.EnableMonologue();

        //other player taunts you with Dealt Damage line
        if (myPlayer.isFirstPlayer)
        {
            gameManager.playerTwo.playerMonologueManager.SetMonologueSystem(3);
            gameManager.playerTwo.playerMonologueManager.EnableMonologue();
        }
        else
        {
            gameManager.playerOne.playerMonologueManager.SetMonologueSystem(3);
            gameManager.playerOne.playerMonologueManager.EnableMonologue();
        }

        //game over
        if (hitpoints <= 0)
        {
            //my player is the loser :-(
            gameManager.GameOver(myPlayer);
        }
    }
    
    

}
