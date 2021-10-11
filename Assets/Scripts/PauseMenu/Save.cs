using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[System.Serializable]
public class Save
{
    public string playerName;
    public int GamePercentage;

    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;
    public string[] cameraRollPaths;
    /*ToDo: Add features of the player profile*/
}
