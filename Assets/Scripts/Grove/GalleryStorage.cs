using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalleryStorage : MonoBehaviour
{
    
    public GameObject galleryStorageUI;
    public GameObject polaroidCamera;
    public bool isOn { get; set; }
    private CameraRoll cameraRoll;
    private CameraRollMenu cameraRollUI;
    private List<photo> gallery = new List<photo>();
    private GameObject startStorageUI;

    private void Start()
    {
        startStorageUI = galleryStorageUI.transform.GetChild(0).gameObject;
        cameraRoll = polaroidCamera.GetComponent<CameraRoll>();
        cameraRollUI = cameraRoll.crUI;
        isOn = false;
    }
    public void StartGalleryStorage()
    {
        Debug.Log("Started Gallery");
        galleryStorageUI.SetActive(true);
        isOn = true;
    }

    public void StartCRtoStorageConvert()
    {
        cameraRollUI.OpenCRStorage();
    }

    public void ExitGalleryStorage()
    {
        galleryStorageUI.SetActive(false);
        startStorageUI.SetActive(true);
    }
}
