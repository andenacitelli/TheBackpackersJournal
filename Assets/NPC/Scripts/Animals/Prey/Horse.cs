using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horse : PreyController
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
            AnimalPlaySound(ChangeSound());
            yield return new WaitForSeconds(eatTime);
            Animations.SetTrigger("idle");
            yield return new WaitUntil(IsIdling);
        }
        else yield return new WaitForSeconds(NewTargetDelay);

        currentSpeed = WalkSpeed;
        GetNewRoamingDestination();

    }
    // changes the sound that the cow makes to a random
    private string ChangeSound()
    {
        string newSound = audioManagerNames[Random.Range(0, audioManagerNames.Length)];
        //audioManager.Assign3DSource(audioSource, newMoo);
        return newSound;
    }
}
