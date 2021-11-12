using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
[XmlRoot("Player")]
public class Save
{
    [XmlAttribute("profileName")]
    public string playerName;
    [XmlAttribute("jPages")]
    public int GamePercentage;

    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;
    [XmlArray("test"), XmlArrayItem("testItem")]
    public string[] cameraRollPaths;
    [XmlArrayAttribute("CameraRoll")]
    public photo[] crTest;
    /*ToDo: Add features of the player profile*/
}
