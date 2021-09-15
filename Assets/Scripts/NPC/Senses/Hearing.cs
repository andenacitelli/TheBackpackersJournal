using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hearing : Sense
{
    [Header("Sound Detect Distance")]
    [SerializeField]
    float hearingRadius = 10;

    protected override void UpdateSense()
    {
        elapsedTime += Time.deltaTime;

        if (elapsedTime >= detectRate)
        {
            DetectAspect();
            elapsedTime = 0;
        }
    }

    // check range for heard things
    void DetectAspect()
    {
        // get colliders in the hearing range
        Collider[] heardColliders = Physics.OverlapSphere(transform.position, hearingRadius);
       
        GameObject detectedObject;
        Aspect aspect;

        foreach (var heardCollider in heardColliders)
        {
            detectedObject = heardCollider.gameObject;

            // ignore self
            if(detectedObject != transform.gameObject) 
            {
                // ignore same aspects
                if (detectedObject.TryGetComponent<Aspect>(out aspect) && aspect.aspectType != aspectName)
                {
                    // report what was heard
                    Debug.Log($"IS THAT A FILTHY {aspect.aspectType} I HEAR????");
                }
            }
        }
    }
}
