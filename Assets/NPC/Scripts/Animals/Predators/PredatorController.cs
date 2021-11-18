using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorController : AnimalController
{
    [Header("Predator Settings")]
    [SerializeField] [Range(0.0f, 60.0f)] float huntTimeout = 5.0f;


    protected Creature huntingTarget;

    protected bool IsHunting()
    {
        return huntingTarget != null;
    }
    protected bool CanHuntNewTarget()
    {
        // if there is still a target then no need to retarget
        return huntingTarget == null && Senses.SeenCreatures.Count > 0;
    }
    protected bool StillHearsTarget()
    {
        bool canDetect = IsHunting() && Senses.HeardCreatures.Count > 0;
        canDetect = canDetect && Senses.HeardCreatures.Contains(huntingTarget);
        return canDetect;
    }

    protected override void Initialize()
    {
        StartCoroutine(LocateHuntingTargets());
    }

    protected override IEnumerator ActionAtTarget()
    {
        // no need to keep looking for targets while these things are happening
        StopCoroutine(LocateHuntingTargets());

        if (!IsHunting())
        { 
            yield return StartCoroutine(IdleBehavior());
        }
        else
        { 
            StopCoroutine(HuntTarget());
            yield return StartCoroutine(AttackBehavior());
        }
        // start detecting targets again
        StartCoroutine(LocateHuntingTargets());

    }

    // Behavior when idling
    protected override IEnumerator IdleBehavior()
    {
        // return to roaming
        yield return StartCoroutine(base.IdleBehavior());
    }

    // Behavior when attacking
    protected override IEnumerator AttackBehavior()
    {
        // murder prey

        // start attack animation
        Animations.Play("Attack");
        AnimalPlaySound(audioManagerNames[0]);
        yield return new WaitForSeconds(Animations.GetCurrentAnimatorStateInfo(0).normalizedTime / 2); // finish attack animation

        yield return StartCoroutine(AffectPrey());

        // end the timer because no longer hunting
        StopCoroutine(HuntTimer());

        Animations.CrossFade("Walk", animTransitionTime);
        currentSpeed = WalkSpeed;

        yield return new WaitForSeconds(NewTargetDelay);
    }

    // Start target's death sequence
    protected IEnumerator AffectPrey()
    {
        // start target's death sequence
        PreyController prey = huntingTarget.gameObject.GetComponent<PreyController>();
        if (prey != null && !prey.IsAttacked())
        {
            prey.StopAllCoroutines();
            // wait until the target has finished its death animation
            yield return StartCoroutine(prey.Attacked());
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
    protected virtual IEnumerator HuntTarget()
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
