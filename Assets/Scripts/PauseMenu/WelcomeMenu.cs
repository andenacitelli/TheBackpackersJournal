using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WelcomeMenu : MonoBehaviour
{
    public GameObject welcomeGUI;

    private PauseMenu pMenu;
    private void Awake()
    {
        pMenu = GetComponent<PauseMenu>();
    }

    // Update is called once per frame
    public void GUIOn()
    {
        
        welcomeGUI.SetActive(true);
    }

    public void StartGame()
    {
        pMenu.DeterminePause();
        pMenu.Save();
        GUIOff();
    }

    public void GUIOff()
    {
        welcomeGUI.SetActive(false);
    }
}
