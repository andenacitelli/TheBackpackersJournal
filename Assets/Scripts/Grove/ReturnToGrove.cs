using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToGrove : MonoBehaviour
{
    public bool playerRoaming {get; set;}

    private GroveToWilderness gTW;

    private void Awake()
    {
        gTW = gameObject.GetComponent<GroveToWilderness>();
    }


    public void UserReturn()
    {
        SceneManager.UnloadSceneAsync("worldGen");
        gTW.groveVisual.SetActive(true);
        gTW.player.transform.position = new Vector3(0, 5, 0);  
        playerRoaming = false;
    }

    public void WildernessLoaded()
    {
        playerRoaming = true;
    }
}
