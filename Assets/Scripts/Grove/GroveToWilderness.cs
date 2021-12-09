using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GroveToWilderness : MonoBehaviour
{
    [Header("SceneChangeObjs")]
    public GameObject toWildernessGUI;
    public GameObject player;
    public GameObject groveVisual;

    private ReturnToGrove rTG;
    // Start is called before the first frame update
    private void Awake()
    {
        rTG = gameObject.GetComponent<ReturnToGrove>();
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
        
        GameObject terrainGO = GameObject.Find("TerrainManager");

        TerrainManager terrainM = terrainGO.GetComponent<TerrainManager>();
        terrainM.player = player;
        
        Vector3 currPos = player.transform.position;
        player.transform.position = new Vector3(currPos.x, currPos.y + 50f, currPos.z);

        // tell spawn manager to begin spawning
        GameObject spawnerGO = GameObject.Find("SpawnManager");
        SpawnManager spawnM = spawnerGO.GetComponent<SpawnManager>();
        spawnM.BeginSpawning(player.transform);

        // disable the worldgen player
        GameObject worldGenPlayer = GameObject.Find("WorldGenPlayer");
        worldGenPlayer.SetActive(false);

        //This would be the spot for a loading screen or something
        // Pass up to GameManager
        groveVisual.SetActive(false);
        toWildernessGUI.SetActive(false);
        rTG.WildernessLoaded();
        //SceneManager.MoveGameObjectToScene(player, wilderness);
        // SceneManager.MoveGameObjectToScene(player, wilderness);
    }
}
