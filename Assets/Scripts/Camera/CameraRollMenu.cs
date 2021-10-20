using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public delegate void KeepPhoto(int val);
public class CameraRollMenu : MonoBehaviour
{
    public CameraRoll cr;
    public static bool isCRMenu = false;
    [Header("UI")]
    public GameObject cameraRollUI;
    public GameObject capturePopUpUI;
    
    public bool canOpen { get; set; }
    PauseAction action;

    private CRUIGallery gallery;
    private GameObject galleryGO;
    private GameObject photoViewGO;
    private CRPopUp popUp;
    private CRPhotoView photoView;
    

    private void Awake()
    {
        action = new PauseAction();
        gallery = cameraRollUI.GetComponent<CRUIGallery>();
        popUp = capturePopUpUI.GetComponent<CRPopUp>();
        photoView = cameraRollUI.GetComponent<CRPhotoView>();
        galleryGO = cameraRollUI.transform.GetChild(0).gameObject;
        photoViewGO = cameraRollUI.transform.GetChild(1).gameObject;
        canOpen = true;
    }

    private void OnEnable()
    {
        action.Enable();
    }

    private void OnDisable()
    {
        action.Disable();
    }

    private void Start()
    {
        action.Pause.CameraRollMenu.performed += _ => DetermineCR();
    }

    private void DetermineCR()
    {
        if (canOpen)
        {
            if (!isCRMenu)
            {
                OpenCR();
            }
            
        } else if(isCRMenu)
        {
            CloseCR();
        }

    }

    public void CloseCR()
    {
        isCRMenu = false;
        galleryGO.SetActive(true);
        photoViewGO.SetActive(false);
        cameraRollUI.SetActive(false);
        Time.timeScale = 1f;
    }
   
    public void OpenCR()
    {
        isCRMenu = true;
        cameraRollUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OpenPopUp()
    {
        capturePopUpUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ClosePopUpKeep()
    {
        capturePopUpUI.SetActive(false);
        gallery.isReplaceMode = true;
        cameraRollUI.SetActive(true);
    }

    public void ClosePopUpDiscard()
    {
        capturePopUpUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RecieveGallerySelect(int option, bool replaceMode)
    {
        
        if(replaceMode)
        {
            gallery.isReplaceMode = false;
            cameraRollUI.SetActive(false);
            KeepPhoto send = cr.KeepCapture;
            send(option);
            Time.timeScale = 1f;
        } else
        {
            
            if(cr.cRollStorage.Count > option)
            {
                galleryGO.SetActive(false);
                UpdateView(cr.cRollStorage[option].captureData, option);
                photoViewGO.SetActive(true);
            }
            
        } 
    }

    public void PhotoViewResponse(int option)
    {
        int count = cr.cRollStorage.Count;
        //where option: 1 - next, -1 - last, 0, return
        if (option == 1)
        {
            
            int newI = photoView.currIndex + 1;
            if(newI == 9 || newI >= count)
            {
                newI = 0;
            }
            UpdateView(cr.cRollStorage[newI].captureData, newI);
        }
        else if (option == -1)
        {
            int newI = photoView.currIndex - 1;
            if (newI == -1)
            {
                newI = count - 1;
            }
            UpdateView(cr.cRollStorage[newI].captureData, newI);
        }
        else
        {
            //return to gallery
            photoViewGO.SetActive(false);
            galleryGO.SetActive(true);
        }
    }

    public void UpdateCR(int guiIndex, Texture2D updateData)
    {
        gallery.AssignPicture(guiIndex, updateData);
    }

    public void UpdatePopUp(Texture2D updateData)
    {
        popUp.AssignPicture(updateData);
    }

    public void UpdateView(Texture2D updateData, int index)
    {
        photoView.AssignPicture(updateData, index);
    }

}
