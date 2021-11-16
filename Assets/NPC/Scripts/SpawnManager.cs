using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WorldGen;

public class SpawnManager : MonoBehaviour
{
    [Header("Animals to Spawn")]
    [SerializeField] GameObject[] commonPrey;
    [SerializeField] GameObject[] commonPredators;

    [Header("Settings")]
    [SerializeField] SpawnSettings spawnSettings;

    [Header("Animal Spawning")]
    // (chunk coordinates, List<animals on the chunk>)
    readonly Dictionary<Vector3, HashSet<GameObject>> worldAnimals = new Dictionary<Vector3, HashSet<GameObject>>();

    [Header("Wildlife Events to Spawn")]
    [SerializeField] private GameObject[] commonEvents;
    [SerializeField] private GameObject[] uncommonEvents;
    [SerializeField] private GameObject[] rareEvents;

    [Header("Wildlife Event Spawning")]
    readonly List<GameObject> worldEvents = new List<GameObject>();

    [SerializeField] int maxEvents = 2;
    // chance that an additional wildlife event will spawn when one is spawning
    [SerializeField] float multiEventChance = 25.0f;

    [SerializeField] [Range(0.0f, 600.0f)] float eventSpawnDelaySeconds = 300.0f;
    [SerializeField] float eventDistanceFromPlayer = 50.0f;
    [SerializeField] float despawnEventAfterDistance = 200.0f;

    [Header("Should add up to less than 100")]
    // the common rate will be 100 - (uncommonEventRate + rareEventRate)
    // ranges for uncommon and rare are just what made sense to me  
    [SerializeField] [Range(20.0f, 40.0f)] private float uncommonEventRate = 38.25f;
    [SerializeField] [Range(0.0f, 10.0f)]  private float rareEventRate = 1.5f;

    TerrainFunctions.TerrainPointData heightData;
    Noise quantityNoise, creatureTypeNoise;

    Transform playerTransform;

    // containers to keep animals and events organized from everything else
    GameObject animalsHolder;
    GameObject eventsHolder;
    
    void Start()
    {
        quantityNoise = gameObject.AddComponent<Noise>(); // noise object with random seeding
        creatureTypeNoise = gameObject.AddComponent<Noise>();
        creatureTypeNoise.scale = 200.0f; // hopefully less change around a small area so you're more likely to spawn groups of the same kind

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        // create animal and wildlife event containers
        animalsHolder = new GameObject("Animals");
        animalsHolder.transform.SetParent(transform, true);
        eventsHolder = new GameObject("Events");
        eventsHolder.transform.SetParent(transform, true);


        // begin spawning animals and wildlife events into the world
        StartCoroutine(AnimalSpawner());
        //StartCoroutine(WildlifeEventSpawner());
    }

    // Return whether the chunks have been placed in world yet
    bool PlayerChunksHaveGenerated()
    {
        return TerrainFunctions.GetTerrainPointData(new Vector2(playerTransform.position.x, playerTransform.position.z)).isHit;
    }

    // Return if the object is still within distance of player as set by despawnRange
    bool AnimalInRange(GameObject animal)
    {
        return Vector3.Distance(playerTransform.position, animal.transform.position) < spawnSettings.despawnRange;
    }

    // Return if the WildlifeEvent is still within distance of player as set by despawnRange
    bool WildlifeEventInRange(GameObject wildlifeEvent)
    {
        return Vector3.Distance(playerTransform.position, wildlifeEvent.transform.position) < despawnEventAfterDistance;
    }

    // Return whether less than the maximum number of animals are spawned
    bool CanAddAnimal()
    {
        return worldAnimals.Count < spawnSettings.maxSpawns;
    }

    // Return whether less than the maximum number of events are spawned
    bool CanAddWildlifeEvent()
    {
        return worldEvents.Count < maxEvents;
    }

    // TODO: fix calls to check for null
    // return the chunk at the float coordinates given MAYBE MOVE TO TerrainManager.cs
    GameObject GetChunkAtCoords(float x, float z)
    {
        Vector2Int chunkCoords = new Vector2Int(Mathf.RoundToInt(x / ChunkGen.size), Mathf.RoundToInt(z / ChunkGen.size));
        return TerrainManager.GetChunkAtCoords(chunkCoords);
    }

