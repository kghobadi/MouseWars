using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickToStartMono : MonoBehaviour
{
    public MonologueManager monoMan;
    public int monoIndex;
    public bool hasActivated;

    private void Awake()
    {
        if (monoMan == null)
            monoMan = GetComponent<MonologueManager>();
    }

    private void OnMouseDown()
    {
        if (!hasActivated)
        {
            ActivateMono();
        }
    }

    void ActivateMono()
    {
        monoMan.SetMonologueSystem(monoIndex);

        monoMan.EnableMonologue();

        hasActivated = true;

        Debug.Log("activated mono");
    }
}
