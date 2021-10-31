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
        yield return new WaitForSeconds(2);
        SceneNumber = 1;
        Destroy(PoweredByUnity);
        CreditScreen.SetActive(true);
    }

    IEnumerator ToMainMenu()
    {
        yield return new WaitForSeconds(3);
        Destroy(CreditScreen);
        Destroy(PoweredByUnity);
        Destroy(SplashScreenCam);
        MainMenuCam.SetActive(true);
        MainMenu.SetActive(true);
        GameObject.Find("WorldGenCamera").GetComponent<Camera>().GetComponent<AudioListener>().enabled = true;
        SceneNumber = 2;
    }

}
