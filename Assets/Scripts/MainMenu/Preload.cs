using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Preload : MonoBehaviour
{
    // Start is called before the first frame update
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
