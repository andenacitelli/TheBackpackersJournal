using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRollMenu : MonoBehaviour
{
    public static bool isCRMenu = false;
    [Header("UI")]
    public GameObject cameraRollUI;
    public GameObject capturePopUpUI;

    PauseAction action;

    private CRUIGallery gallery;
    private CRPopUp popUp;

    private void Awake()
    {
        action = new PauseAction();
        gallery = cameraRollUI.GetComponent<CRUIGallery>();
        popUp = capturePopUpUI.GetComponent<CRPopUp>();
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
        if (!isCRMenu)
        {
            OpenCR();
        }
        else
        {
            CloseCR();
        }
    }

    public void CloseCR()
    {
        isCRMenu = false;
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
        Debug.Log("Kept");
        capturePopUpUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ClosePopUpDiscard()
    {
        Debug.Log("Discarded");
        capturePopUpUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void UpdateCR(int guiIndex, Texture2D updateData)
    {
        gallery.AssignPicture(guiIndex, updateData);
    }

    public void UpdatePopUp(Texture2D updateData)
    {
        popUp.AssignPicture(updateData);
    }

}
