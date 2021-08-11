using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// Controls the mouse movement (cursor and clicking on things :-)
/// </summary>
public class MouseController : MonoBehaviour
{
    public GameObject threeDCursor;
    public float rayDistance = 15f;
    public LayerMask mouseInteractive;
    private Camera mainCam;

    public List<CreatureCard> myHand = new List<CreatureCard>();
    
    void Start()
    {
        mainCam = Camera.main;

        //enable cursor and confine to window 
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }

    void Update()
    {
        if (threeDCursor)
        {
            PositionCursorInWorld();
        }
    }

    //Moves 3d cursor in world space 
    void PositionCursorInWorld()
    {
        // Convert to world space
        Ray ray = HandleUtility.GUIPointToWorldRay(Input.mousePosition);
        RaycastHit hit;
        bool result;

        result = Physics.Raycast(ray, out hit, mainCam.farClipPlane, mouseInteractive);

        // Find our ray's intersection through the selected layer
        if (result)
        {
            threeDCursor.transform.position = hit.point ;
        }
    }

    public void DrawCard()
    {
        
    }
}
