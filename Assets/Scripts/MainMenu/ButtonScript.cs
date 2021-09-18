using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public class ButtonScript : MonoBehaviour
{
    public void PlayGame()
    {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            FindObjectOfType<AudioManager>().Stop("MainMenuBackground");
            FindObjectOfType<AudioManager>().Play("GameStart");
    }

    public void Load()
    {
        Save save = LoadByXML();
        PlayerPrefs.SetFloat("PlayerPosX", save.playerPositionX);
        PlayerPrefs.SetFloat("PlayerPosY", save.playerPositionY);
        PlayerPrefs.SetFloat("PlayerPosZ", save.playerPositionZ);
        PlayGame();
    }

    public Save LoadByXML()
    {
        if (File.Exists(Application.dataPath + "/XMLSaves/" + "Roger" + ".xml"))
        {
            
            Save save = new Save();
            XmlSerializer serializer = new XmlSerializer(typeof(Save));
            FileStream stream = new FileStream(Application.dataPath + "/XMLSaves/" + "Roger" + ".xml", FileMode.Open);
            save = serializer.Deserialize(stream) as Save;
            stream.Close();
            Debug.Log(save.playerPositionX);
            return save;
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
            Save save = new Save();
            Debug.Log("XML SAVE FILE NOT FOUND");
            return save;
        }
    }

    public void Quit()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();
    }
}
