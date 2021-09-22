using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sight : Sense
{
    [Range(0.0f, 360.0f)]
    public int fieldOfView = 90;
    [Range(0.0f, 100.0f)]
    public int viewRadius = 100;

    public LayerMask targetMask;
    public LayerMask obstacleMask;


    //private Transform targetTransform;
    //private Vector3 rayDirection;

    protected override void Initialize()
    {
        //targetTransform = GameObject.Find("Target").transform;
    }
    public Vector3 AngleDirection(float angleDegrees, bool isGlobal)
    {
        if (!isGlobal)
        {
            angleDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleDegrees * Mathf.Deg2Rad), 0.0f, Mathf.Cos(angleDegrees * Mathf.Deg2Rad));
    }

    protected override void DetectAspect()
    {
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        foreach (var targetInRadius in targetsInRadius)
        {
            Transform target = targetInRadius.transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;

            // disregard objects outside of fov angle
            if (Vector3.Angle(transform.forward, dirToTarget) < fieldOfView / 2)
            {
                float targetDistance = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, targetDistance, obstacleMask))
                {
                    detectedTargets.Add(target.gameObject);
                }
            }

        }


        //RaycastHit hit;
        //// get angle between target and npc
        //rayDirection = targetTransform.position - transform.position;

        //// check if angle is in the fov
        //if ((Vector3.Angle(rayDirection, transform.forward)) < fieldOfView)
        //{
        //    // check for unobstructed line of sight in range
        //    if (Physics.Raycast(transform.position, rayDirection, out hit, viewRadius))
        //    {
        //        Aspect aspect = hit.collider.GetComponent<Aspect>();
        //        if (aspect != null && aspect.aspectType != aspectName)
        //        {
        //            detected.Add(aspect.gameObject);
        //        }
        //    }
        //}
    }
}
