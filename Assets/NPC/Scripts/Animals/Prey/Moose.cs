using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo fix sounds
public class Moose : PreyController
{
    [SerializeField] private float sleepTime = 10.0f;
    [SerializeField] private float maxFlexTime = 5.0f;
    [SerializeField] private float maxSitTime = 20.0f;
    [SerializeField] private Transform head;
  
    protected override void Initialize()
    {
        base.Initialize();
        sounds.Add("dead", 0);
        sounds.Add("grunt", 1);
        sounds.Add("longGrunt", 2);

        // select antlers to use
        int antlerIndex = Random.Range(5,11);
        if(antlerIndex < 10)
        {
            Transform antlers = head.GetChild(antlerIndex);
            antlers.gameObject.SetActive(true);
        }
    }

    protected override IEnumerator IdleBehavior()
    {
        // start idle animation
        Animations.SetTrigger("idle");
        yield return new WaitUntil(IsIdling);

        float randChoice = Random.Range(0.0f, 100.0f);
        if (randChoice < 3.0f) yield return StartCoroutine(FlexingMoose());
        else if (randChoice < 70.0f) yield return StartCoroutine(IdleDrink());
        else yield return StartCoroutine(IdleSleep());
    }

    // stand actions when idling
    private IEnumerator FlexingMoose()
    {
        Animations.SetTrigger("flex");

        // wait until the transition to flexing finishes
        yield return StartCoroutine(WaitOnAnimationState("Flexing"));
        AnimalPlaySound("longGrunt");
        yield return new WaitForSeconds(Random.Range(1.5f, maxFlexTime));

        Animations.SetTrigger("scratch");

        // wait until all transitions have finished
        yield return new WaitUntil(IsIdling);

        yield return new WaitForSeconds(NewTargetDelay);

        currentSpeed = WalkSpeed;
        GetNewRoamingDestination();
    }

    protected override IEnumerator PlayFleeSound()
    {
        while (true)
        {
            yield return new WaitUntil(IsFleeing);
            AnimalPlaySound("grunt");
            while (IsFleeing()) yield return null;
        }
    }

    private IEnumerator IdleDrink()
    {
        // trigger transition
        Animations.SetTrigger("drink");
        AnimalPlaySound("grunt");
        yield return new WaitUntil(IsIdling);
    }

    // smell actions when idling
    private IEnumerator IdleSleep()
    {
        Animations.SetTrigger("sit");
        yield return StartCoroutine(WaitOnAnimationState("Sitting"));

        // sit for an amount of time before sleeping
        yield return new WaitForSeconds(Random.Range(1.0f, maxSitTime));
        Animations.SetTrigger("sleep");

        // sleep for an sleepTime seconds before waking
        yield return new WaitForSeconds(sleepTime);
        Animations.SetTrigger("stand");

        // wait until transitions finish
        yield return new WaitUntil(IsIdling);
    }
}
