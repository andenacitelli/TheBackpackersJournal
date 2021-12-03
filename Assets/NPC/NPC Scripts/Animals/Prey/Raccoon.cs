using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo fix sounds
public class Raccoon : PreyController
{
    [SerializeField] private float sleepTime = 10.0f;
    [SerializeField] private float maxSitTime = 20.0f;
  
    protected override void Initialize()
    {
        base.Initialize();
        sounds.Add("dead", 0);
        sounds.Add("angry", 1);
        sounds.Add("chitter", 2);
        sounds.Add("eating", 3);
        sounds.Add("tired", 4);
        sounds.Add("small", 5);
    }

    protected override IEnumerator IdleBehavior()
    {
        // start idle animation
        Animations.SetTrigger("idle");
        yield return new WaitUntil(IsIdling);

        float randChoice = Random.Range(0.0f, 100.0f);
        if (randChoice < 25.0f) yield return StartCoroutine(IdleSit());
        else if (randChoice < 50.0f) yield return StartCoroutine(IdleSneak());
        else if (randChoice < 75.0f) yield return StartCoroutine(IdleStand());
        else yield return StartCoroutine(IdleActions());
    }

    protected override IEnumerator PlayFleeSound()
    {
        while (true)
        {
            yield return new WaitUntil(IsFleeing);
            AnimalPlaySound("chitter");
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
            if (randChoice < 7.5f) Animations.SetTrigger("eat");
            else Animations.SetTrigger("dig");

            yield return StartCoroutine(WaitOnAnimationState("Eating"));
            AnimalPlaySound("eating");
        }

        yield return new WaitUntil(IsIdling);
    }

    // sneak actions when idling
    private IEnumerator IdleSneak()
    {
        Animations.SetTrigger("sneak");
        AnimalPlaySound("angry");

        yield return new WaitUntil(IsIdling);
    }

    // standing actions when idling
    private IEnumerator IdleStand()
    {
        Animations.SetTrigger("stand");
        yield return StartCoroutine(WaitOnAnimationState("Standing"));
        Animations.SetTrigger("look");
        AnimalPlaySound("small");
        yield return StartCoroutine(WaitOnAnimationState("Standing"));
        Animations.SetTrigger("sit");

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
            
            Animations.SetTrigger("sleep");
            yield return StartCoroutine(WaitOnAnimationState("Sleeping"));
            yield return new WaitForSeconds(sleepTime);

            Animations.SetTrigger("lay");
            yield return StartCoroutine(WaitOnAnimationState("Sleep Lay"));

            Animations.SetTrigger("sit");
        }

        // wait for laying animations to start sitting up
        yield return StartCoroutine(WaitOnAnimationState("Lie to Seat"));
        AnimalPlaySound("tired");
    }
}