    // returns the array of animals to spawn from
    // uses noise at the location
    GameObject[] GetSpawnSource(Vector3 location)
    {
        // uses the noise at the spawnpoint to figure out what to place
        return creatureTypeNoise.GetNoiseAtPoint(location.x, location.z) <= spawnSettings.preySpawnChance ? commonPrey : commonPredators;
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

    // Adds the newly spawned animal to the dictionary of world animals
    void AddToAnimals(GameObject animal)
    {
        Vector3 animalChunkPos = GetChunkAtCoords(animal.transform.position.x, animal.transform.position.z).transform.position;

        // add chunk key to dictionary if it hasn't been added yet
        if (!worldAnimals.ContainsKey(animalChunkPos))
        {
            worldAnimals.Add(animalChunkPos, new HashSet<GameObject>());
        }

        // add animal to the list associated with its chunk
        worldAnimals[animalChunkPos].Add(animal);
        spawnSettings.totalSpawns++;
    }

    // removes the animal from the dictionary of world animals
    void RemoveFromAnimals(GameObject animal)
    {
        Vector3 animalChunkPos = GetChunkAtCoords(animal.transform.position.x, animal.transform.position.z).transform.position;

        if (worldAnimals.ContainsKey(animalChunkPos))
        {
            // add animal to the list associated with its chunk
            worldAnimals[animalChunkPos].Remove(animal);
            spawnSettings.totalSpawns--;
        } else
        {
            // do nothing if animal is somehow not on a chunk
            Debug.Log("Animal apparently not on a chunk");
        }
    }

    // updates what chunk the animal is associated with in the dictionary
    void UpdateChunkAnimals(GameObject animal, Vector3 prevChunk)
    {
        Vector3 animalChunkPos = GetChunkAtCoords(animal.transform.position.x, animal.transform.position.z).transform.position;

        Vector3 oldChunk = Vector3.negativeInfinity; // negative infinity because it can't be nulled

        // check each chunk with animals for the animal
        foreach(KeyValuePair<Vector3, HashSet<GameObject>> chunkAnimals in worldAnimals)
        {
            if (chunkAnimals.Value.Contains(animal))
            {
                // get chunk location and stop searching if animal is found
                oldChunk = chunkAnimals.Key;
                break;
            }
        }

        if (!animalChunkPos.Equals(oldChunk) && !oldChunk.Equals(Vector3.negativeInfinity))
        { // if animal has moved to a new chunk, remove it from it's old one
            worldAnimals[oldChunk].Remove(animal);
        }

        // add animal to the chunk at its current position
        AddToAnimals(animal);
    }

    // Spawn a random animal gameobject into the game world, returns the object spawned
    GameObject SpawnRandomAnimalFromSource(Vector3 center, GameObject[] source)
    {
        GameObject newSpawn;

        if (source == commonPrey) Debug.Log("Spawning a prey");
        else if (source == commonPredators) Debug.Log("Spawning a predator");
        else Debug.Log("Spawning from other source.");

        // spawn random animal from source at location
        newSpawn = Instantiate(source[Random.Range(0, source.Length)], center, Quaternion.identity, animalsHolder.transform);

        if (newSpawn == null) Debug.Log("Issue while spawning animal");
        else
        {
            // adjust the name
            CleanName(newSpawn);

            // start despawn timer
            StartCoroutine(AnimalDespawnTimer(newSpawn));
            Debug.Log($"{newSpawn.name} spawned into the world.");

            Debug.Log($"SPAWNING {newSpawn.name} ON CHUNK AT {GetChunkAtCoords(center.x, center.z).transform.position}");
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
    Vector3 GenerateWildlifeEventLocation()
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
        } while (!heightData.isHit || Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(location.x, location.z)) < spawnSettings.minPlayerDistance);


        location.y = heightData.height + spawnSettings.aboveGroundDistance;

        return location;
    }

