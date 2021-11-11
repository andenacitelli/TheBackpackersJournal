using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo fix sounds
public class Bear : PredatorController
{
    [SerializeField] private float sleepTime = 10.0f;
    [SerializeField] private float eatTime = 5.0f;

  
    protected override void Initialize()
    {
        base.Initialize();
        sounds.Add("growl", 0);
        sounds.Add("growl1", 1);
        sounds.Add("snarl", 2);
        sounds.Add("roar", 3);
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
        int randChoice = Random.Range(0, 3);
        switch (randChoice)
        {
            case 0: // bite
                Animations.SetTrigger("bite attack");
                break;
            case 1: // paw
                Animations.SetTrigger("paw attack");
                break;
            case 2: // paws
                Animations.SetTrigger("paws attack");
                break;
        }

        AnimalPlaySound("snarl");

        // tell prey it's been hit
        yield return StartCoroutine(AffectPrey());

        // end the timer because no longer hunting
        StopCoroutine(HuntTimer());

        // eat the kill for the set time
        AnimalPlaySound("growl1");
        yield return StartCoroutine(WaitOnAnimationState("Eat"));

        yield return new WaitForSeconds(eatTime);
    }

    protected override IEnumerator IdleBehavior()
    {
        // start idle animation
        Animations.SetTrigger("idle");
        yield return new WaitUntil(IsIdling);

        int randChoice = Random.Range(0, 3);
        switch (randChoice)
        {
            case 0: // roar sequence
                yield return StartCoroutine(IdleRoar());
                break;
            case 1: // stand sequence 
                yield return StartCoroutine(IdleStand());
                break;
            case 2: // smell sequence
                yield return StartCoroutine(IdleSmell());
                break;
        }
    }

    // stand actions when idling
    private IEnumerator IdleStand()
    {
        Animations.SetTrigger("stand");

        // wait until the transition to standing finishes
        yield return StartCoroutine(WaitOnAnimationState("Standing"));

        // 75% chance to scratch, 25% to roar
        if (Random.Range(0.0f, 100.0f) < 25.0f)
        {
            Animations.SetTrigger("roar");
            AnimalPlaySound("roar");
        }
        else Animations.SetTrigger("scratch");

        // wait until all transitions have finished
        yield return new WaitUntil(IsIdling);
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
    private IEnumerator IdleSmell()
    {
        // trigger transition
        Animations.SetTrigger("smell");

        // wait animation to start
        yield return StartCoroutine(WaitOnAnimationState("Smelling"));

        // select action to take
        int randChoice = Random.Range(0, 3);
        switch (randChoice)
        {
            case 0: // drink
                Animations.SetTrigger("drink");
                yield return StartCoroutine(WaitOnAnimationState("Drinking"));
                Animations.ResetTrigger("drink");
                yield return StartCoroutine(WaitOnAnimationState("Smelling"));
                break;
            case 1: // dig
                Animations.SetTrigger("dig");
                yield return StartCoroutine(WaitOnAnimationState("Digging"));
                AnimalPlaySound("growl");
                yield return StartCoroutine(WaitOnAnimationState("Smelling"));
                break;
            case 2: // sit
                // just yield for a frame, it sits after by default anyway
                yield return null;
                break;
        }

        // exit smelling state and move to sitting
        Animations.SetTrigger("sit");
        yield return StartCoroutine(WaitOnAnimationState("Sitting Look"));

        // 50% chance to sleep, 50% to get back up
        if (Random.Range(0.0f, 100.0f) < 50.0f) Animations.SetTrigger("stand");
        else
        {
            Animations.SetTrigger("sleep");
            yield return StartCoroutine(WaitOnAnimationState("Sleeping"));
            // sleep for the set duration
            yield return new WaitForSeconds(sleepTime);
            Animations.SetTrigger("wake");
        }

        AnimalPlaySound("growl");
        // wait until transitions finish
        yield return new WaitUntil(IsIdling);

    }
}
