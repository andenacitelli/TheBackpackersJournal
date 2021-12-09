using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WorldGen;

public class SpawnManager : MonoBehaviour
{
    [Header("Animals to Spawn")]
    [SerializeField] GameObject[] availablePrey;
    [SerializeField] GameObject[] availablePredators;

    [Header("Animal Spawning")]
    // (chunk coordinates, List<animals on the chunk>)
    [HideInInspector] public readonly Dictionary<Vector3, HashSet<GameObject>> worldAnimals = new Dictionary<Vector3, HashSet<GameObject>>();

    SpawnSettings spawnSettings;
    TerrainFunctions.TerrainPointData heightData;
    Noise quantityNoise, creatureTypeNoise;

    Transform playerTransform;

    // containers to keep animals and events organized from everything else
    GameObject animalsHolder;

    void Awake()
    {
        spawnSettings = GetComponent<SpawnSettings>();

        quantityNoise = gameObject.AddComponent<Noise>(); // noise object with random seeding
        creatureTypeNoise = gameObject.AddComponent<Noise>();
        creatureTypeNoise.scale = 200.0f; // hopefully less change around a small area so you're more likely to spawn groups of the same kind


        // create animal and wildlife event containers
        animalsHolder = new GameObject("Animals");
        animalsHolder.transform.SetParent(transform, true);
        //eventsHolder = new GameObject("Events");
        //eventsHolder.transform.SetParent(transform, true);
    }

    // start the spawning coroutines and store reference to the player
    public void BeginSpawning(Transform playerTransform)
    {
        Debug.Log("Spawning started");
        this.playerTransform = playerTransform;

        // begin spawning animals and wildlife events into the world
        StartCoroutine(AnimalSpawner());
        //StartCoroutine(WildlifeEventSpawner());
    }

    // DEBUG: used to find where code stops working
    bool YieldForever() { return Input.GetMouseButtonDown(0); }

    // Return whether the chunks have been placed in world yet
    bool PlayerChunksHaveGenerated()
    {
        return TerrainFunctions.GetTerrainPointData(new Vector2(playerTransform.position.x, playerTransform.position.z)).isHit;
    }

    // Return if the object is still on an active chunk
    bool AnimalInRange(Vector3 animalPos)
    {
        Vector2 animalPos2D = new Vector2(animalPos.x, animalPos.z);
        Vector2 playerPos2D = new Vector2(playerTransform.position.x, playerTransform.position.z);

        return Vector2.Distance(animalPos2D, playerPos2D) < TerrainManager.generateRadius * ChunkGen.size;
    }

    // Return whether less than the maximum number of animals are spawned
    bool CanAddAnimal()
    {
        return spawnSettings.TotalSpawns < spawnSettings.MaxSpawns;
    }

    // return the Vector3 coordinates of the chunk that contains the point at (x,z)
    // if no chunk found, return infinity
    Vector3 GetChunkCoordinates(float x, float z)
    {
        Vector3 coordinates = Vector3.positiveInfinity;

        Vector2Int chunkCoords = new Vector2Int(Mathf.RoundToInt(x / ChunkGen.size), Mathf.RoundToInt(z / ChunkGen.size));
        GameObject chunk = TerrainManager.GetChunkAtCoords(chunkCoords);
        if (chunk != null)
        {
            coordinates = chunk.transform.position;
        }

        return coordinates;
    }

    // returns the array of animals to spawn from
    // uses noise at the location
    GameObject[] GetSpawnSource(Vector3 location)
    {
        // uses the noise at the spawnpoint to figure out what to place
        return creatureTypeNoise.GetNoiseAtPoint(location.x, location.z) <= spawnSettings.PreySpawnChance ? availablePrey : availablePredators;
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
        Vector3 animalChunkPos = GetChunkCoordinates(animal.transform.position.x, animal.transform.position.z);

        if (!animalChunkPos.Equals(Vector3.positiveInfinity))
        {
            // add chunk key to dictionary if it hasn't been added yet
            if (!worldAnimals.ContainsKey(animalChunkPos))
            {
                worldAnimals.Add(animalChunkPos, new HashSet<GameObject>());
            }

            // add animal to the list associated with its chunk
            worldAnimals[animalChunkPos].Add(animal);
//            Debug.Log("Successfully added animal to dictionary");
        }
        else
        {// if not on a chunk, remove the animal and destroy
            Debug.Log($"No chunk at {animal.transform.position}. Removing.");
            RemoveFromAnimals(animal, true);
        }
    }

