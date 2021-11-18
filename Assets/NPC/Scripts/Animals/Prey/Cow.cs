using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// todo fix sounds and add random ones that play during non eating moments
public class Cow : PreyController
{
    [SerializeField] private float eatTime = 5.0f;

    protected override void Initialize()
    {
        base.Initialize();
        sounds.Add("dead", 0);
        sounds.Add("run", 1);
        sounds.Add("moo", 2);
        sounds.Add("moopocalypse", 3);
        sounds.Add("mootastrophe", 4);
        sounds.Add("moofoundland", 5);
        sounds.Add("moose", 6);
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
            AnimalPlaySound(ChangeMoo());
            yield return new WaitForSeconds(eatTime);
            Animations.SetTrigger("idle");
            yield return new WaitUntil(IsIdling);
        }
        else yield return new WaitForSeconds(NewTargetDelay);

        currentSpeed = WalkSpeed;
        GetNewRoamingDestination();

    }
    // changes the sound that the cow makes to a random
    private string ChangeMoo()
    {
        switch (Random.Range(2, 7))
        {
            case 2:
                return "moo";
            case 3:
                return "moopocalypse";
            case 4:
                return "mootastrophe";
            case 5:
                return "moofoundland";
            case 6:
                return "moose";
            default:
                return "moo";
        }
    }
}
