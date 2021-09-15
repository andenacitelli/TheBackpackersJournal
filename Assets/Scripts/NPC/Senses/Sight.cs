using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : Sense
{
    public int fov = 45;
    public int range = 100;

    private Transform targetTransform;
    private Vector3 rayDirection;

    protected override void Initialize()
    {
        targetTransform = GameObject.Find("Target").transform;
    }

    protected override void UpdateSense()
    {
        elapsedTime += Time.deltaTime;

        if(elapsedTime >= detectRate)
        {
            DetectAspect();
            elapsedTime = 0;
        }
    }

    void DetectAspect()
    {
        RaycastHit hit;
        // get angle between target and npc
        rayDirection = targetTransform.position - transform.position;

        // check if angle is in the fov
        if ((Vector3.Angle(rayDirection, transform.forward)) < fov)
        {
            // check for unobstructed line of sight in range
            if(Physics.Raycast(transform.position, rayDirection, out hit, range))
            {
                Aspect aspect = hit.collider.GetComponent<Aspect>();
                if (aspect != null && aspect.aspectType != aspectName)
                {
                    // report what was seen
                    Debug.Log($"I CAN SEE YOU, YOU {aspect.aspectType} BASTARD!");
                }
            }
        }
    }
}
