using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    Camera mainCam;

    [Header("FPS Camera Controls")]
    public bool isActive;
    private bool activeAtStart;
    float hRot, vRot;
    public float sensitivityX = 1f;
    public float sensitivityY = 1f;
    public bool invertX, invertY;

    [Header("Clamp Rotation")]
    public bool clamps;
    public float minX, maxX;
    public float minY, maxY;


    private Cinemachine.CinemachineVirtualCamera cinemachineCam;
    private Cinemachine.CinemachineTrackedDolly trackedDolly;
    void Awake()
    {
        mainCam = Camera.main;
        activeAtStart = isActive;
        cinemachineCam = GetComponent<Cinemachine.CinemachineVirtualCamera>();
        trackedDolly = cinemachineCam.GetComponent<Cinemachine.CinemachineTrackedDolly>();
    }

    void Update()
    {
        //for viewing with cam
        if (isActive)
        {
            CameraRotation();
        }
    }

    void CameraRotation()
    {
        if (clamps)
        {
            //mouse
            hRot += Input.GetAxis("Mouse X") * sensitivityX;
            vRot += Input.GetAxis("Mouse Y") * sensitivityY;

            //clamp Y - horizontal
            hRot = Mathf.Clamp(hRot, minY, maxY);
            //clamp X - vertical
            vRot = Mathf.Clamp(vRot, minX, maxX);
            //horizontal parent axis  - Y
            transform.parent.rotation = Quaternion.Euler(0f, hRot, 0f);
            //vertical camera axis - X
            transform.localRotation = Quaternion.Euler(-vRot, 0f, 0f);

            
        }
        //free rotations without clamp
        else
        {
            //mouse
            hRot = sensitivityX * Input.GetAxis("Mouse X");
            vRot = sensitivityY * Input.GetAxis("Mouse Y");

            //neg value 
            if (invertX)
                hRot *= -1f;
            //neg value 
            if (invertY)
                vRot *= -1f;

            //Rotates Player on "X" Axis Acording to Mouse Input
            transform.parent.Rotate(0, hRot, 0);
            //Rotates Player on "Y" Axis Acording to Mouse Input
            transform.Rotate(vRot, 0, 0);
        }

        
    }
}
