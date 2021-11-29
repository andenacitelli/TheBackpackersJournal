using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : PredatorController
{
    [SerializeField] private float sleepTime = 30.0f;
    [SerializeField] private float eatTime = 5.0f;
    [SerializeField] private float layTime = 10.0f;

  
    protected override void Initialize()
    {
        base.Initialize();
        sounds.Add("digBark", 0); // dig
        sounds.Add("standDouble", 0); // stand
        sounds.Add("attackDouble", 0); // attack
        sounds.Add("standSingle", 1); // stand
        sounds.Add("attackSingle", 1); // attack
        sounds.Add("idleHowl", 3); // howl
        sounds.Add("attackHowl", 3); // howl
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
        AnimalPlaySound("attackHowl");
        yield return new WaitForSeconds(3.0f);

        int randChoice = Random.Range(0, 3);
        switch (randChoice)
        {
            case 0: // bite
                Animations.SetTrigger("bite attack");
                AnimalPlaySound("attackSingle");
                break;
            case 1: // paw
                Animations.SetTrigger("paw attack");
                break;
            case 2: // claws
                Animations.SetTrigger("paws attack");
                AnimalPlaySound("attackDouble");
                break;
        }

        // tell prey it's been hit
        yield return StartCoroutine(AffectPrey());

        // end the timer because no longer hunting
        StopCoroutine(HuntTimer());

        // eat the kill for the set time
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
            case 0: // howl sequence
                yield return StartCoroutine(IdleHowl());
                break;
            case 1: // stand sequence 
                yield return StartCoroutine(IdleCrawl());
                break;
            case 2: // smell sequence
                yield return StartCoroutine(IdleSmell());
                break;
        }
    }

    // crawl actions when idling
    private IEnumerator IdleCrawl()
    {
        Animations.SetTrigger("crawl");

        // wait until the transition to standing finishes
        yield return StartCoroutine(WaitOnAnimationState("Crawling"));
        // wait until all transitions have finished
        yield return new WaitUntil(IsIdling);
    }    
    
    // howl action when idling
    private IEnumerator IdleHowl()
    {
        // trigger transition and play sound
        Animations.SetTrigger("howl");
        AnimalPlaySound("idleHowl");

        // wait until the animation starts and finishes
        yield return StartCoroutine(WaitOnAnimationState("Howling"));
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
                AnimalPlaySound("digBark");
                yield return StartCoroutine(WaitOnAnimationState("Smelling"));
                break;
            case 2: // sit
                // just yield for a frame, it sits after by default anyway
                yield return null;
                break;
        }

        // exit smelling state and move to sitting
        Animations.SetTrigger("sit");
        yield return StartCoroutine(WaitOnAnimationState("Sitting"));
        Animations.SetTrigger("lay");
        yield return StartCoroutine(WaitOnAnimationState("Seat to Lay"));

        // 50% chance to sleep, 50% to just lay down
        if (Random.Range(0.0f, 100.0f) < 50.0f)
        {
            Animations.SetTrigger("lay");
            yield return StartCoroutine(WaitOnAnimationState("Laying"));
            // lay down for the set duration
            yield return new WaitForSeconds(layTime);
            Animations.SetTrigger("stand");
        }
        else
        {
            Animations.SetTrigger("sleep");
            yield return StartCoroutine(WaitOnAnimationState("Sleeping"));
            // sleep for the set duration
            yield return new WaitForSeconds(sleepTime);
            Animations.SetTrigger("wake");
        }

        // potentially play a sound when standing up
        randChoice = Random.Range(0, 3);
        switch (randChoice)
        {
            case 0: // no sound
                break;
            case 1: // dig
                AnimalPlaySound("standSingle");
                break;
            case 2:
                AnimalPlaySound("standDouble");
                break;
        }

        // wait until transitions finish
        yield return new WaitUntil(IsIdling);

    }
}
