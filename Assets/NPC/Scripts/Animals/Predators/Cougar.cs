using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cougar : PredatorController
{
    [SerializeField] private float sleepTime = 30.0f;
    [SerializeField] private float eatTime = 5.0f;
    [SerializeField] private float maxSitTime = 20.0f;
    [SerializeField] private float layTime = 10.0f;

  
    protected override void Initialize()
    {
        base.Initialize();
        sounds.Add("roar", 0);
        sounds.Add("attack roar", 1);

        // 10% chance of it having spots
        Transform spots = transform.GetChild(0).GetChild(1);
        if (Random.Range(0.0f, 10.0f) < 1.0f) spots.gameObject.SetActive(true);
    }

    protected override IEnumerator ActionAtTarget()
    {
        yield return StartCoroutine(base.ActionAtTarget());

        // return to walking animation and restore movement speed
        Animations.CrossFade("Walk", animTransitionTime);
        currentSpeed = WalkSpeed;

        // get a new wander target, and restart hunt detection
        GetNewRoamingDestination();
    }

    protected override IEnumerator AttackBehavior()
    {
        AnimalPlaySound("attack roar");
        yield return new WaitForSeconds(2.5f);

        int randChoice = Random.Range(0, 3);
        switch (randChoice)
        {
            case 0: // bite right
                Animations.SetTrigger("bite attack");
                break;
            case 1: // paw left
                Animations.SetTrigger("paw attack");
                break;
            case 2: // claws
                Animations.SetTrigger("paws attack");
                break;
        }

        // tell prey it's been hit
        yield return StartCoroutine(AffectPrey());

        // end the timer because no longer hunting
        StopCoroutine(HuntTimer());

        // eat the kill for the set time
        yield return StartCoroutine(WaitOnAnimationState("Eating"));

        yield return new WaitForSeconds(eatTime);
    }

    protected override IEnumerator IdleBehavior()
    {
        // start idle animation
        Animations.SetTrigger("idle");
        yield return new WaitUntil(IsIdling);

        int randChoice = Random.Range(0, 2);
        if(randChoice < 1) yield return StartCoroutine(IdleRoar()); // roar sequence
        else yield return StartCoroutine(IdleSit()); // sit sequence
    }

    // roar action when idling
    private IEnumerator IdleRoar()
    {
        // trigger transition and play sound
        Animations.SetTrigger("roar");
        AnimalPlaySound("roar");

        // wait until the animation starts and finishes
        yield return StartCoroutine(WaitOnAnimationState("Roaring"));
        yield return new WaitUntil(IsIdling);
    }    

    // smell actions when idling
    private IEnumerator IdleSit()
    {
        // exit state and move to sitting
        Animations.SetTrigger("sit");
        yield return StartCoroutine(WaitOnAnimationState("Sitting"));

        // decide whether to lay or stand back up
        int randChoice = Random.Range(0, 4);
        if (randChoice > 0)
        { // lay down
            yield return StartCoroutine(IdleLay());
        }
        else
        { // stay seated
            yield return new WaitForSeconds(Random.Range(0.0f, maxSitTime));
            Animations.SetTrigger("stand");
        }

        yield return StartCoroutine(WaitOnAnimationState("Seat to Stand"));
        // potentially roar after standing up
        randChoice = Random.Range(0, 2);
        if (randChoice < 1) 
        { // no sound 
            Animations.SetTrigger("idle");
            yield return new WaitUntil(IsIdling);
        }
        else
        {
            yield return StartCoroutine(IdleRoar());
        }
    }

    private IEnumerator IdleLay()
    {
        Animations.SetTrigger("lay");
        yield return StartCoroutine(WaitOnAnimationState("Seat to Lay"));

        // 50% chance to sleep, 50% to just lay down
        if (Random.Range(0, 2) < 1)
        {
            Animations.SetTrigger("lay");
            yield return StartCoroutine(WaitOnAnimationState("Laying"));
            // lay down for the set duration
            yield return new WaitForSeconds(layTime);
        }
        else
        {
            Animations.SetTrigger("sleep");
            yield return StartCoroutine(WaitOnAnimationState("Sleeping"));
            // sleep for the set duration
            yield return new WaitForSeconds(sleepTime);
        }
        Animations.SetTrigger("stand");
    }
}
