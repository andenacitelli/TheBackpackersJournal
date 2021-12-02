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
    
    [XmlArrayAttribute("CameraRoll")]
    public photo[] crTest;
    [XmlArrayAttribute("GalleryRoll")]
    public photo[] gallRoll;
    [XmlArrayAttribute("JournalRoll")]
    public photo[] jRoll;
    /*ToDo: Add features of the player profile*/
}
