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
        }
    }

    void DetectAspect()
    {
        RaycastHit hit;
        rayDirection = targetTransform.position - transform.position;

        if ((Vector3.Angle(rayDirection, transform.forward)) < fov)
        {
            // detect if something can be seen
            if(Physics.Raycast(transform.position, rayDirection, out hit, range))
            {
                Aspect aspect = hit.collider.GetComponent<Aspect>();
                if (aspect != null)
                {
                    // report what was seen
                    if(aspect.aspectType != aspectName)
                    {
                        Debug.Log($"{aspect.aspectType} sighted");
                    }
                }
            }
        }
    }
}
