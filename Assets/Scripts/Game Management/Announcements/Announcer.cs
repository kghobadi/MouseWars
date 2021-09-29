using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Announces tussles between mice, deaths, and damage dealt to mouse holes.
/// Should take card items and somehow translate them to be in front of the main camera or instead Generate UI versions on the main canvas. 
/// </summary>
public class Announcer : AudioHandler
{
    private GameManager gameManager;
    
    [Header("Announcement Settings")]
    public GameObject cardUIprefab;
    public GameObject announcementsScrollView;
    public GameObject activationButton;
    public Transform scrollContent;

    public GameObject tussleUiPrefab;
    public GameObject deathUiPrefab;
    public GameObject mouseHoleDamageUiPrefab;

    public bool active;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        
        Deactivate();
        activationButton.SetActive(false);
        
        gameManager.actionPhaseBegin.AddListener(OnActionPhaseBegin);
        gameManager.actionPhaseEnd.AddListener(OnActionPhaseEnd);
    }

    public void ToggleActive()
    {
        if (active)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }
    
    public void Activate()
    {
        announcementsScrollView.SetActive(true);
        active = true;
    }

    public void Deactivate()
    {
        announcementsScrollView.SetActive(false);
        active = false;
    }
    
    void OnActionPhaseBegin()
    {
        Activate();
    }

    void OnActionPhaseEnd()
    {
        Deactivate();
    }

    public void GenerateCreatureCardUIObject(Transform UIparent, CreatureBehavior creatureBehavior)
    {
        GameObject creatureCardUIobj = Instantiate(cardUIprefab, UIparent);

        CreatureCardUIItem creatureCardUI = creatureCardUIobj.GetComponent<CreatureCardUIItem>();
        
        creatureCardUI.InjectCreatureWithData(creatureBehavior.myCardData , creatureBehavior.teamHand.myPlayer);
    }

    //called by the initiator of the tussle 
    public void TussleAnnouncement(CreatureBehavior tussler, CreatureBehavior victim)
    {
        GameObject tussleAnnouncement = Instantiate(tussleUiPrefab, scrollContent);

        TussleAnnouncement tussleAnnounce = tussleAnnouncement.GetComponent<TussleAnnouncement>();
        
        tussleAnnounce.SetAnnouncer(this);
        
        GenerateCreatureCardUIObject(tussleAnnounce.tusslerParent, tussler);
        
        GenerateCreatureCardUIObject(tussleAnnounce.victimParent, victim);
        
        //play tussle sound?
    }

    //called when any mouse is killed in a tussle
    public void DeathAnnouncement(CreatureBehavior killer, CreatureBehavior martyr)
    {
        GameObject deathAnnouncement = Instantiate(deathUiPrefab, scrollContent);

        DeathAnnouncement deathAnnounce = deathAnnouncement.GetComponent<DeathAnnouncement>();
        
        deathAnnounce.SetAnnouncer(this);
        
        GenerateCreatureCardUIObject(deathAnnounce.killerParent, killer);
        
        GenerateCreatureCardUIObject(deathAnnounce.martyrParent, martyr);
        
        //play tussle sound?
    }

    //called when any mouse damages a mouse hole 
    public void MouseHoleDamagedAnnouncement(CreatureBehavior dealer, MouseHole mouseHole)
    {
        GameObject mouseHoleDamagedAnnouncement = Instantiate(mouseHoleDamageUiPrefab, scrollContent);

        MouseHoleDamageAnnouncement mouseHoleDamagedAnnounce = mouseHoleDamagedAnnouncement.GetComponent<MouseHoleDamageAnnouncement>();
        
        mouseHoleDamagedAnnounce.SetAnnouncer(this);
        
        GenerateCreatureCardUIObject(mouseHoleDamagedAnnounce.dealerParent, dealer);
        
        mouseHoleDamagedAnnounce.SetDamageAmount(dealer.myCardData.damage);
        mouseHoleDamagedAnnounce.SetPlayerImage(mouseHole.GetPlayer().playerSprite);
        
        //play sound?
    }
}
