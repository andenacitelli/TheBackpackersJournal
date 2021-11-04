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
        //audioManager.Assign3DSource(audioSource, soundNames[0]);
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

        PlaySound(soundNames[0]);

        // start target's death sequence
        PreyController prey = huntingTarget.gameObject.GetComponent<PreyController>();
        if (prey != null)
        {
            prey.StopAllCoroutines();
            // wait until the target has finished its death animation
            yield return StartCoroutine(prey.Attacked());
        }

        // end the timer because no longer hunting
        StopCoroutine(HuntTimer());
        // eat the kill for the set time
        while (!AnimationStateMatchesName("Eat")) yield return null;
        yield return new WaitForSeconds(eatTime);
    }

    // stand actions when idling
    private IEnumerator IdleStand()
    {
        Animations.SetTrigger("stand");

        // wait until the transition to standing finishes
        while (!AnimationStateMatchesName("Standing")) yield return null;

        // 75% chance to scratch, 25% to roar
        if (Random.Range(0.0f, 100.0f) < 25.0f)
        {
            Animations.SetTrigger("roar");
            PlaySound(soundNames[0]);
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
        PlaySound(soundNames[0]);

        // wait until the animation starts and finishes
        while (!AnimationStateMatchesName("Roaring")) yield return null;
        yield return new WaitUntil(IsIdling);
    }    

    // smell actions when idling
    private IEnumerator IdleSmell()
    {
        // trigger transition
        Animations.SetTrigger("smell");
        
        // wait animation to start
        while (!AnimationStateMatchesName("Smelling")) yield return null;

        // select action to take
        int randChoice = Random.Range(0, 3);
        switch (randChoice)
        {
            case 0: // drink
                Animations.SetTrigger("drink");
                while (!AnimationStateMatchesName("Drinking")) yield return null;
                Animations.ResetTrigger("drink");
                while (!AnimationStateMatchesName("Smelling")) yield return null;
                break;
            case 1: // dig
                Animations.SetTrigger("dig");
                while (!AnimationStateMatchesName("Digging")) yield return null;
                while (!AnimationStateMatchesName("Smelling")) yield return null;
                break;
            case 2: // sit
                // just yield for a frame, it sits after by default anyway
                yield return null;
                break;
        }

        // exit smelling state and move to sitting
        Animations.SetTrigger("sit");
        while (!AnimationStateMatchesName("Sitting Look")) yield return null;

        // 50% chance to sleep, 50% to get back up
        if (Random.Range(0.0f, 100.0f) < 50.0f) Animations.SetTrigger("stand");
        else
        {
            Animations.SetTrigger("sleep");
            while (!AnimationStateMatchesName("Sleeping")) yield return null;
            // sleep for the set duration
            yield return new WaitForSeconds(sleepTime);
            Animations.SetTrigger("wake");
        }

        // wait until transitions finish
        yield return new WaitUntil(IsIdling);

    }
}