    GameObject SpawnWithNoise(Vector3 location)
    {
        float noiseValue = quantityNoise.GetNoiseAtPoint(location.x, location.z);

        // get quantity to spawn
        int quantity;
        if (noiseValue < .10f) quantity = 5;
        else if (noiseValue < .50f) quantity = 3;
        else if (noiseValue < .75f) quantity = 2;
        else quantity = 1;

        // use quantity to determine predator/prey distribution
        int newSpawns = 0;
        GameObject[] spawnSource;
        GameObject newSpawn;
        Vector3 placeLocation;
        while (spawnSettings.totalSpawns < spawnSettings.maxSpawns && newSpawns < quantity)
        {
            // get the actual location around the spawn center to place animals
            //placeLocation = GenerateSpawnLocation(location);

            // determine whether to spawn prey or predator
            //spawnSource = GetSpawnSource(placeLocation);

            //newSpawn = SpawnRandomAnimalFromSource(location, spawnSource);
        }

        return null;
    }

    // Return a valid spawn location for a new animal
    Vector3 GenerateSpawnLocation()
    {
        // these ranges will need changed, but for now I just want to spawn stuff near the player
        Vector3 location = new Vector3(), playerPos = playerTransform.position;

        do
        {
            // get a point within the spawn range
            location.x = Random.Range(playerPos.x - spawnSettings.maxPlayerDistance, playerPos.x + spawnSettings.maxPlayerDistance);
            location.z = Random.Range(playerPos.z - spawnSettings.maxPlayerDistance, playerPos.z + spawnSettings.maxPlayerDistance);

            heightData = TerrainFunctions.GetTerrainPointData(new Vector2(location.x, location.z));
            // loop if point not on a chunk or if it's too close to player
        } while (!heightData.isHit || Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(location.x, location.z)) < spawnSettings.minPlayerDistance);

        location.y = heightData.height + spawnSettings.aboveGroundDistance;

        return location;
    }

    // TODO: Return a valid spawn location for a new animal near center
    Vector3 GenerateSpawnLocation(Vector3 center, List<(float x, float z)> pointsUsed)
    {
        // these ranges will need changed, but for now I just want to spawn stuff near the player
        Vector3 location = new Vector3(), playerPos = playerTransform.position;

        do
        {
            // get a point within the spawn range
            location.x = Random.Range(playerPos.x - spawnSettings.maxPlayerDistance, playerPos.x + spawnSettings.maxPlayerDistance);
            location.z = Random.Range(playerPos.z - spawnSettings.maxPlayerDistance, playerPos.z + spawnSettings.maxPlayerDistance);

            heightData = TerrainFunctions.GetTerrainPointData(new Vector2(location.x, location.z));
            // loop if point not on a chunk or if it's too close to player
        } while (!heightData.isHit || Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(location.x, location.z)) < spawnSettings.minPlayerDistance);

        // TODO: might add min distance from other animals as well.


        location.y = heightData.height + spawnSettings.aboveGroundDistance;

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
            yield return new WaitForSeconds(spawnSettings.despawnDelaySeconds);
        } while (AnimalInRange(animal));

        // remove animal from list, and destroy in game
        RemoveFromAnimals(animal);
        Destroy(animal);
    }

    // Spawns animals into world at start
    IEnumerator SpawnFirstAnimals()
    {
        // cannot begin placing animals until there is a spot to put them in the world
        yield return new WaitUntil(PlayerChunksHaveGenerated);

        Vector3 spawnLoc;
        while(worldAnimals.Count < spawnSettings.maxSpawns)
        {
            // get location to place animal
            spawnLoc = GenerateSpawnLocation();
            
            // spawn random animal from selected array and add it to the list of spawned animals
            //AddToAnimals(SpawnRandomAnimalFromSource(spawnLoc, source));
            AddToAnimals(SpawnWithNoise(spawnLoc));
        }
    }

    // TODO: Constant spawning during gameplay. Rather than player based for location, look at active chunks and spawn on the chunk
    IEnumerator AnimalSpawner()
    {
        yield return StartCoroutine(SpawnFirstAnimals());

        Vector3 spawnLoc;
        while (true)
        {
            yield return new WaitForSeconds(spawnSettings.spawnDelaySeconds);
            yield return new WaitUntil(CanAddAnimal);

            // get location to place animal
            spawnLoc = GenerateSpawnLocation();

            // spawn random animal from selected array and add it to the list of spawned animals
            //AddToAnimals(SpawnRandomAnimalFromSource(spawnLoc, source));
            AddToAnimals(SpawnWithNoise(spawnLoc));
        }
    }

    IEnumerator WildlifeEventSpawner()
    {
        // cannot begin placing events until there is a spot to put them in the 
        yield return new WaitUntil(PlayerChunksHaveGenerated);

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
