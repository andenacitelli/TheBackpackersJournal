using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    /* things that were being checked in pausemenu, polaroidcontroller, playercontroller, etc.
     *     public GameObject pauseObj;
           public GameObject savePromptObj;
           public GameObject camerRollUI;
           public GameObject camerRollPopUp;
     * 
     */
    //[Header("Save/Load")]

    [Header("Player")]
    public PlayerController playerC;
    public GameObject cameraC;
    //[Header("Game")]

    [Header("UI")]
    public Canvas uiCanvas;
    public ChildrenActive popUpMenus;

    private PolaroidController polC;
    private CameraRollMenu crMenu;
    private string lastPopUp;

    public void Awake()
    {
        polC = cameraC.GetComponent<PolaroidController>();
        crMenu = uiCanvas.GetComponent<CameraRollMenu>();
        lastPopUp = "";
    }

    public void Update()
    {
        DetermineInputState();
    }

    private void DetermineInputState() 
    {
        string activePopUp = popUpMenus.ActiveChildren();
        if (activePopUp.Equals(""))
        {
            HandlePlayerInput();
            
        } else if(lastPopUp.Equals(""))
        {
            // Pop Up was opened in last update:
            ToggleCameraInputs(false);
            ToggleCameraRollInputs(false);
            
        }
        lastPopUp = activePopUp;
    
    }

    private void HandlePlayerInput()
    {
        ToggleCameraInputs(true);
        ToggleCameraRollInputs(true);
        playerC.UpdateMove();
        playerC.UpdateLook();   
    }

    private void ToggleCameraInputs(bool pass)
    {
        polC.inputAllowed = pass;
    }

    private void ToggleCameraRollInputs(bool pass)
    {
        crMenu.canOpen = pass;
    }
}