    // removes the animal from the dictionary of world animals
    void RemoveFromAnimals(GameObject animal, bool toDestroy)
    {
        Vector3 animalChunkPos = GetChunkCoordinates(animal.transform.position.x, animal.transform.position.z);

        if (!animalChunkPos.Equals(Vector3.positiveInfinity))
        {
            if (worldAnimals.ContainsKey(animalChunkPos))
            {
                // remove animal from the list associated with its chunk
                worldAnimals[animalChunkPos].Remove(animal);
            }
            if (toDestroy) Destroy(animal);
        }
        else
        {
            Destroy(animal);
        }
    }

    // updates what chunk the animal is associated with in the dictionary
    void UpdateChunkAnimals(GameObject animal, Vector3 prevChunk)
    {
        Vector3 animalChunkPos = GetChunkCoordinates(animal.transform.position.x, animal.transform.position.z);

        Vector3 oldChunk = Vector3.negativeInfinity; // negative infinity because it can't be nulled

        // check each chunk with animals for the animal
        foreach (KeyValuePair<Vector3, HashSet<GameObject>> chunkAnimals in worldAnimals)
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

        if (!animalChunkPos.Equals(Vector3.positiveInfinity))
        {
            // add animal to the chunk at its current position
            AddToAnimals(animal);
        }

    }

    GameObject NoiseSpawn(Vector3 spawnPoint)
    {
        // use quantity to determine predator/prey distribution
        GameObject[] spawnSource;
        int spawnIndex = 0;

        // determine whether to spawn prey or predator
        spawnSource = GetSpawnSource(spawnPoint);

        //if (spawnSource == commonPrey) Debug.Log("Spawning a prey");
        //else if (spawnSource == commonPredators) Debug.Log("Spawning a predator");
        //else Debug.Log("Spawning from other source.");

        // spawn random animal from source at location
        float noise = creatureTypeNoise.GetNoiseAtPoint(spawnPoint.x, spawnPoint.z);

        if (noise > spawnSettings.SameSpeciesChance)
        {
            // pick random index from the source
            spawnIndex = Random.Range(0, spawnSource.Length);
        }

        // add new animal to world 
        return Instantiate(spawnSource[spawnIndex], spawnPoint, Quaternion.identity, animalsHolder.transform);
    }

    // uses noise to spawn animal(s) at location
    IEnumerator SpawnAnimalsWithNoise(Vector3 spawnCenter)
    {
        float noiseValue = quantityNoise.GetNoiseAtPoint(spawnCenter.x, spawnCenter.z);
        float multiS = spawnSettings.MultiSpawnChance, tripleS = multiS + spawnSettings.TripleSpawnChance, doubleS = tripleS + spawnSettings.DoubleSpawnChance;

        // get quantity to spawn
        int quantity;
        if (noiseValue < multiS) quantity = 5;
        else if (noiseValue < tripleS) quantity = 3;
        else if (noiseValue < doubleS) quantity = 2;
        else quantity = 1;


        GameObject newSpawn;
        Vector3 spawnPoint;
        List<GameObject> spawned = new List<GameObject>();
        List<Vector3> spawnedPoints = new List<Vector3>();

//        Debug.Log($"Attempting to spawn {quantity} animals.");
        //yield return new WaitUntil(YieldForever); // TESTING ONLY

        while (spawnSettings.TotalSpawns < spawnSettings.MaxSpawns && spawned.Count < quantity)
        {
            // get the actual locations around the spawn center to place animals
            spawnPoint = GenerateSpawnLocation(spawnCenter, spawnedPoints);

            // spawn the new animal
            newSpawn = NoiseSpawn(spawnPoint);

            if (newSpawn == null) Debug.Log("GameObject not placed in world. spawn Failed");
            else
            {
                // set movement bounds and start animal behavior             
                AnimalController animal = newSpawn.GetComponent<AnimalController>();
                animal.territory = new Bounds(GetChunkCoordinates(newSpawn.transform.position.x, newSpawn.transform.position.z), new Vector3(ChunkGen.size, 10.0f, ChunkGen.size));
                animal.LetsGetGoing();

                // fix the name
                CleanName(newSpawn);

                // start despawn timer
                StartCoroutine(AnimalDespawner(newSpawn));

                // add to world animals
                AddToAnimals(newSpawn);
                spawnedPoints.Add(spawnPoint);
                spawned.Add(newSpawn);

                //yield return new WaitUntil(YieldForever); // TESTING ONLY
            }

            yield return null;
        }
        //yield return new WaitUntil(YieldForever); // TESTING ONLY

    }

