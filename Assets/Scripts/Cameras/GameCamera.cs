using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Base class for game cameras

namespace Cameras
{
    public class GameCamera : MonoBehaviour
    {
        CameraManager manager;
        public FadeUI shiftUI;

        private void Awake()
        {
            manager = FindObjectOfType<CameraManager>();
        }
    }
}

