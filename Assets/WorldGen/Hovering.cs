using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hovering : MonoBehaviour
{
/*    public float speed = 50;*/

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
            this.GetComponent<FlyCamController>().enabled = false;
    }
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Quaternion q = new Quaternion(0, 1, 0, 0);
            this.transform.Rotate(new Vector3(0, 20, 0) * Time.deltaTime, Space.World);
        }

    }
}
