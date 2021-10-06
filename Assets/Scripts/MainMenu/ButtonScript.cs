using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ButtonScript : MonoBehaviour
{
    public GameObject LoadMenu;
    public GameObject MainMenu;
    public GameObject SettingsMenu;
    public void PlayGame()
    {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            FindObjectOfType<AudioManager>().Stop("MainMenuBackground");
            FindObjectOfType<AudioManager>().Play("GameStart");
    }

    public void Load()
    {
        MainMenu.SetActive(false);
        LoadMenu.SetActive(true);

    }

    public void BackToMainMenu()
    {
        MainMenu.SetActive(true);
        LoadMenu.SetActive(false);
    }

    public void Settings()
    {
        MainMenu.SetActive(false);
        SettingsMenu.SetActive(true);
    }

    public void SettingsBackToMainMenu()
    {
        MainMenu.SetActive(true);
        SettingsMenu.SetActive(false);
    }

    public void Quit()
    {

        Debug.Log("Quit button pressed");
        Application.Quit();
    }
}
