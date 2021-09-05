using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiplier : MonoBehaviour
{
    public float timer, multiplyTime = 5f;
    public List<GameObject> clones = new List<GameObject>();
    
    void Update()
    {
        timer += Time.deltaTime;

        if(timer > multiplyTime)
        {
            GenerateClone();
            timer = 0;
        }
    }

    public void GenerateClone()
    {
        GameObject clone = Instantiate(gameObject, transform.position, transform.rotation);
        clone.GetComponent<Multiplier>().enabled = false;
        clones.Add(clone);
    }

    public void DeleteAllClones()
    {
        for(int i = 0; i < clones.Count; i++)
        {
            Destroy(clones[i]);
        }

        clones.Clear();
    }

}
