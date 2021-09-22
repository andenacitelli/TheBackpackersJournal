using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hearing : Sense
{
    [Header("Sound Detect Distance")]
    [Range(0.0f, 100.0f)]
    public float hearingRadius = 10.0f;
    // check range for heard things
    protected override void DetectAspect()
    {
        // get colliders in the hearing range
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, hearingRadius);
       
        GameObject detectedObject;
        Aspect aspect;

        foreach (var targetInRadius in targetsInRadius)
        {
            detectedObject = targetInRadius.gameObject;

            // ignore self
            //if(detectedObject != transform.gameObject) 
            //{
                // ignore same aspects
                if (detectedObject.TryGetComponent<Aspect>(out aspect))
                {
                    detectedTargets.Add(detectedObject);
                }
            //}
        }
    }
}
