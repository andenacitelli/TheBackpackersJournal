using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRollMenu : MonoBehaviour
{
    public static bool isCRMenu = false;
    public GameObject cameraRollUI;

    PauseAction action;

    private void Awake()
    {
        action = new PauseAction();
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
   
    void OpenCR()
    {
        isCRMenu = true;
        cameraRollUI.SetActive(true);
        Time.timeScale = 0f;
    }

}
