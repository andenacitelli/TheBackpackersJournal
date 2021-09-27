using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class PauseMenu : MonoBehaviour
{
    public static bool isPaused = false;
    private static bool isSaving = false;
    public GameObject pauseMenuUI;
    PauseAction action;
    public GameObject savePrompt;

    private string input;

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
        action.Pause.PauseGame.performed += _ => DeterminePause();
    }

    private void DeterminePause()
    {
        if(!isSaving){
            if (isPaused)
                Resume();
            else
                Pause();
        }      
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }
    public void Save()
    {
        pauseMenuUI.SetActive(false);
        savePrompt.SetActive(true);
        isSaving = true;
    }
    public void SaveByXML(string s)
    {
        if (s != "")
        {
            Save save = createSaveGameObject(s);
            XmlDocument xmlDoc = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(typeof(Save));
            FileStream stream = new FileStream(Application.dataPath + "/XMLSaves/" + s + ".xml", FileMode.Create);
            serializer.Serialize(stream, save);
            stream.Close();
            savePrompt.SetActive(false);
            pauseMenuUI.SetActive(true);
            isSaving = false;
        }
    }

    private Save createSaveGameObject(string s)
    {
        Save save = new Save();
        save.playerName = s;
        save.playerPositionX = GameObject.FindWithTag("Player").transform.position.x;
        save.playerPositionY = GameObject.FindWithTag("Player").transform.position.y;
        save.playerPositionZ = GameObject.FindWithTag("Player").transform.position.z;
        save.GamePercentage = 0;
        return save;

    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Quit()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();
    }

}
