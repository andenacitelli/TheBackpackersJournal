using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo fix sounds
public class Fox : PreyController
{
    [SerializeField] private float sleepTime = 10.0f;
    [SerializeField] private float maxSitTime = 20.0f;
  
    protected override void Initialize()
    {
        base.Initialize();
        sounds.Add("dead", 0);
        sounds.Add("noise", 1);
        sounds.Add("eating", 2);
    }

    protected override IEnumerator IdleBehavior()
    {
        // start idle animation
        Animations.SetTrigger("idle");
        yield return new WaitUntil(IsIdling);

        float randChoice = Random.Range(0.0f, 100.0f);
        if (randChoice < 50.0f) yield return StartCoroutine(IdleSit());
        else yield return StartCoroutine(IdleActions());
    }

    protected override IEnumerator PlayFleeSound()
    {
        while (true)
        {
            yield return new WaitUntil(IsFleeing);
            AnimalPlaySound("noise");
            while (IsFleeing()) yield return null;
        }
    }

    // general actions when idling
    private IEnumerator IdleActions()
    {
        Animations.SetTrigger("look");
        yield return new WaitUntil(IsIdling);

        float randChoice = Random.Range(0.0f, 10.0f);

        if (randChoice < 5.0) Animations.SetTrigger("scratch"); // scratch anim sequence
        else
        { // eat sequence
            Animations.SetTrigger("smell");
            yield return StartCoroutine(WaitOnAnimationState("Smelling"));

            // dig first or just eat
            if (randChoice < 7.5f)
            { // eat
                Animations.SetTrigger("eat");
                yield return StartCoroutine(WaitOnAnimationState("Eating"));
                AnimalPlaySound("eating");
            }
            else
            { // dig
                Animations.SetTrigger("dig");
                yield return StartCoroutine(WaitOnAnimationState("Digging"));

                randChoice = Random.Range(0, 3);
                if (randChoice < 2) Animations.SetTrigger("pick"); // pick up
                else
                { // eat
                    Animations.SetTrigger("eat");
                    yield return StartCoroutine(WaitOnAnimationState("Eating"));
                    AnimalPlaySound("eating");
                }
            }
        }

        yield return new WaitUntil(IsIdling);
    }

    // smell actions when idling
    private IEnumerator IdleSit()
    {
        Animations.SetTrigger("sit");
        yield return StartCoroutine(WaitOnAnimationState("Sitting"));


        // sit for an amount of time before sleeping
        yield return new WaitForSeconds(Random.Range(1.0f, maxSitTime));
        yield return StartCoroutine(IdleLay());

        // stand up again
        yield return StartCoroutine(WaitOnAnimationState("Sitting"));
        Animations.SetTrigger("stand");

        // wait until transitions finish
        yield return new WaitUntil(IsIdling);
    }

    // lay down actions
    private IEnumerator IdleLay()
    {
        Animations.SetTrigger("lay");
        yield return StartCoroutine(WaitOnAnimationState("Seat to Lie"));

        float randChoice = Random.Range(0.0f, 10.0f);
        if(randChoice < 7.0f)
        { // lay down without sleeping
            Animations.SetTrigger("lay");
            yield return StartCoroutine(WaitOnAnimationState("Laying"));
            yield return new WaitForSeconds(Random.Range(maxSitTime/2, maxSitTime));
            Animations.SetTrigger("sit");
        }
        else
        { // lay down and sleep
            Animations.SetTrigger("sleep");

            yield return StartCoroutine(WaitOnAnimationState("Sleep Lay"));
            yield return new WaitForSeconds(Random.Range(maxSitTime / 2, maxSitTime));

            Animations.SetTrigger("sleep");
            yield return StartCoroutine(WaitOnAnimationState("Sleeping"));
            
            yield return new WaitForSeconds(sleepTime);
            Animations.SetTrigger("sit");
        }

        // wait for laying animations to start sitting up
        yield return StartCoroutine(WaitOnAnimationState("Lie to Seat"));
        AnimalPlaySound("noise");
    }
}
