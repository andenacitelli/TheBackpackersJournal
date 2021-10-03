using System.Collections;
using UnityEngine;

public class AnimalController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private bool canFly = false;
    [SerializeField] [Range(0.0f, 100.0f)] public float movementSpeed; // public for spawner testing, will be private when using actual models
    [SerializeField] [Range(0.0f, 100.0f)] public float dashSpeed; // public for spawner testing, will be private when using actual models
    [SerializeField] [Range(0.0f, 10.0f)] public float turnSpeed; // public for spawner testing, will be private when using actual models
    [SerializeField] [Range(0.0f, 10.0f)] public float targetTolerance; // public for spawner testing, will be private when using actual models
    [SerializeField] [Range(0.0f, 10.0f)] private float newTargetDelay = 0.5f;
    protected float currentSpeed;

    [Header("Roam Restrictions")]
    [SerializeField] [Range(0.0f, 100.0f)] private float newLocationMinDistance = 5.0f;
    [SerializeField] public Bounds territory;


    // Sight and Hearing
    protected Creature.CreatureTypes creatureType;
    protected Vector3 targetDestination;
    private AnimalSenses senses;
    private CharacterController controller;

    public bool CanFly { get => canFly; }
    protected float MovementSpeed { get => movementSpeed; }
    protected float DashSpeed { get => dashSpeed; }
    protected float TurnSpeed { get => turnSpeed; }
    protected float TargetTolerance { get => targetTolerance; }
    protected float NewTargetDelay { get => newTargetDelay; }
    protected AnimalSenses Senses { get => senses; }
    protected CharacterController Controller { get => controller; }
    public Creature.CreatureTypes CreatureType { get => creatureType; }

    protected virtual void Initialize() { }
    protected virtual IEnumerator ActionAtTarget()
    {
        // wait at target and get new destination
        yield return new WaitForSeconds(newTargetDelay);
        GetNewRoamingDestination();
    }

    // Animal behavior
    void Start()
    {
        currentSpeed = MovementSpeed;
        creatureType = GetComponent<Creature>().creatureType;
        controller = GetComponent<CharacterController>();
        senses = transform.GetComponent<AnimalSenses>();

        Initialize();
        StartCoroutine(AnimalBehavior());
    }

    // returns true when within tolerance distance of destination
    bool AtTarget()
    {
        return Vector3.Distance(targetDestination, transform.position) <= targetTolerance;
    }

    // Carry out all of the animal's functions
    IEnumerator AnimalBehavior()
    {
        GetNewRoamingDestination();
        while (true)
        {
            yield return StartCoroutine(MoveToTarget());
            yield return StartCoroutine(ActionAtTarget());
        }
    }

    // Move toward the current target
    IEnumerator MoveToTarget()
    {
        while (!AtTarget())
        {
            // adjust target to be in territory
            StayInYaLane();
            // move toward target
            //proper move
            controller.Move(((targetDestination - transform.position).normalized) * currentSpeed * Time.deltaTime);
            // move ignoring walls
            //transform.position = Vector3.MoveTowards(transform.position, targetDestination,  Time.deltaTime * currentSpeed);

            // turn toward target
            Quaternion targetRotation = Quaternion.LookRotation(targetDestination - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

            yield return null;
        }
    }

    // very possibly going to get rid of this, makes it so that any animal can't leave the bounds given by maxX, maxY, maxZ
    // to fix fleeing/hunting outside of their territory
    void StayInYaLane()
    {
        if (!territory.Contains(targetDestination))
        {
            targetDestination = territory.ClosestPoint(targetDestination);
            targetDestination.y = transform.position.y;
        }
    }

    // Start roaming toward a new random location
    protected void GetNewRoamingDestination()
    {
        float minX = territory.min.x, minY = territory.min.y, minZ = territory.min.z;
        float maxX = territory.max.x, maxY = territory.max.y, maxZ = territory.max.z;

        // generate a point within the bounds/territory
        targetDestination = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
        while(Vector3.Distance(targetDestination, transform.position) < newLocationMinDistance) targetDestination = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
        if (!canFly) targetDestination.y = transform.position.y; // adjust height if not flying, probably fucks behavior on nonflat surfaces

        // return to default moving speed
        currentSpeed = MovementSpeed;

        Debug.Log($"{CreatureType} now roaming toward {targetDestination}");
    }
}
