using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WorldGen;

public class SpawnManager : MonoBehaviour
{
    [Header("Animals to Spawn")]
    [SerializeField] GameObject[] commonPrey;
    [SerializeField] GameObject[] commonPredators;

    [Header("Animal Spawning")]
    readonly List<GameObject> worldAnimals = new List<GameObject>();

    [SerializeField] int maxSpawns = 10;
    [SerializeField] [Range(0.0f, 300.0f)] float spawnDelaySeconds = 30.0f;
    [SerializeField] float minPlayerDistance = 10.0f;
    [SerializeField] float maxPlayerDistance = 40.0f;
    [SerializeField] float minAnimalsDistance = 5.0f;
    [SerializeField] float aboveGroundOffset = 1.0f; // to ensure that they spawn above ground level. they'll drop to touch ground with gravity
    // The chance that an animal spawned will be predator or prey
    // predator chance will be equal to 100 - prey chance.
    [SerializeField] [Range(0.0f, 100.0f)] float preySpawnChance = 65.0f;
    float predatorSpawnChance; // this hasn't actually been used yet because it can be applied without storing a separate number

    [Header("Animal Despawning")]
    [SerializeField] float despawnRange = 20.0f;
    [SerializeField] [Range(0.0f, 300.0f)] float despawnDelaySeconds = 120.0f; // time ater player leaves range to despawn animals

    [Header("Wildlife Events to Spawn")]
    [SerializeField] private GameObject[] commonEvents;
    [SerializeField] private GameObject[] uncommonEvents;
    [SerializeField] private GameObject[] rareEvents;

    [Header("Wildlife Event Spawning")]
    readonly List<GameObject> worldEvents = new List<GameObject>();

    [SerializeField] int maxEvents = 2;
    [SerializeField] float multiEventChance = 25.0f; // chance that an additional wildlife event will spawn when one is spawning
    [SerializeField] [Range(0.0f, 600.0f)] float eventSpawnDelaySeconds = 300.0f;
    [SerializeField] float eventDistanceFromPlayer = 50.0f;
    [SerializeField] float despawnEventAfterDistance = 200.0f;
    [Header("Should add up to less than 100")]
    // the common rate will be 100 - (uncommonEventRate + rareEventRate)
    // ranges for uncommon and rare are just what made sense to me  
    [SerializeField] [Range(20.0f, 40.0f)] private float uncommonEventRate = 38.25f;
    [SerializeField] [Range(0.0f, 10.0f)]  private float rareEventRate = 1.5f;

    TerrainFunctions.TerrainPointData heightData;
    Transform playerTransform;

    // containers to keep animals and events organized from everything else
    GameObject animalsHolder;
    GameObject eventsHolder;
    
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        predatorSpawnChance = 100.0f - preySpawnChance;

        // create animal container in world
        animalsHolder = new GameObject("Animals");
        eventsHolder = new GameObject("Events");

