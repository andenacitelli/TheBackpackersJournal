using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashSequence : MonoBehaviour
{
    public int SceneNumber;
    public GameObject PoweredByUnity;
    public GameObject MainMenu;
    public GameObject CreditScreen;
    public GameObject SplashScreenCam;
    public GameObject MainMenuCam;

    private GameObject additivePlayer;
    private GameObject additiveCam;

    void Start()
    {
        SceneManager.LoadScene("worldGen", LoadSceneMode.Additive);
        
        if (SceneNumber == 0)
        {
            StartCoroutine(ToSplashTwo());
        }
        
    }

    void Update()
    {
        if (SceneNumber == 1)
        {
            StartCoroutine(ToMainMenu());
        }
    }


    IEnumerator ToSplashTwo()
    {
        yield return new WaitForSeconds(1);
        SceneNumber = 1;
        additivePlayer = GameObject.Find("WorldGenPlayer");
        Destroy(PoweredByUnity);
        CreditScreen.SetActive(true);
    }

    IEnumerator ToMainMenu()
    {
        yield return new WaitForSeconds(1);
        additiveCam = additivePlayer.transform.GetChild(0).gameObject;
        Destroy(CreditScreen);
        Destroy(PoweredByUnity);
        Destroy(SplashScreenCam);
        MainMenuCam.SetActive(true);
        MainMenu.SetActive(true);
        
        //additiveCam.transform.parent.gameObject.SetActive(true);
        additiveCam.SetActive(true);
        MainMenuCam.GetComponent<Camera>().GetComponent<AudioListener>().enabled = true;
        SceneNumber = 2;
    }

}
