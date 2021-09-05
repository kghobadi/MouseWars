using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdvanceScene : MonoBehaviour {

    public float timeToRestart = 5f;
    public float restartTimer;

    public FadeUI fader;
    public float fadeWait = 1f;
    public bool debugInputs;
    
	void Update ()
    {
        if (debugInputs)
        {
            //next scene 
            if (Input.GetKeyDown(KeyCode.Return) )
            {
                LoadNextScene();
            }

            //previous scene 
            if (Input.GetKeyDown(KeyCode.CapsLock) )
            {
                LoadPreviousScene();
            }

            //restart game
            if (Input.GetKey(KeyCode.Delete) )
            {
                restartTimer += Time.deltaTime;

                if(restartTimer > timeToRestart)
                {
                    Restart();
                }
            }
            else
            {
                restartTimer = 0;
            }
        }
	}

    public void LoadPreviousScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    public void LoadNextScene()
    {
        if (fader)
        {
            fader.FadeIn();

            StartCoroutine(WaitToLoad(fadeWait, SceneManager.GetActiveScene().buildIndex + 1));
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

    public void Restart()
    {
        if (fader)
        {
            fader.FadeIn();
            
            StartCoroutine(WaitToLoad(fadeWait, 0));
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
    
    
    IEnumerator WaitToLoad(float time, int scene)
    {
        yield return new WaitForSeconds(time);

        SceneManager.LoadScene(scene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