        // begin spawning animals and wildlife events into the world
        StartCoroutine(AnimalSpawner());
        StartCoroutine(WildlifeEventSpawner());
    }
    
    // Return whether the chunks have been placed in world yet
    bool ChunksRenderedAtPlayer()
    {
        return TerrainFunctions.GetTerrainPointData(new Vector2(playerTransform.position.x, playerTransform.position.z)).isHit;
    }

    // Return if the object is still within distance of player as set by despawnRange
    bool AnimalInRange(GameObject animal)
    {
        return Vector3.Distance(playerTransform.position, animal.transform.position) < despawnRange;
    }

    // Return if the WildlifeEvent is still within distance of player as set by despawnRange
    bool WildlifeEventInRange(GameObject wildlifeEvent)
    {
        return Vector3.Distance(playerTransform.position, wildlifeEvent.transform.position) < despawnEventAfterDistance;
    }


    // Return whether less than the maximum number of animals are spawned
    bool CanAddAnimal()
    {
        return worldAnimals.Count < maxSpawns;
    }

    // Return whether less than the maximum number of events are spawned
    bool CanAddWildlifeEvent()
    {
        return worldEvents.Count < maxEvents;
    }

    // returns the array of animals to spawn from
    // uses preySpawnChance and a random number generator to determine which array to return
    GameObject[] GetSpawnSource()
    {
        return Random.Range(0.0f, 100.0f) <= preySpawnChance ? commonPrey : commonPredators;
    }

    // return the array of events to spawn
    // uses event rarities to determine which array to return
    GameObject[] GetWildlifeEventSource()
    {
        WildlifeEvent.EventRarity rarity = GenerateWildlifeEventRarity();

        switch (rarity)
        {
            case WildlifeEvent.EventRarity.RARE:
                return rareEvents;
            case WildlifeEvent.EventRarity.UNCOMMON:
                return uncommonEvents;
            case WildlifeEvent.EventRarity.COMMON:
                return commonEvents;
            default:
                Debug.Log("Something didn't work with event rarity. Defaulting to common.");
                return commonEvents;
        }
    }
    
    // Clean up clone object's name by removing the '(Clone)' to help with photo ID
    void CleanName(GameObject cloneObject)
    {
        string objectName = cloneObject.name;
        if (objectName.Contains("(Clone)")) cloneObject.name = objectName.Substring(0, objectName.LastIndexOf("(Clone)"));
    }

    // Spawn a random animal gameobject into the game world, returns the object spawned
    GameObject SpawnAnimalFromSource(Vector3 location, GameObject[] source)
    {
        GameObject newSpawn;

        if (source == commonPrey) Debug.Log("Spawning a prey");
        else if (source == commonPredators) Debug.Log("Spawning a predator");
        else Debug.Log("Spawning from other source.");

        // spawn random animal from source at location
        newSpawn = Instantiate(source[Random.Range(0, source.Length)], location, Quaternion.identity, animalsHolder.transform);

        if (newSpawn == null) Debug.Log("Issue while spawning animal");
        else
        {
            // adjust the name
            CleanName(newSpawn);

            // start despawn timer
            StartCoroutine(AnimalDespawnTimer(newSpawn));
            Debug.Log($"{newSpawn.name} spawned into the world.");
        }

        return newSpawn;
    }
    
    // Spawn a random event gameobject into the game world, returns the object spawned
    GameObject SpawnWildlifeEventFromSource(Vector3 location, GameObject[] source)
    {
        GameObject newEvent;

        if (source == commonEvents) Debug.Log("Spawning a common event");
        else if (source == uncommonEvents) Debug.Log("Spawning an uncommon event");
        else if (source == rareEvents) Debug.Log("Spawning a rare event");
        else Debug.Log("Spawning event from other source.");

        // spawn random event from source at location
        newEvent = Instantiate(source[Random.Range(0, source.Length)], location, Quaternion.identity, eventsHolder.transform);

        if (newEvent == null) Debug.Log("Issue while spawning event");
        else
        {
            // adjust the name
            CleanName(newEvent);

            // start despawn timer
            StartCoroutine(WildlifeEventDespawnTimer(newEvent));
            Debug.Log($"{newEvent.name} spawned into the world.");
        }

        return newEvent;
    }

    // Return a valid spawn location for a new animal
    Vector3 GenerateSpawnLocation()
    {
        // these ranges will need changed, but for now I just want to spawn stuff near the player
        Vector3 location = new Vector3(), playerPos = playerTransform.position;

        do
        {
            // get a point within the spawn range
            location.x = Random.Range(playerPos.x - eventDistanceFromPlayer, playerPos.x + eventDistanceFromPlayer);
            location.z = Random.Range(playerPos.z - eventDistanceFromPlayer, playerPos.z + eventDistanceFromPlayer);

            heightData = TerrainFunctions.GetTerrainPointData(new Vector2(location.x, location.z));
            // loop if point not on a chunk or if it's too close to player
        } while (!heightData.isHit || Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(location.x, location.z)) < minPlayerDistance);


        location.y = heightData.height + aboveGroundOffset;

        return location;
    }
    
    // Return a valid spawn location for a new animal
    Vector3 GenerateWildlifeEventLocation()
    {
        // these ranges will need changed, but for now I just want to spawn stuff near the player
        Vector3 location = new Vector3(), playerPos = playerTransform.position;

        do
        {
            // get a point within the spawn range
            location.x = Random.Range(playerPos.x - maxPlayerDistance, playerPos.x + maxPlayerDistance);
            location.z = Random.Range(playerPos.z - maxPlayerDistance, playerPos.z + maxPlayerDistance);

            heightData = TerrainFunctions.GetTerrainPointData(new Vector2(location.x, location.z));
            // loop if point not on a chunk or if it's too close to player
        } while (!heightData.isHit || Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(location.x, location.z)) < minPlayerDistance);

        // TODO: might add min distance from other animals as well.


        location.y = heightData.height + aboveGroundOffset;

        return location;
    }

    // Generate a random event rarity using the event rarities
    WildlifeEvent.EventRarity GenerateWildlifeEventRarity() 
    {
        float random = Random.Range(0.0f, 100.0f);
        if (random < rareEventRate) return WildlifeEvent.EventRarity.RARE;
        else if (random < uncommonEventRate) return WildlifeEvent.EventRarity.UNCOMMON;
        else return WildlifeEvent.EventRarity.COMMON;
    }

    // Despawn animal after despawn conditions are met
    IEnumerator AnimalDespawnTimer(GameObject animal)
    {
        do
        { // wait for despawn timer to hit, and then if player is out of range get rid of the animal
            yield return new WaitForSeconds(despawnDelaySeconds);
        } while (AnimalInRange(animal));

        // remove animal from list, and destroy in game
        worldAnimals.Remove(animal);
        Destroy(animal);
    }

    // Spawns animals into world at start
    IEnumerator SpawnFirstAnimals()
    {
        // cannot begin placing animals until there is a spot to put them in the world
        yield return new WaitUntil(ChunksRenderedAtPlayer);

        Vector3 spawnLoc;
        GameObject[] source;
        while(worldAnimals.Count < maxSpawns)
        {
            // get location to place animal
            spawnLoc = GenerateSpawnLocation();
            // get array to spawn from
            source = GetSpawnSource(); 

            // spawn random animal from selected array and add it to the list of spawned animals
            worldAnimals.Add(SpawnAnimalFromSource(spawnLoc, source));
        }
    }

    // TODO: Constant spawning during gameplay. Rather than player based for location, look at active chunks and spawn on the chunk
    IEnumerator AnimalSpawner()
    {
        yield return StartCoroutine(SpawnFirstAnimals());

        Vector3 spawnLoc;
        GameObject[] source;
        while (true)
        {
            yield return new WaitForSeconds(spawnDelaySeconds);
            yield return new WaitUntil(CanAddAnimal);

            // get location to place animal
            spawnLoc = GenerateSpawnLocation();
            // get array to spawn from
            source = GetSpawnSource();

            // spawn random animal from selected array and add it to the list of spawned animals
            worldAnimals.Add(SpawnAnimalFromSource(spawnLoc, source));
        }
    }

    IEnumerator WildlifeEventSpawner()
    {
        // cannot begin placing events until there is a spot to put them in the 
        yield return new WaitUntil(ChunksRenderedAtPlayer);

        Vector3 spawnLoc;
        GameObject[] eventSource;
        while (true)
        {
            yield return new WaitForSeconds(eventSpawnDelaySeconds);
            yield return new WaitUntil(CanAddWildlifeEvent);
            spawnLoc = GenerateWildlifeEventLocation();
            eventSource = GetWildlifeEventSource();

            worldEvents.Add(SpawnWildlifeEventFromSource(spawnLoc, eventSource));
        }
    }

    //Fill this in using the "duration" of wildlifeEvent's 'WildlifeEvent' component
    IEnumerator WildlifeEventDespawnTimer(GameObject wildlifeEventObject)
    {
        WildlifeEvent wildlifeEvent = wildlifeEventObject.GetComponent<WildlifeEvent>();
        
        do
        { // wait for duration, and then if player is out of range get rid of the wildlife event
            yield return new WaitForSeconds(wildlifeEvent.DurationSeconds);
        } while (WildlifeEventInRange(wildlifeEventObject));

        // remove wildlifeEventObject from list, and destroy in game
        worldEvents.Remove(wildlifeEventObject);
        Destroy(wildlifeEventObject);
    }
}
