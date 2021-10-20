using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("Gallery")]
    public GameObject galleryGO;

    private GalleryStorage galleryStorage;

    public void Start()
    {
        galleryStorage = galleryGO.GetComponent<GalleryStorage>();
    }
    public void PlayInteractAnim()
    {
        var pos = transform.position;
        Vector3 pressedPos = new Vector3(pos.x, pos.y, pos.z - .09f);
        transform.Translate(pressedPos);
    }

    public void CallUIEvent()
    {
        Debug.Log("CAlled UI EVENT");
        galleryStorage.StartGalleryStorage();
    }
}
