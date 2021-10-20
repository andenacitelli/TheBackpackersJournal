using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class OverlayMainmenu : MonoBehaviour
{
    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            this.GetComponent<AudioListener>().enabled = false;
            Camera cam = GameObject.Find("MainmenuCam").GetComponent<Camera>();
            var cameraData = GetComponent<Camera>().GetUniversalAdditionalCameraData();
            cameraData.cameraStack.Add(cam);
        }

    }
}
