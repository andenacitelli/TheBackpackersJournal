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
    public PauseMenu pMenu;

    [Header("UI")]
    public Canvas uiCanvas;
    public ChildrenActive popUpMenus;

    private InteractableGUI hud;
    private PolaroidController polC;
    private CameraRollMenu crMenu;
    private string lastPopUp;
    private bool lastEditOn;
    private Save saveInfo;
    private LoadGame lMenu;
    private WelcomeMenu wMenu;
    private CameraRoll cr;
    public void Awake()
    {
        //DontDestroyOnLoad(this);
        lMenu = GetComponent<LoadGame>();

        int profileIndex;
        if (PlayerPrefs.HasKey("SaveIndex"))
        {
            profileIndex = PlayerPrefs.GetInt("SaveIndex");
        } else
        {
            profileIndex = -1;
        }

        var newSave = lMenu.GetSaveForGame(profileIndex);
        cr = cameraC.GetComponent<CameraRoll>();
        if (newSave == null)
        {
            Debug.Log("No file to load - fresh start");
            wMenu = uiCanvas.GetComponent<WelcomeMenu>();
            wMenu.GUIOn();
        } else
        {
            AssignSaveOnStart(newSave);
            cr.LoadCRoll(newSave);
        }
        
        polC = cameraC.GetComponent<PolaroidController>();
        
        hud = uiCanvas.GetComponent<InteractableGUI>();
        crMenu = uiCanvas.GetComponent<CameraRollMenu>();
        lastPopUp = "";
        lastEditOn = false;
    }
    private void OnDisable()
    {
        // Gottta override this or it will persist between plays
        PlayerPrefs.DeleteKey("SaveIndex");
        PlayerPrefs.DeleteKey("profileName");
    }
    public void Update()
    {
        DetermineInputState();
        DetermineGameState();
    }

    private void AssignSaveOnStart(Save newSave)
    {
        saveInfo = newSave;
        PlayerPrefs.SetString("profileName", saveInfo.playerName);
        Debug.Log("Profile Name: " + saveInfo.playerName + "saved to player prefs.");  
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
