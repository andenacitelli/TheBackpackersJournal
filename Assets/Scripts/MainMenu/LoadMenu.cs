using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TMPro;

public class LoadMenu : MonoBehaviour
{

    public ButtonScript myButtonScript;
    public GameObject[] buttons;
    string savesPath;

    private string[] filenames = new string[10];
    public void Awake()
    {
        savesPath = Application.persistentDataPath + "/XMLSaves/";

        if (!Directory.Exists(savesPath)) Debug.LogError($"Path {savesPath} not found.");

        string[] XMLfiles = Directory.GetFiles(savesPath, "*.XML");

        int i = 0;
        foreach (string file in XMLfiles)
        {
            buttons[i].SetActive(true);
            buttons[i].transform.GetChild(0).GetComponent<TMP_Text>().text = Path.ChangeExtension(Path.GetFileName(file), null);
            filenames[i] = Path.ChangeExtension(Path.GetFileName(file), null);
            i++;
        }
    }

    public void LoadByXML(int i)
    {
        Save save = new Save();
        string file = filenames[i];
        if (File.Exists(savesPath + file + ".xml"))
        {
            PlayerPrefs.SetInt("SaveIndex", i);
            myButtonScript.PlayGame();

            //No need to deserialize before pertinent scene loads - handle on wakeup, just capture input now
            /*XmlSerializer serializer = new XmlSerializer(typeof(Save));
            FileStream stream = new FileStream(Application.persistentDataPath + "/XMLSaves/" + file + ".xml", FileMode.Open);
            save = serializer.Deserialize(stream) as Save;
            stream.Close();
            Debug.Log(save.playerPositionX);*/

            /*            XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(Application.dataPath + "/DataXML.text");

                        XmlNodeList playerPosX = xmlDoc.GetElementsByTagName("PlayerPositionX");
                        float playerPositionX = float.Parse(playerPosX[0].InnerText);
                        save.playerPositionX = playerPositionX;

                        XmlNodeList playerPosY = xmlDoc.GetElementsByTagName("PlayerPositionY");
                        float playerPositionY = float.Parse(playerPosY[0].InnerText);
                        save.playerPositionY = playerPositionY;

                        XmlNodeList playerPosZ = xmlDoc.GetElementsByTagName("PlayerPositionZ");
                        float playerPositionZ = float.Parse(playerPosZ[0].InnerText);
                        save.playerPositionZ = playerPositionZ;*/

            /*Load the game*/
        }
        else
        {
            Debug.Log("XML SAVE FILE NOT FOUND");
            return;
        }
        /*
        PlayerPrefs.SetFloat("PlayerPosX", save.playerPositionX);
        PlayerPrefs.SetFloat("PlayerPosY", save.playerPositionY);
        PlayerPrefs.SetFloat("PlayerPosZ", save.playerPositionZ);
        */
        
    }
}
