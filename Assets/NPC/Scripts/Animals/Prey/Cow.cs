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
        //audioManager.Assign3DSource(audioSource, soundNames[0]);
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
            PlaySound(ChangeMoo());
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
        string newMoo = soundNames[Random.Range(0, soundNames.Length)];
        //audioManager.Assign3DSource(audioSource, newMoo);
        return newMoo;
    }
}
