using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GroveToWilderness : MonoBehaviour
{
    public GameObject toWildernessGUI;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ConfirmWildernessTravel()
    {
        // Load next scene
        Debug.Log("Going to next scene");
        SceneManager.LoadScene("worldGen");
        toWildernessGUI.SetActive(false);
        
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
}
