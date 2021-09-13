using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using Cameras;
using Cinemachine;
using UnityEngine.Events;

public class MonologueManager : MonoBehaviour
{
    //player refs
    AdvanceScene scener;
    GameObject currentPlayer;
    CameraManager camManager;
    CinematicsManager cineManager;

    //npc management refs 
    [HideInInspector]
    public WorldMonologueManager wmManager;
    [Tooltip("Only set this if your canvas is Screen space and not world space (i.e., if canvas is not a child of this obj")]
    public MonologueReader monoReader;
    
    [Tooltip("if there is a background for speaking text")]
    public FadeUI textBack;
    //text component and string array of its lines
    [Tooltip("if there is an icon for speaking text")]
    public FadeUI characterIcon;
    public int currentMonologue;
    [Tooltip("Fill this with all the individual monologues the character will give")]
    public List<Monologue> allMyMonologues = new List<Monologue>();
    
    public bool inMonologue;
    [HideInInspector]
    public MonologueTrigger mTrigger;
    public GameCamera monoCam;

    [Tooltip("Check to Enable monologue at index 0 at start")]
    public bool enableOnStart;
    
    [Header("Events")]
    public UnityEvent startMono;
    public UnityEvent endMono;

    void Awake()
    {
        scener = FindObjectOfType<AdvanceScene>();
        cineManager = FindObjectOfType<CinematicsManager>();

        wmManager = FindObjectOfType<WorldMonologueManager>();
        camManager = FindObjectOfType<CameraManager>();

        if(monoReader == null)
            monoReader = GetComponentInChildren<MonologueReader>();
        if (monoReader.hostObj == null)
            monoReader.hostObj = gameObject;
        monoReader.monoManager = this;
    }

    void Start()
    {
        //set text to first string in my list of monologues 
        if(allMyMonologues.Count > 0)
            SetMonologueSystem(currentMonologue);

        //play mono 0 
        if (enableOnStart)
        {
            EnableMonologue();
        }
    }

    //sets monologue system to values contained in Monologue[index]
    public void SetMonologueSystem(int index)
    {
        //set current monologue
        currentMonologue = index;

        //set mono reader text lines 
        monoReader.textLines = (allMyMonologues[currentMonologue].monologue.text.Split('\n'));

        //set current to 0 and end to length 
        monoReader.currentLine = 0;
        monoReader.endAtLine = monoReader.textLines.Length;

        //set mono reader text speeds 
        monoReader.timeBetweenLetters = allMyMonologues[currentMonologue].timeBetweenLetters;
        monoReader.timeBetweenLines = allMyMonologues[currentMonologue].timeBetweenLines;
        monoReader.conversational = allMyMonologues[currentMonologue].conversational;
        monoReader.waitTimes = allMyMonologues[currentMonologue].waitTimes;

        //found a single audio clip 
        if (allMyMonologues[currentMonologue].singleAudioClip != null)
        {
            //set mono reader
            monoReader.hasSingleAudioClip = true;
            monoReader.singleClip = allMyMonologues[currentMonologue].singleAudioClip;
        }
        //no single audio clip -- disable it in mono reader
        else
        {
            //set mono reader
            monoReader.hasSingleAudioClip = false;
            monoReader.singleClip = null;
        }
    }

    //has a wait for built in
    public void EnableMonologue()
    {
        //disable until its time to start 
        if (allMyMonologues[currentMonologue].waitToStart)
        {
            StartCoroutine(WaitToStart());
        }
        //starts now
        else
        {
            StartMonologue();
        }
    }

    IEnumerator WaitToStart()
    {
        yield return new WaitForSeconds(allMyMonologues[currentMonologue].timeUntilStart);

        StartMonologue();
    }

    //actually starts
    void StartMonologue()
    {
        //enable text comps 
        if (monoReader.usesTMP)
            monoReader.the_Text.enabled = true;
        else
            monoReader.theText.enabled = true;

        //textback
        if (textBack)
        {
            textBack.FadeIn();
        }
        //icon
        if (characterIcon)
        {
            characterIcon.FadeIn();
        }

        //player ref 
        GameCamera cam = camManager.currentCamera;
        //currentPlayer = cam.transform.parent.gameObject;

        //transition to mono cam
        if(monoCam != null)
        {
            camManager.Set(monoCam);
        }

        //lock player movement
        if (allMyMonologues[currentMonologue].lockPlayer)
        {
            
        }

        //event
        startMono.Invoke();
        
        //begin mono 
        inMonologue = true;

        //start the typing!
        monoReader.SetTypingLine();
    }
    
    public void DisableMonologue()
    {
        StopAllCoroutines();
        monoReader.ClearMonoReader();
        //disable text components 
        if (monoReader.usesTMP)
            monoReader.the_Text.enabled = false;
        else
            monoReader.theText.enabled = false;

        //textback
        if (textBack)
        {
            textBack.FadeOut();
        }
        //icon
        if (characterIcon)
        {
            characterIcon.FadeOut();
        }
        
        StartCoroutine(WaitForCameraTransition());
    }

    IEnumerator WaitForCameraTransition()
    {
        yield return new WaitForSeconds(1f);

        Monologue mono = allMyMonologues[currentMonologue];

        //player ref 
        GameCamera cam = camManager.currentCamera;
        //currentPlayer = cam.transform.parent.gameObject;

        //unlock player
        if (mono.lockPlayer)
        {
            
        }

        //check for cinematic to enable 
        if (mono.playsCinematic)
        {
            cineManager.allCinematics[mono.cinematic.cIndex].cPlaybackManager.StartTimeline();
        }
        //cinematic triggers to enable
        if (mono.enablesCinematicTriggers)
        {
            for (int i = 0; i < mono.cTriggers.Length; i++)
            {
                cineManager.allCinematics[mono.cTriggers[i].cIndex].cTrigger.gameObject.SetActive(true);
            }
        }

      
        //if this monologue repeats at finish
        if (mono.repeatsAtFinish)
        {
            //reset the monologue trigger after 3 sec 
            if(mTrigger)
                mTrigger.WaitToReset(5f);
        }
        //disable the monologue trigger, it's done 
        else
        {
            if(mTrigger)
                mTrigger.gameObject.SetActive(false);
        }

        //if this monologue has a new monologue to activate
        if (mono.triggersMonologues)
        {
            //enable the monologues but wait to make them usable to player 
            for(int i = 0; i< mono.monologueTriggerIndeces.Length; i++)
            {
                MonologueTrigger mTrigger = wmManager.allMonologues[mono.monologueTriggerIndeces[i]].mTrigger;
                mTrigger.gameObject.SetActive(true);
                mTrigger.hasActivated = true;
                mTrigger.WaitToReset(mono.monologueWaits[i]);
            }

            //loop thru other managers to activate
            for (int i = 0; i < mono.monologueManagerIndeces.Length; i++)
            {
                //get manager
                MonologueManager otherMonoManager = wmManager.allMonoManagers[mono.monologueManagerIndeces[i]];
                //set manager to new monologue from within its list
                otherMonoManager.SetMonologueSystem(mono.monologueIndecesWithinManager[i]);
                //enable it?
                otherMonoManager.EnableMonologue();
            }
        }

        //advance scene 
        if (mono.loadsScene)
            scener.LoadNextScene();

        //event
        endMono.Invoke();
        
        inMonologue = false;
    }
}

