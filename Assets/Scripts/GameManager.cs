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
    public ChildrenActive popUpMenus;

    private PolaroidController polC;

    public void Awake()
    {
        polC = cameraC.GetComponent<PolaroidController>();
    }

    public void Update()
    {

        HandlePlayerInput();
        
        HandleCameraInputs();
    }

    private void HandlePlayerInput()
    {
        //this needs jesus
        switch (popUpMenus.ActiveChildren())
        {
            // if pause - lock out other menus

            case "":
                polC.inputAllowed = true;
                playerC.UpdateMove();
                playerC.UpdateLook();
                break;
            default:
                polC.inputAllowed = false;
                break;
        }
    }

    private void HandleCameraInputs()
    {
        if (popUpMenus.noChildrenActive)
        {
            polC.inputAllowed = true;
        } else
        {
            polC.inputAllowed = false;
        }
    }
}
