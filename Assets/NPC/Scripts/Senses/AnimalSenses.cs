using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalSenses : MonoBehaviour
{
    [Header("Sense Sample Frequency")]
    [SerializeField] [Range(0.0f, 1.0f)] float detectRate = 0.2f;

    [Header("Vision")]
    [SerializeField] [Range(0.0f, 360.0f)] float viewingAngle = 50.0f;
    [SerializeField] [Range(0.0f, 100.0f)] float visionRadius = 10.0f;
    [SerializeField] LayerMask creatureMask, obstacleMask;


    [Header("Hearing")]
    [SerializeField] [Range(0.0f, 100.0f)] float hearingRadius = 12.0f;
    readonly float hearingAngle = 360.0f;

    readonly List<Creature> seenCreatures = new List<Creature>();
    readonly List<Creature> heardCreatures = new List<Creature>();

    public float ViewingAngle { get => viewingAngle; }
    public float VisionRadius { get => visionRadius; }
    public float HearingRadius { get => hearingRadius; }
    public float HearingAngle { get => hearingAngle; }
    public float DetectRate { get => detectRate; }
    public List<Creature> SeenCreatures { get => seenCreatures; }
    public List<Creature> HeardCreatures { get => heardCreatures; }

    void Start()
    {
        // activate sense detection
        StartCoroutine(UpdateHearingWithDelay());
        StartCoroutine(UpdateVisionWithDelay());
    }

    private void Update()
    {
        CleanList(seenCreatures);
        CleanList(heardCreatures);
    }

    // Refresh list of seen creatures
    IEnumerator UpdateHearingWithDelay()
    {
        while (true)
        {
            // clear old detections
            heardCreatures.Clear();

            Collider[] heardInRadius = Physics.OverlapSphere(transform.position, hearingRadius, creatureMask);
            Creature creature;

            // add all in hearing range to heard
            foreach (var heard in heardInRadius)
            {
                yield return null;

                if (heard != null && heard.gameObject != transform.gameObject && heard.TryGetComponent<Creature>(out creature))
                    heardCreatures.Add(creature);
            }

            yield return new WaitForSeconds(detectRate);
        }
    }

    // Refresh list of seen creatures
    IEnumerator UpdateVisionWithDelay()
    {
        while (true)
        {
            // clear old detections
            seenCreatures.Clear();

            Collider[] seenInRadius = Physics.OverlapSphere(transform.position, visionRadius, creatureMask);

            // verify creatures in sphere
            foreach (var seen in seenInRadius)
            {
                yield return null;

                // TODO: fix the null reference issue after destroyed
                if (seen != null)
                {
                    Transform seenTransform = seen.transform;
                    Vector3 dirToSeen = (seenTransform.position - transform.position).normalized;
                    Creature seenCreature;

                    // disregard objects outside of viewing angle
                    if (Vector3.Angle(transform.forward, dirToSeen) < viewingAngle / 2)
                    {
                        // distance to detected object
                        float targetDistance = Vector3.Distance(transform.position, seenTransform.position);
                        // check distance to creature and verify unobstructed
                        if (!Physics.Raycast(transform.position, dirToSeen, targetDistance, obstacleMask) && seenTransform.gameObject != transform.gameObject && seenTransform.TryGetComponent<Creature>(out seenCreature))
                        {
                            seenCreatures.Add(seenCreature);
                        }
                    }
                }
            }
            yield return new WaitForSeconds(detectRate);
        }
    }

    // cleans up list by removing null references from objects that got destroyed
    void CleanList(List<Creature> creatures)
    {
        // loop over list backwards searching for and removing null references
        for (var i = creatures.Count - 1; i > -1; i--)
        {
            if (creatures[i] == null)
                creatures.RemoveAt(i);
        }
    }

    // used in AnimalSensesEditor to draw visualisation
    public Vector3 AngleDirection(float angleDegrees, bool isGlobal)
    {
        if (!isGlobal)
        {
            angleDegrees += transform.eulerAngles.y;
        }

        return new Vector3(Mathf.Sin(angleDegrees * Mathf.Deg2Rad), 0.0f, Mathf.Cos(angleDegrees * Mathf.Deg2Rad));
    }

}
