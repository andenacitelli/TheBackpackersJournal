using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Xml;
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
        LoadByXML();
    }

    public void LoadByXML()
    {
        if (File.Exists(Application.dataPath + "/DataXML.text"))
        {
            Save save = new Save();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Application.dataPath + "/DataXML.text");

            XmlNodeList playerPosX = xmlDoc.GetElementsByTagName("PlayerPositionX");
            float playerPositionX = float.Parse(playerPosX[0].InnerText);
            save.playerPositionX = playerPositionX;

            XmlNodeList playerPosY = xmlDoc.GetElementsByTagName("PlayerPositionY");
            float playerPositionY = float.Parse(playerPosY[0].InnerText);
            save.playerPositionY = playerPositionY;

            XmlNodeList playerPosZ = xmlDoc.GetElementsByTagName("PlayerPositionZ");
            float playerPositionZ = float.Parse(playerPosZ[0].InnerText);
            save.playerPositionZ = playerPositionZ;

            /*Load the game*/
        }
        else
        {
            Debug.Log("XML FILE NOT FOUND");
        }
    }

    public void Quit()
    {
        Debug.Log("Quit button pressed");
        Application.Quit();
    }
}
