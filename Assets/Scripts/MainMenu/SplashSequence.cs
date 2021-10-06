using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashSequence : MonoBehaviour
{
    public static int SceneNumber;
    void Start()
    {
        if (SceneNumber == 0)
        {
            StartCoroutine(ToSplashTwo());
        }
        if(SceneNumber == 1)
        {
            StartCoroutine(ToMainMenu ());
        }
    }

/*    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null && SceneNumber != 2)
        {
            GameObject.FindGameObjectWithTag("Player").SetActive(false);
        }
        
    }*/

    IEnumerator ToSplashTwo()
    {
        yield return new WaitForSeconds(2);
        SceneNumber = 1;
        SceneManager.LoadScene(1);
    }

    IEnumerator ToMainMenu()
    {
        yield return new WaitForSeconds(3);
        SceneManager.LoadScene(2);
        SceneManager.LoadScene("Main", LoadSceneMode.Additive);
        SceneNumber = 2;
    }

}
