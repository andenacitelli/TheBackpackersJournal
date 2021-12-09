using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Preload : MonoBehaviour
{
    [HideInInspector]
    public GameObject LoadMenu;

    void Start()
    {
        PlayerPrefs.SetFloat("PlayerPosX", 0f);
        PlayerPrefs.SetFloat("PlayerPosY", 0.5f);
        PlayerPrefs.SetFloat("PlayerPosZ", 0f);

        CreatePath(Application.persistentDataPath + "/XMLSaves/");
        //CreatePath(Application.persistentDataPath + "/PhotoStorage/");
    }

    /// <summary>
    /// Creates a directory at the given path
    /// </summary>
    /// <param name="pathName">the path to create</param>
    public static void CreatePath(string pathName)
    {
        if (!Directory.Exists(pathName))
        {
            Debug.Log($"Path: {pathName} does not exist. Attempting to create path.");
            DirectoryInfo dir = Directory.CreateDirectory(pathName);
            if (Directory.Exists(pathName)) Debug.Log($"Created path {pathName}");
            else Debug.LogWarning($"Could not create path {pathName}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
