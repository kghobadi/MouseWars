using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cameras;

public class MonologueTrigger : MonoBehaviour
{
    //player refs
    GameObject currentPlayer;
    CameraManager camManager;

    //general
    [Tooltip("Only need this if the Trigger first becomes active when an NPC moves into it")]
    public GameObject speakerHost;
    [Tooltip("Defaults to true, uncheck if player can only activate once an NPC enters it")]
    public bool canActivate = true;
    [Tooltip("Check to auto activate when player enters trigger")]
    public bool autoActivate;
    [Tooltip("True when monologue has been activated")]
    public bool hasActivated;
    [Tooltip("True when player is within trigger")]
    public bool playerInZone;
    [Tooltip("Check to display talking head UI")]
    public bool displayUI;
    public GameObject interactUI;
    [Tooltip("Will attach to NPC upon activation")]
    public bool parentToNPC;
    int activationCount = 0;

    public string playerTag = "Player";
    
    //monologues
    [Tooltip("Monologue Managers of the NPCs whose monologues should be activated")]
    public MonologueManager[] myMonologues;
    [Tooltip("Indeces of above Mono Managers to set")]
    public int[] monoNumbers;

    private void Awake()
    {
        camManager = FindObjectOfType<CameraManager>();
        //disable ui 
        if (interactUI)
            interactUI.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        //player ref 
        GameCamera cam = camManager.currentCamera;
        currentPlayer = cam.transform.parent.gameObject;

        //player entered 
        if (other.gameObject == currentPlayer)
        {
            if (!playerInZone && canActivate)
                PlayerEnteredZone();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == playerTag)
        {
            if (!playerInZone && canActivate)
                PlayerEnteredZone();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == playerTag)
        {
            PlayerExitedZone();
        }
    }

    void Update()
    {
        //get input device 
        //var inputDevice = InputManager.ActiveDevice;

        if (playerInZone)
        {
            if ((Input.GetKeyUp(KeyCode.Space) || autoActivate) && !hasActivated)
            {
                ActivateMonologue();
            }
        }
    }

    //called in OnTriggerEnter()
    public void PlayerEnteredZone()
    {
        if (!hasActivated)
        {
            playerInZone = true;

            if (displayUI)
                interactUI.SetActive(true);
        }
    }

    //activates monologues
    void ActivateMonologue()
    {
        if (!hasActivated)
        {
            //sets monologues 
            for (int i = 0; i < myMonologues.Length; i++)
            {
                myMonologues[i].mTrigger = this;
                myMonologues[i].SetMonologueSystem(monoNumbers[i]);
                myMonologues[i].EnableMonologue();
            }
            
            hasActivated = true;
            activationCount++;
            autoActivate = false;

            if (displayUI)
                interactUI.SetActive(false);

            //follow NPC 
            if (parentToNPC)
                transform.SetParent(myMonologues[0].transform);
        }
    }
    
    //called in OnTriggerExit()
    public void PlayerExitedZone()
    {
        playerInZone = false;

        if (displayUI)
            interactUI.SetActive(false);
    }

    //called when monologue text script is reset
    public void WaitToReset(float time)
    {
        StartCoroutine(WaitToReactivate(time));
    }

    IEnumerator WaitToReactivate(float timer)
    {
        yield return new WaitForSeconds(timer);

        hasActivated = false;
    }
}
