using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyController : AnimalController
{
    [Header("Prey Settings")]
    [SerializeField] protected bool fleeing = false;
    [SerializeField] [Range(0.0f, 60.0f)] protected float threatFleeTimeout = 2.0f;
    [SerializeField] [Range(0.0f, 60.0f)] protected float threatRefreshDelay = 2.0f;
    readonly List<Creature> threats = new List<Creature>();

    Vector3 threatCenter; // center point of detected threats to flee

    protected override void Initialize()
    {
        creatureType = Creature.CreatureTypes.PREY;
        StartCoroutine(LocateThreats());
    }

    // Timebox 3: currently just uses default animal behavior at the target
    protected override IEnumerator ActionAtTarget()
    {

        yield return StartCoroutine(base.ActionAtTarget());
    }

    // Monitor senses for threats
    IEnumerator LocateThreats()
    {
        while (true)
        {
            threats.Clear();
            Creature sensedCreature;
            // check line of sight
            for (int i = 0; i < Senses.SeenCreatures.Count; i++)
            {
                sensedCreature = Senses.SeenCreatures[i];
                if (sensedCreature.creatureType != CreatureType) threats.Add(sensedCreature);
                yield return null;
            }
            // check hearing
            for (int i = 0; i < Senses.HeardCreatures.Count; i++)
            {
                sensedCreature = Senses.HeardCreatures[i];
                if (sensedCreature.creatureType != CreatureType) threats.Add(sensedCreature);
                yield return null;
            }

            // flee from detected threats
            if (threats.Count > 0) yield return RunAway();
            // not constantly updating threat reaction
            yield return new WaitForSeconds(threatRefreshDelay);
        }
    }

    // Run away from the threats
    IEnumerator RunAway()
    {
        fleeing = true;
        StartCoroutine(FleeTimer()); // can only flee for a certain amount of time
        currentSpeed = DashSpeed;
        yield return null;

        while (fleeing)
        {
            GetThreatCenter();
            Vector3 target = threatCenter - transform.position; // get location away from the threat point
            targetDestination = transform.position - target;
            yield return null;
        }
    }

    // Control how long to run from a threat
    IEnumerator FleeTimer()
    {
        yield return new WaitForSeconds(threatFleeTimeout);
        Debug.Log("Fled long enough. Either dying or escaped");
        StopFleeing();
        yield return StartCoroutine(base.ActionAtTarget());
    }

    // stop running from threats
    void StopFleeing()
    {
        fleeing = false;
        currentSpeed = MovementSpeed;
    }
    
    // Calculate the centerpoint of the detected threats by taking the average of their locations
    void GetThreatCenter()
    {
        threatCenter = new Vector3();
        foreach (var threat in threats)
        {
            threatCenter.x += threat.transform.position.x;
            threatCenter.y += threat.transform.position.y;
            threatCenter.z += threat.transform.position.z;
        }
        threatCenter.x = threatCenter.x / threats.Count;
        threatCenter.y = transform.position.y; // potentially needs changed, movement acts weird when the y changes
        threatCenter.z = threatCenter.z / threats.Count;
    }
}
