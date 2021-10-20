using System.Collections;
using System.Collections.Generic;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
