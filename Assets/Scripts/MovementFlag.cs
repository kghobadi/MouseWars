
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Shows the upcoming move for a mouse 
/// </summary>
public class MovementFlag : MonoBehaviour
{
    public LineRenderer lineToFlag;
    public SpriteRenderer spriteFlag;
    public Transform lineStartPoint;

    public void ActivateFlag(Vector3 endpoint)
    {
        //set flag position
        transform.position = endpoint;
       
        //set line positions
        lineToFlag.SetPosition(0, lineStartPoint.position);
        lineToFlag.SetPosition(1, endpoint);
        
        //enable line & sprite
        lineToFlag.enabled = true;
        spriteFlag.enabled = true;
    }

    public void DeactivateFlag()
    {
        lineToFlag.enabled = false;
        spriteFlag.enabled = false;
    }
}
