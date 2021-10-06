using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hovering : MonoBehaviour
{
/*    public float speed = 50;*/

    void Start()
    {   
    }
    void Update()
    {
        Quaternion q = new Quaternion(0, 1, 0, 0);
        this.transform.Rotate(new Vector3(0, 20, 0) * Time.deltaTime, Space.World);
    }
}
