using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GroveToWilderness : MonoBehaviour
{
    public GameObject toWildernessGUI;
    public GameObject player;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ConfirmWildernessTravel()
    {
        StartCoroutine(LoadWilderness());
        
        
        
        
    }

    public void DenyWildernessTravel()
    {
        // Set player back in Grove
        toWildernessGUI.SetActive(false);
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        toWildernessGUI.SetActive(true);
        Vector3 resetPos = new Vector3(0, .5f, 0);
        other.transform.position = resetPos;
    }

    private IEnumerator LoadWilderness()
    {
        // Load next scene
        Debug.Log("Going to next scene");
        /*
        Scene buffer = SceneManager.GetSceneByName("DontDestroyOnLoad");
        SceneManager.MoveGameObjectToScene(player, buffer);
        */
        SceneManager.LoadScene("worldGen", LoadSceneMode.Additive);
        yield return new WaitForEndOfFrame();

        Scene thisScene = SceneManager.GetSceneByName("firstperson");
        Scene wilderness = SceneManager.GetSceneByName("worldGen");
        yield return new WaitForEndOfFrame();
        GameObject additiveCam = GameObject.Find("WorldGenPlayer");
        additiveCam.SetActive(false);
        GameObject terrainGO = GameObject.Find("TerrainManager");

        TerrainManager terrainM = terrainGO.GetComponent<TerrainManager>();
        terrainM.player = player;
        
        Vector3 currPos = player.transform.position;
        player.transform.position = new Vector3(currPos.x, currPos.y + 50f, currPos.z);
        toWildernessGUI.SetActive(false);
        //SceneManager.MoveGameObjectToScene(player, wilderness);
        // SceneManager.MoveGameObjectToScene(player, wilderness);
    }
}
