using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//this script is responsible for the active reading out of a monologue 
public class MonologueReader : MonoBehaviour {
    public GameObject hostObj;  // parent NPC obj set by MonologueManager
    [HideInInspector]
    public MonologueManager monoManager; //my mono manager

    SpeakerSound speakerAudio;
    [HideInInspector] public Text theText;
    [HideInInspector] public TMP_Text the_Text;
    [HideInInspector] public bool usesTMP;
    [Tooltip("No need to fill this in, that will happen automatically")]
    public string[] textLines;
    //current and last lines
    public int currentLine;
    public int endAtLine;
    public bool canSkip = true;
    public bool hasFinished;
    //typing vars
    private bool isTyping = false;
    IEnumerator currentTypingLine;
    IEnumerator waitForNextLine;

    [Header("Text Timing")]
    public float timeBetweenLetters;
    //wait between lines
    public float timeBetweenLines;
    [Tooltip("Check this and fill in array below so that each line of text can be assigned a different wait")]
    public bool conversational;
    public float[] waitTimes;
    bool waiting;

    [Header("Single Audio Clip")]
    public bool hasSingleAudioClip;
    [Tooltip("Single audio clip can be passed by individual Monologue objects to play a single clip rather than read audio sounds")]
    public AudioClip singleClip;

    public bool animatedSprite = false;
    public Animator speakerSprite;

    void Awake()
    {
        theText = GetComponent<Text>();

        if (theText == null)
        {
            usesTMP = true;
            the_Text = GetComponent<TMP_Text>();
        }



        if (usesTMP)
            the_Text.enabled = false;
        else
            theText.enabled = false;
    }

    void Start()
    {
        
        speakerAudio = hostObj.GetComponent<SpeakerSound>();
    }

    void Update ()
    {
        LineSkipping();
    }

    void LineSkipping()
    {
        //get input device 
        //var inputDevice = InputManager.ActiveDevice;

        //speaker is typing out message
        if (isTyping)
        {
            //player skips to the end of the line
            if (Input.GetMouseButtonDown(0) && canSkip)
            {
                if (currentTypingLine != null)
                {
                    StopCoroutine(currentTypingLine);
                }

                //set to full line
                if (isTyping)
                    CompleteTextLine(textLines[currentLine]);

                SetWaitForNextLine();
            }
        }

        //player is waiting for next message
        if (waiting)
        {
            //player skips to next line
            if (Input.GetMouseButtonDown(0) && canSkip)
            {
                if (waitForNextLine != null)
                {
                    StopCoroutine(waitForNextLine);
                }

                ProgressLine();
            }
        }
    }

    void ProgressLine()
    {
        currentLine += 1;
        waiting = false;

        //reached the  end, reset
        if (currentLine >= endAtLine)
        {
            hasFinished = true;
            monoManager.DisableMonologue();
        }
        //set next typing line 
        else
        {
            SetTypingLine();
        }
    }

    //calls text scroll coroutine 
    public void SetTypingLine()
    {
        if (currentTypingLine != null)
        {
            StopCoroutine(currentTypingLine);
        }
        currentTypingLine = TextScroll(textLines[currentLine]);

        StartCoroutine(currentTypingLine);
    }

    //Coroutine that types out each letter individually
    private IEnumerator TextScroll(string lineOfText)
    {
        // set first letter
        int letter = 0;
        if (usesTMP)
            the_Text.text = "";
        else
            theText.text = "";

        //play single audio clip
        if (hasSingleAudioClip)
        {
            if (speakerAudio != null )
            {
                speakerAudio.PlaySound(singleClip, 1f);
                if(animatedSprite)
                    speakerSprite.SetTrigger("Spoke");
            }
        }
        
        isTyping = true;

        while (isTyping && (letter < lineOfText.Length - 1))
        {
            //add this letter to our text
            if (usesTMP)
                the_Text.text += lineOfText[letter];
            else
                theText.text += lineOfText[letter];

            //check what audio to play 
            if (speakerAudio != null && !hasSingleAudioClip)
            {
                speakerAudio.AudioCheck(lineOfText, letter);
                if(animatedSprite)
                    speakerSprite.SetTrigger("Spoke");
            }
                
            //next letter
            letter += 1;
            yield return new WaitForSeconds(timeBetweenLetters);
        }

        //player waited to read full line
        if (isTyping)
            CompleteTextLine(lineOfText);

        SetWaitForNextLine();
    }

    //completes current line of text
    void CompleteTextLine(string lineOfText)
    {
        if (usesTMP)
            the_Text.text = lineOfText;
        else
            theText.text = lineOfText;
        isTyping = false;
    }

    //calls wait for next line coroutine 
    void SetWaitForNextLine()
    {
        //start waiting coroutine 
        if (waitForNextLine != null)
        {
            StopCoroutine(waitForNextLine);
        }

        //check what the wait time for this line should be 
        if (conversational)
        {
            waitForNextLine = WaitToProgressLine(waitTimes[currentLine]);
        }
        else
        {
            waitForNextLine = WaitToProgressLine(timeBetweenLines);
        }

        StartCoroutine(waitForNextLine);
    }

    //start wait for next line after spacebar skip
    IEnumerator WaitToProgressLine(float time)
    {
        yield return new WaitForEndOfFrame();

        waiting = true;

        yield return new WaitForSeconds(time);

        ProgressLine();
    }

    public void ClearMonoReader()
    {
        StopAllCoroutines();
        isTyping = false;
        waiting = false;
    }
}
