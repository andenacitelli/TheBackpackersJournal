using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorController : AnimalController
{
    [Header("Predator Settings")]
    [SerializeField] [Range(0.0f, 60.0f)] float huntTimeout = 5.0f;

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
        creatureType = Creature.CreatureTypes.PREDATOR;
        StartCoroutine(LocateHuntingTargets());
    }

    protected override IEnumerator ActionAtTarget()
    {

        if (!IsHunting())
        {// return to roaming
            yield return StartCoroutine(base.ActionAtTarget());
        }
        else
        { // murder prey
            Debug.Log($"Absolutely wrecked the {huntingTarget.creatureType} at {huntingTarget.transform.position}");
            Destroy(huntingTarget.gameObject);
            StopCoroutine(HuntTimer()); // end the timer because no longer hunting
            StopChasing();
            yield return new WaitForSeconds(NewTargetDelay);
        }
    }

    // Monitor vision for things to hunt
    IEnumerator LocateHuntingTargets()
    {
        while (true)
        {
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
                    StartCoroutine(HuntTarget());
                }
            }
        }
    }
    // Follow current position of the target
    IEnumerator HuntTarget()
    {
        StartCoroutine(HuntTimer());
        currentSpeed = DashSpeed;
        while (IsHunting())
        {
            targetDestination = huntingTarget.transform.position;
            yield return null;
        }
    }
    // Control how long to chase a target
    IEnumerator HuntTimer()
    {
        yield return new WaitForSeconds(huntTimeout);
        if (huntingTarget != null && !StillHearsTarget())
        {
            Debug.Log($"Giving up");
            StopChasing();
        }
    }
    // stop following target
    void StopChasing()
    {
        huntingTarget = null;
        currentSpeed = MovementSpeed;
        StopCoroutine(HuntTarget());
    }
}
