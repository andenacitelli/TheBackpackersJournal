using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo a lot more of the behaviors. this is one of the ones with lots o animations
public class Rabbit : PreyController
{
    [SerializeField] private float eatTime = 5.0f;

    protected override void Initialize()
    {
        base.Initialize();
        sounds.Add("dead", 0);
    }
    protected override IEnumerator IdleBehavior()
    {
        // start idle animation
        Animations.SetTrigger("idle");
        yield return new WaitUntil(IsIdling);

        if (Random.Range(0.0f, 100.0f) < 80.0f)
        {
            Animations.SetTrigger("eat");
            while (!AnimationStateMatchesName("Eating")) yield return null;
            yield return new WaitForSeconds(eatTime);
            Animations.SetTrigger("idle");
            yield return new WaitUntil(IsIdling);
        }
        else yield return new WaitForSeconds(NewTargetDelay);

        currentSpeed = WalkSpeed;
        GetNewRoamingDestination();
    }

}
