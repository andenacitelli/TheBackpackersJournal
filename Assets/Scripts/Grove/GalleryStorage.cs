using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalleryStorage : MonoBehaviour
{
    
    public GameObject galleryStorageUI;

    public bool isOn { get; set; }
    private List<photo> gallery = new List<photo>();
    private GameObject startStorageUI;

    private void Start()
    {
        startStorageUI = galleryStorageUI.transform.GetChild(0).gameObject;
        isOn = false;
    }
    public void StartGalleryStorage()
    {
        Debug.Log("Started Gallery");
        galleryStorageUI.SetActive(true);
        isOn = true;
    }

    public void ExitGalleryStorage()
    {
        galleryStorageUI.SetActive(false);
        startStorageUI.SetActive(true);
    }
}
