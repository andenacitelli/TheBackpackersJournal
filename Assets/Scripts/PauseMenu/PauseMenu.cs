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
    public CameraRoll cr;
    public GameObject groveReturnPrompt;
    public GameObject groveReturn;

    private string input;
    private ReturnToGrove rTG;
    

    private void Awake()
    {
        action = new PauseAction();
        rTG = groveReturn.GetComponent<ReturnToGrove>();
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

    public void DeterminePause()
    {  
        if(!isSaving){
            if (isPaused)
                Resume();
            else
                Pause();
                DetermineReturnPrompt();
        }      
    }

    private void DetermineReturnPrompt()
    {
        if (rTG.playerRoaming)
        {
            groveReturnPrompt.SetActive(true);
        } else
        {
            groveReturnPrompt.SetActive(false);
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

    public void AutoSave()
    {
        SaveByXML(PlayerPrefs.GetString("profileName"));
        Resume();
    }

    public void SaveByXML(string s)
    {
        if (s != "")
        {
            if (cr.cRollStorage == null)
            {
                // No load was ran, because player has no data.
                cr.cRollStorage = new List<photo>();
            } else
            {
                // Check to be sure target directory exists
                DirectoryInfo crInfo = new DirectoryInfo(Application.persistentDataPath + "/PhotoStorage/" + s + "/CameraRoll/");
                DirectoryInfo grInfo = new DirectoryInfo(Application.persistentDataPath + "/PhotoStorage/" + s + "/GalleryRoll/");
                // probably will need to add business here for the journal
                if (!crInfo.Exists)
                {
                    crInfo.Create();
                }
                if (!grInfo.Exists)
                {
                    grInfo.Create();
                }
            }

            print("profileName pref set to: " + s);
            PlayerPrefs.SetString("profileName", s);
            Save save = createSaveGameObject(s);
            XmlDocument xmlDoc = new XmlDocument();
            XmlSerializer serializer = new XmlSerializer(typeof(Save));
            string dataPath = Application.persistentDataPath + "/XMLSaves/";
            DirectoryInfo xmlSaveInfo = new DirectoryInfo(dataPath);
            if (!xmlSaveInfo.Exists)
            {
                xmlSaveInfo.Create();
            }
            FileStream stream = new FileStream(dataPath+ s + ".xml", FileMode.Create);
            serializer.Serialize(stream, save);
            stream.Close();
            savePrompt.SetActive(false);
            pauseMenuUI.SetActive(true);
            isSaving = false;

            
            foreach (photo p in save.crTest)
            {
                if(cr.profileName != s)
                {
                    cr.profileName = s;
                }
                cr.WriteFile(p.fileName, p.captureData);
            }
        }

        
    }

    private Save createSaveGameObject(string s)
    {
        Save save = new Save();
        save.playerName = s;
        save.playerPositionX = GameObject.FindWithTag("Player").transform.position.x;
        save.playerPositionY = GameObject.FindWithTag("Player").transform.position.y;
        save.playerPositionZ = GameObject.FindWithTag("Player").transform.position.z;

        #region getArray
        string[] crPaths = new string[cr.cRollStorage.Count];
        int i = 0;
        Debug.Log("Saving...");
        foreach(photo p in cr.cRollStorage)
        {
            Debug.Log(p.fileName);
            crPaths[i] = p.fileName;
            i++;
        }
        Debug.Log("-Finished-");
        #endregion
        //save.cameraRollPaths = crPaths;
        save.crTest = cr.cRollStorage.ToArray();
        save.GamePercentage = 0;
        return save;

    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ReturnToGrove()
    {
        Debug.Log("Return to Grove Started");
        rTG.UserReturn();
        Resume();
    }

    public void Quit()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();
    }

}
