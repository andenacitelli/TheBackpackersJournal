using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo a lot more of the behaviors. this is one of the ones with lots o animations
public class Deer : PreyController
{
    [SerializeField] private float eatDuration = 5.0f;
    [SerializeField] private float minSleepDelay = 2.0f;
    [SerializeField] private float maxSleepDelay = 30.0f;
    [SerializeField] private float minSleepDuration = 10.0f;
    [SerializeField] private float maxSleepDuration = 90.0f;

    protected override void Initialize()
    {
        base.Initialize();
        if (maxSleepDuration <= minSleepDuration)
        {
            maxSleepDuration = minSleepDuration * 2;
        }

        sounds.Add("dead", 0);
        sounds.Add("bleat", 1);
        sounds.Add("grunt", 2);
    }

    protected override IEnumerator IdleBehavior()
    {
        // start idle animation
        Animations.SetTrigger("idle");
        yield return new WaitUntil(IsIdling);

        int randChoice = Random.Range(0, 3);
        switch (randChoice)
        {
            case 0: // eat sequence
                yield return StartCoroutine(IdleEat());
                break;
            case 1: // sit sequence 
                yield return StartCoroutine(IdleSleep());
                break;
            case 2: // look sequence
                yield return StartCoroutine(IdleLook());
                break;
        }

        randChoice = Random.Range(0, 5);
        switch (randChoice)
        {
            case 0: // eat sequence
                yield return StartCoroutine(IdleEat());
                break;
            case 1: // look sequence
                yield return StartCoroutine(IdleLook());
                break;
        }

        yield return new WaitForSeconds(NewTargetDelay);

        currentSpeed = WalkSpeed;
        GetNewRoamingDestination();

    }

    protected override IEnumerator PlayFleeSound()
    {
        while (true)
        {
            yield return new WaitUntil(IsFleeing);
            AnimalPlaySound("bleat");
            yield return null;
        }
    }

    IEnumerator IdleEat()
    {
        // start eating and make a sound
        Animations.SetTrigger("eat");
        yield return StartCoroutine(WaitOnAnimationState("Eating"));
        AnimalPlaySound("bleat");

        // eat for eating duration
        yield return new WaitForSeconds(eatDuration);

        // return to idle animation
        Animations.SetTrigger("idle");
        yield return new WaitUntil(IsIdling);
    }

    IEnumerator IdleSleep()
    {
        Animations.SetTrigger("sit");
        yield return StartCoroutine(WaitOnAnimationState("Sitting"));
        // sit for a time before falling asleep
        yield return new WaitForSeconds(Random.Range(minSleepDelay, maxSleepDelay));

        // sleep
        Animations.SetTrigger("sleep");
        yield return StartCoroutine(WaitOnAnimationState("Sleeping"));
        // sleep for an amount of time
        yield return new WaitForSeconds(Random.Range(minSleepDuration, maxSleepDuration));

        // wake back up and stand
        AnimalPlaySound("grunt");
        Animations.SetTrigger("wake");
        yield return new WaitUntil(IsIdling);
    }

    IEnumerator IdleLook()
    {
        Animations.SetTrigger("look left");
        yield return StartCoroutine(WaitOnAnimationState("Looking Left"));
        yield return new WaitUntil(IsIdling);
    }
}
