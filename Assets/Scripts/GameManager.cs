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
    [Header("Game")]
    public GalleryStorage storage;
    

    [Header("UI")]
    public Canvas uiCanvas;
    public ChildrenActive popUpMenus;

    private InteractableGUI hud;
    private PolaroidController polC;
    private CameraRollMenu crMenu;
    private string lastPopUp;
    private bool lastEditOn;

    public void Awake()
    {
        polC = cameraC.GetComponent<PolaroidController>();
        hud = uiCanvas.GetComponent<InteractableGUI>();
        crMenu = uiCanvas.GetComponent<CameraRollMenu>();
        lastPopUp = "";
        lastEditOn = false;
    }

    public void Update()
    {
        DetermineInputState();
        DetermineGameState();
    }

    private void DetermineInputState() 
    {
        string activePopUp = popUpMenus.ActiveChildren();
        if (activePopUp.Equals(""))
        {
            if (storage.editingStorageOn)
            {
                if (!lastEditOn)
                {
                    //edit mode just turned on
                    hud.ToggleCursor(false, true, storage.scaleModifier);
                } else
                {
                    HandleStorageEditingInput();
                }
                
            } else
            {
                if (lastEditOn)
                {
                    //edit mode just turned off
                    hud.ToggleCursor(false, false, -1);
                }
                else
                {
                    HandlePlayerInput();
                }
                
            }
            
            
        } else if(lastPopUp.Equals(""))
        {
            // Pop Up was opened in last update:
            ToggleCameraInputs(false);
            ToggleCameraRollInputs(false);
            hud.ToggleCursor(false, false, -1);
            
        }
        lastPopUp = activePopUp;
        lastEditOn = storage.editingStorageOn;
    
    }

    private void DetermineGameState()
    {

    }
    private void HandlePlayerInput()
    {
        hud.ToggleCursor(true, false, -1);
        ToggleCameraInputs(true);
        ToggleCameraRollInputs(true);
        playerC.UpdateMove();
        playerC.UpdateLook();   
    }

    private void HandleStorageEditingInput()
    {
        hud.ToggleCursor(true, true, storage.scaleModifier);
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
