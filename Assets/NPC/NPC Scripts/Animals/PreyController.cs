using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreyController : AnimalController
{
    [Header("Prey Settings")]
    
    bool isAttacked = false;
    [SerializeField] [Range(0.0f, 60.0f)] protected float threatFleeTimeout = 2.0f;
    [SerializeField] [Range(0.0f, 60.0f)] protected float threatRefreshDelay = 2.0f;
    readonly List<Creature> threats = new List<Creature>();


    public bool IsAttacked() { return isAttacked; }

    public bool IsFleeing() { return fleeing; }

    protected override void Initialize()
    {
        StartCoroutine(LocateThreats());
    }

    protected override IEnumerator ActionAtTarget()
    {

        yield return StartCoroutine(IdleBehavior());
    }

    protected override IEnumerator IdleBehavior()
    {
        return base.IdleBehavior();
    }

    protected override IEnumerator TriggeredSounds()
    {
        StartCoroutine(PlayDeathSound());
        StartCoroutine(PlayFleeSound());
        yield return null;
    }

    // all prey have a "dead" sound
    protected IEnumerator PlayDeathSound()
    {
        yield return new WaitUntil(IsAttacked);
        AnimalPlaySound("dead");
    }

    protected virtual IEnumerator PlayFleeSound() { yield return null; }

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
            if (threats.Count > 0)
            {
                StopCoroutine(ActionAtTarget());
                yield return StartCoroutine(RunAway());
            }
            // not constantly updating threat reaction
            yield return new WaitForSeconds(threatRefreshDelay);
        }
    }

    // Run away from the threats
    IEnumerator RunAway()
    {
        // start running animation
        Animations.CrossFade("Run", animTransitionTime);
        currentSpeed = RunSpeed;

        fleeing = true;
        StartCoroutine(FleeTimer()); // can only flee for a certain amount of time
        yield return null;

        while (fleeing)
        {
            GetThreatCenter();
            Vector3 target = threatCenter - transform.position; // get location away from the threat point
            TargetDestination = transform.position - target;
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

        // stop running animation
        Animations.CrossFade("Walk", animTransitionTime);
        currentSpeed = WalkSpeed;
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

    // Behavior when the animal gets attacked by a predator
    public IEnumerator Attacked()
    {
        //play death animation
        currentSpeed = 0;
        isAttacked = true;
        Animations.CrossFade("Death", animTransitionTime);
        yield return new WaitForSeconds(Animations.GetCurrentAnimatorStateInfo(0).normalizedTime); // finish death animation
        Destroy(gameObject); // remove from game
    }
}
