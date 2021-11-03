using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorController : AnimalController
{
    [Header("Predator Settings")]
    [SerializeField] [Range(0.0f, 60.0f)] float huntTimeout = 5.0f;

    [Header("Sound Names")]
    [SerializeField] protected string attackSoundName;

    Creature huntingTarget;

    bool IsHunting()
    {
        return huntingTarget != null;
    }
    bool CanHuntNewTarget()
    {
        // if there is still a target then no need to retarget
        return huntingTarget == null && Senses.SeenCreatures.Count > 0;
    }
    bool StillHearsTarget()
    {
        bool canDetect = IsHunting() && Senses.HeardCreatures.Count > 0;
        canDetect = canDetect && Senses.HeardCreatures.Contains(huntingTarget);
        return canDetect;
    }

    protected override void Initialize()
    {
        StartCoroutine(LocateHuntingTargets());
    }

    protected virtual void PlayAttackSound() { }

    protected override IEnumerator ActionAtTarget()
    {

        if (!IsHunting())
        { // return to roaming
            yield return StartCoroutine(base.ActionAtTarget());
        }
        else
        { // murder prey
            // start attack animation
            Animations.Play("Attack");
            PlayAttackSound();
            yield return new WaitForSeconds(Animations.GetCurrentAnimatorStateInfo(0).normalizedTime/2); // finish attack animation

            // start target's death sequence
            PreyController prey = huntingTarget.gameObject.GetComponent<PreyController>();
            if (prey != null)
            {
                prey.StopAllCoroutines();
                yield return StartCoroutine(prey.Attacked());
            }

            // end the timer because no longer hunting
            StopCoroutine(HuntTimer());

            Animations.CrossFade("Walk", animTransitionTime);
            currentSpeed = movementSpeed;

            yield return new WaitForSeconds(NewTargetDelay);
        }
    }

    // Monitor vision for things to hunt
    protected IEnumerator LocateHuntingTargets()
    {
        while (true)
        {
            // do not look for a new target until current is either caught or escaped
            yield return new WaitUntil(CanHuntNewTarget);

            Creature foundTarget = null;

            // target a player or prey in sight
            for (int i = 0; i < Senses.SeenCreatures.Count; i++)
            {
                foundTarget = Senses.SeenCreatures[i];
                if (foundTarget.creatureType != CreatureType) break;
                yield return null;
            }

            // Set hunting target and begin to chase
            if (foundTarget != null && foundTarget.creatureType != CreatureType)
            {
                yield return null;
                huntingTarget = foundTarget;
                if (huntingTarget != null)
                {
                    Debug.Log($"NEW HUNTING TARGET {huntingTarget.name}");
                    yield return StartCoroutine(HuntTarget());
                }
            }
        }
    }
    
    // Follow current position of the target
    protected IEnumerator HuntTarget()
    {
        // Start running animation sequence
        Animations.Play("Run");
        currentSpeed = RunSpeed;

        StartCoroutine(HuntTimer());

        // update destination to target's current position
        while (IsHunting())
        {
            targetDestination = huntingTarget.transform.position;
            yield return new WaitForSeconds(Senses.DetectRate * 2);
        }
    }
    
    // Control how long to chase a target
    protected IEnumerator HuntTimer()
    {
        yield return new WaitForSeconds(huntTimeout);
        if (huntingTarget != null && !StillHearsTarget())
        { // give up if target is not close enough to hear
            huntingTarget = null;

            Animations.CrossFade("Walk", animTransitionTime);
            currentSpeed = WalkSpeed;

            StopCoroutine(HuntTarget());
        }else yield return StartCoroutine(HuntTimer());
    }

}
