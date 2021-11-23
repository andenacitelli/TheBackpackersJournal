using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    Light sun;

    // Length of Day (IN SECONDS) 
    [SerializeField]
    private float DayTime;

    // Length of Night (IN SECONDS) 
    [SerializeField]
    private float NightTime;

    // Tracks our current time through the cycle (IN SECONDS) 
    private float t;

    void Start()
    {
        sun = GetComponent<Light>();
    }

    void Update()
    {
        // Iterate time 
        t += Time.deltaTime; 
        if (t >= DayTime + NightTime)
        {
            t = 0; 
        }

        // If it's currently day, set as proportion
        if (t <= DayTime)
        {
            // Scales from 0 at t=0 to 1 at t=DayTime/2 to 0 at t=DayTime
            sun.intensity = 1 - (Mathf.Abs(t - (DayTime / 2))) / (DayTime / 2);

            // Rotate directional light relative 
            sun.transform.Rotate(new Vector3((Time.deltaTime / DayTime) * 180, 0, 0));
        }

        // Night time sets intensity to zero
        else
        {
            // sun.transform.Rotate(new Vector3((Time.deltaTime / NightTime) * 180, 0, 0)); 
            sun.transform.eulerAngles = Vector3.down; 
            // sun.intensity = 0; 
        }
    }
}
