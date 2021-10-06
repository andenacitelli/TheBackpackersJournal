using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
public class OverlayMainmenu : MonoBehaviour
{
    private void Start()
    {
        Camera overlayCam = new Camera();
        Camera[] camArray = Camera.allCameras;
        foreach (Camera cam in camArray)
        {
            if (cam.gameObject.name == "MainmenuCam")
                overlayCam = cam;
        }
        var cameraData = GetComponent<Camera>().GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Add(overlayCam);
    }
}
