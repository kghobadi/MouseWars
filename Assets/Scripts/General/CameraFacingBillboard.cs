using UnityEngine;
using System.Collections;

//This script makes any Sprite object look at the player's camera with the correct orientation from Gravity 
public class CameraFacingBillboard : MonoBehaviour
{
    Camera playerCam;
    SpriteRenderer sr;
    void Awake()
    {
        //cam refs
        playerCam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
    }

	void Update()
	{
		//fp -- look at cam
        transform.LookAt(playerCam.transform.position, playerCam.transform.up);
	}

}
