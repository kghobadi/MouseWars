using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Dialogue : MonoBehaviour
{
    public bool done;

    public TextMeshProUGUI textDisplay;
    public string[] sentences;
    private int index;
    public float typingSpeed;
    public Sprite mickey;
    public Sprite pika;
    //public Sprite jerry;

    public GameObject portrait; 
    private AudioSource source;


    public int introDelay;
    public int betweenDelay;
    bool firstSentence;

    // public GameObject background; 

    void Start()
    {
        source = GetComponent<AudioSource>();
        firstSentence = true;
        StartCoroutine(Type());
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Return) || (Input.GetMouseButtonDown(0)))
        {
            if (textDisplay.text == sentences[index])
            {
                NextSentence();
            }
            else
            {
                //do nothing 
            }

        }
    }

    IEnumerator Type()
    {
        //delay before the first dialogue
        if (firstSentence)
        {
            yield return new WaitForSeconds(introDelay);
        }
        else if (!firstSentence)
        {
            yield return new WaitForSeconds(betweenDelay);
        }
        foreach (char letter in sentences[index].ToCharArray())
        {
            textDisplay.text += letter;

            //play dialogue sound at a random pitch 
            source.pitch = Random.value;
            source.Play();

            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void NextSentence()
    {
        firstSentence = false;

        if (index < sentences.Length - 1)
        {
            index++;
            textDisplay.text = "";
            StartCoroutine(Type());
        }
        else
        {
            textDisplay.text = "";
            done = true;
            Debug.Log("end of dialogue");
        }
    }
}