    // Return a valid spawn location for a new animal
    Vector3 GenerateSpawnLocation()
    {
        // these ranges will need changed, but for now I just want to spawn stuff near the player
        Vector3 location = new Vector3(), playerPos = playerTransform.position;
        Vector3 chunk;
        do
        {
            // get a point within the spawn range
            location.x = Random.Range(playerPos.x - spawnSettings.MaxPlayerDistance, playerPos.x + spawnSettings.MaxPlayerDistance);
            location.z = Random.Range(playerPos.z - spawnSettings.MaxPlayerDistance, playerPos.z + spawnSettings.MaxPlayerDistance);

            heightData = TerrainFunctions.GetTerrainPointData(new Vector2(location.x, location.z));
            // loop if point not on a chunk or if it's too close to player
            chunk = GetChunkCoordinates(location.x, location.z);
        } while (chunk.Equals(Vector3.positiveInfinity) || Vector2.Distance(new Vector2(playerPos.x, playerPos.z), new Vector2(location.x, location.z)) < spawnSettings.MinPlayerDistance);

        // set y posiiton to the ground adding the offset
        location.y = heightData.height + spawnSettings.AboveGroundDistance;

        //Debug.Log($"Generated spawn point at {location} on chunk at {chunk}");
        return location;
    }

    // Return a valid spawn location for a new animal near center
    Vector3 GenerateSpawnLocation(Vector3 center, List<Vector3> pointsUsed)
    {
        // these ranges will need changed, but for now I just want to spawn stuff near the player
        Vector3 location = new Vector3();
        Vector3 chunk;
        bool tooCloseToAnimals = false;
        do
        {
            // get a point within the spawn range
            location.x = Random.Range(center.x - spawnSettings.MaxSpawnCenterDistance, center.x + spawnSettings.MaxSpawnCenterDistance);
            location.z = Random.Range(center.z - spawnSettings.MaxSpawnCenterDistance, center.z + spawnSettings.MaxSpawnCenterDistance);

            heightData = TerrainFunctions.GetTerrainPointData(new Vector2(location.x, location.z));

            foreach (Vector3 usedLoc in pointsUsed)
            { // go through the spawn points used to see if the new one is too close to any
                if (Vector2.Distance(usedLoc, location) < spawnSettings.MinAnimalDistance)
                {
                    tooCloseToAnimals = true;
                    Debug.Log("Spawn point was too close to another animal. Regenerating");
                    break;
                }
            }
            chunk = GetChunkCoordinates(location.x, location.z);
        } while (chunk.Equals(Vector3.positiveInfinity) || tooCloseToAnimals);

        location.y = heightData.height + spawnSettings.AboveGroundDistance;

//        Debug.Log($"Generated spawn point at {location} on chunk at {chunk}");
        return location;
    }

    // Despawn animal after despawn conditions are met
    IEnumerator AnimalDespawner(GameObject animal)
    {
        while (AnimalInRange(animal.transform.position))
        {
            yield return null;
            if (animal == null) yield break;
        }
        Debug.Log($"{animal.name} too far away. Despawning.");
        // remove animal from list, and destroy in game
        RemoveFromAnimals(animal, true);
    }

    // Spawns animals into world at start
    IEnumerator SpawnFirstAnimals()
    {
        // cannot begin placing animals until there is a spot to put them in the world
        yield return new WaitUntil(PlayerChunksHaveGenerated);

        Debug.Log("Spawning initial animals");
        Vector3 spawnLoc;
        while (spawnSettings.TotalSpawns < spawnSettings.MaxSpawns)
        {
            // get location to place animal
            spawnLoc = GenerateSpawnLocation();

            // spawn random animal(s)
            //AddToAnimals(SpawnRandomAnimalFromSource(spawnLoc, source));
            //Debug.Log($"INITIAL: {spawnSettings.maxSpawns - spawnSettings.totalSpawns} slots remain. Spawning more.");
            yield return StartCoroutine(SpawnAnimalsWithNoise(spawnLoc));
        }
    }

    // Continually spawns animals into world.
    IEnumerator AnimalSpawner()
    {
        yield return StartCoroutine(SpawnFirstAnimals());

        Debug.Log("Initial spawns finished. Moving to active spawning.");
        Vector3 spawnLoc;
        while (true)
        {
            Debug.Log("Spawn cap reached. Waiting to spawn more...");
            yield return new WaitUntil(CanAddAnimal);
            float spawnDelay = Random.Range(spawnSettings.MinSpawnDelaySeconds, spawnSettings.MaxSpawnDelaySeconds);
            Debug.Log($"{spawnDelay} more seconds before spawning more animals...");
            yield return new WaitForSeconds(spawnDelay);

            //yield return new WaitUntil(YieldForever); // TESTING ONLY

            // get location to place animal
            spawnLoc = GenerateSpawnLocation();

            // spawn random animal(s)
            //AddToAnimals(SpawnRandomAnimalFromSource(spawnLoc, source));
            yield return StartCoroutine(SpawnAnimalsWithNoise(spawnLoc));
            Debug.Log($"Finished spawning at {spawnLoc}");
            //yield return new WaitUntil(YieldForever); // TESTING ONLY

        }
    }
}
