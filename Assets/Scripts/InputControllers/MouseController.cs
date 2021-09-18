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
    public LayerMask mouseInteractive;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        //enable cursor and confine to window 
        Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Confined;
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
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool result;

        result = Physics.Raycast(ray, out hit, mainCam.farClipPlane, mouseInteractive);

        // Find our ray's intersection through the selected layer
        if (result)
        {
            threeDCursor.transform.position = hit.point ;
            Vector3 viewPos = mainCam.WorldToViewportPoint(threeDCursor.transform.position);
          
            Shader.SetGlobalVector("_HandScreenPos", new Vector4(viewPos.x, viewPos.y, 0, 0));
        }
    }

}
