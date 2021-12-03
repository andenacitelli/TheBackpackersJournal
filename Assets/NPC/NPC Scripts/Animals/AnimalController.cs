using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WorldGen;

public class AnimalController : MonoBehaviour
{
    //[SerializeField] private bool inEvent = false;
    [Header("Movement Settings")]
    bool isMoving = false;
    private readonly float GRAVITY = -9.8f;
    [SerializeField] [Range(0.0f, 10.0f)] public float movementSpeed; // public for spawner testing, will be private when using actual models
    [SerializeField] [Range(0.0f, 15.0f)] public float dashSpeed; // public for spawner testing, will be private when using actual models
    [SerializeField] [Range(0.0f, 5.0f)] private float turnSpeed = 0.5f; // public for spawner testing, will be private when using actual models
    [SerializeField] [Range(0.0f, 10.0f)] protected float targetTolerance = 3.0f; // public for spawner testing, will be private when using actual prefabs
    [SerializeField] [Range(0.0f, 10.0f)] protected float newTargetDelay = 0.5f;
    [SerializeField] protected float currentSpeed;

    [Header("Roam Restrictions")]
    [SerializeField] [Range(0.0f, 100.0f)] private float newLocationMinDistance = 5.0f;

    // TODO: remove territory. instead have roaming points use chunk area
    [SerializeField] public Bounds territory;


    // Sight and Hearing
    protected Creature.CreatureTypes creatureType;
    protected Vector3 targetDestination;
    private AnimalSenses senses;
    protected bool fleeing = false;
    protected Vector3 threatCenter; // center point of detected threats to flee

    protected Transform playerTransform = null;

    private CharacterController controller;
    
    protected Animator anim;
    [SerializeField] [Range(0.0f, 1.0f)] protected float animTransitionTime = 0.5f;

    protected AudioManager audioManager;
    protected AudioSource audioSource;
    [SerializeField] protected string[] audioManagerNames;

    // key is animal specific shorthand for the sound, value is the index of the 
    protected readonly Dictionary<string, int> sounds = new Dictionary<string, int>();


    protected float WalkSpeed { get => movementSpeed; }
    protected float RunSpeed { get => dashSpeed; }
    protected float TurnSpeed { get => turnSpeed; }
    protected float TargetTolerance { get => targetTolerance; }
    protected float NewTargetDelay { get => newTargetDelay; }
    protected AnimalSenses Senses { get => senses; }
    public bool PlayerInRange { get => playerTransform != null; }
    protected CharacterController Controller { get => controller; }
    public Creature.CreatureTypes CreatureType { get => creatureType; }
    public Animator Animations { get => anim;  }

    // to make it work with all the animals the triggers are: startRun, stopRun, startAttack, startWalk, returnIdle



    protected virtual void Initialize() { }

    protected virtual IEnumerator ActionAtTarget()
    {
        yield return StartCoroutine(IdleBehavior());
    }

    protected virtual IEnumerator IdleBehavior()
    {
        // start idle animation
        Animations.CrossFade("Idle", animTransitionTime);

        // wait at target and get new destination
        yield return new WaitForSeconds(newTargetDelay);
        currentSpeed = WalkSpeed;
        GetNewRoamingDestination();
    }

    protected virtual IEnumerator AttackBehavior() { yield return null; }

    protected IEnumerator WaitOnAnimationState(string stateName)
    {
        while (!AnimationStateMatchesName(stateName)) yield return null;
    }

    // Animal behavior
    void Awake()
    {
        currentSpeed = WalkSpeed;

        creatureType = GetComponent<Creature>().creatureType;
        controller = GetComponent<CharacterController>();
        senses = GetComponent<AnimalSenses>();
        anim = GetComponent<Animator>();

        audioManager = FindObjectOfType<AudioManager>();
        audioSource = GetComponent<AudioSource>();
    }

    // call to start the animal doing its thing after placing in world
    public void LetsGetGoing()
    {
        Initialize();
        StartCoroutine(AnimalBehavior());
    }
    // returns true when within tolerance distance of destination
    bool AtTarget()
    {
        return Vector3.Distance(targetDestination, transform.position) <= targetTolerance;
    }

    protected bool IsIdling()
    {
        return AnimationStateMatchesName("Idling");
    }

    // returns true if current animation state matches the passed name
    protected bool AnimationStateMatchesName(string name)
    {
        return Animations.GetCurrentAnimatorStateInfo(0).IsName(name);
    }

    // uses the shorthand to get the actual sound name and send that to audiomanager
    private string TrueSoundName(string shorthand)
    {
        return audioManagerNames[sounds[shorthand]];
    }

    // play given sound from animal
    protected void AnimalPlaySound(string shorthand)
    {
        if(audioManager != null)
        {
            string soundName = TrueSoundName(shorthand);
            audioManager.Assign3DSource(audioSource, soundName);
            audioManager.Play(soundName);
        }

    }

    protected virtual IEnumerator TriggeredSounds() { yield return null; }

    // Carry out all of the animal's functions
    IEnumerator AnimalBehavior()
    {
        StartCoroutine(TriggeredSounds());
        GetNewRoamingDestination();
        while (true)
        {
            yield return StartCoroutine(MoveToTarget());
            yield return StartCoroutine(ActionAtTarget());
        }
    }

    // Move toward the current target
    protected virtual IEnumerator MoveToTarget()
    {
        // start moving animations
        Animations.CrossFade("Walk", animTransitionTime);
        isMoving = true;
        while (!AtTarget())
        {
            // adjust target to be in territory
            StayInYaLane();

            // turn toward target
            Quaternion targetRotation = Quaternion.LookRotation(targetDestination - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);

            // move toward target
            Vector3 moveDirection = transform.TransformDirection(Vector3.forward) * currentSpeed;
            moveDirection.y = GRAVITY;
            controller.Move(moveDirection * Time.deltaTime);
            yield return null;
        }

        isMoving = false;
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
        targetDestination.y = transform.position.y; // adjust height if not flying, probably fucks behavior on nonflat surfaces

    }

}
