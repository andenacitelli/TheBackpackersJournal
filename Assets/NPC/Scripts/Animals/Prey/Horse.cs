using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horse : PreyController
{
    [SerializeField] private float eatTime = 5.0f;

    protected override void Initialize()
    {
        base.Initialize();
        sounds.Add("dead", 0);
        sounds.Add("run", 1);
        sounds.Add("grunt", 2);
        sounds.Add("sound", 3);
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
            AnimalPlaySound(ChangeSound());
            yield return new WaitForSeconds(eatTime);
            Animations.SetTrigger("idle");
            yield return new WaitUntil(IsIdling);
        }
        else yield return new WaitForSeconds(NewTargetDelay);

        currentSpeed = WalkSpeed;
        GetNewRoamingDestination();

    }

    protected override IEnumerator PlayFleeSound()
    {
        while (true)
        {
            yield return new WaitUntil(IsFleeing);
            AnimalPlaySound("run");
            yield return null;
        }
    }

    private string ChangeSound()
    {
        return Random.Range(0.0f, 1.0f)<.5?"sound":"grunt";
    }
}
