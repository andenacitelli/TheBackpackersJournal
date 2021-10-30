using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WorldGen;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] GameObject[] commonPrey;
    [SerializeField] GameObject[] commonPredators;

    [Header("Animal Spawn Settings")]
    [SerializeField] int maxSpawns = 10;
    [SerializeField] float minPlayerDistance = 10.0f;
    [SerializeField] float maxPlayerDistance = 40.0f;
    [SerializeField] float minAnimalsDistance = 5.0f;
    [SerializeField] float aboveGroundOffset = 1.0f; // to ensure that they spawn above ground level. they'll drop to touch ground with gravity


    // The chance that an animal spawned will be predator or prey
    // predator chance will be equal to 100 - prey chance.
    [Range(0.0f, 100.0f)]
    [SerializeField] float preySpawnChance = 65.0f;
    float predatorSpawnChance;


    [Header("Despawn Settings")]
    [SerializeField] float despawnRange = 20.0f;
    [SerializeField] float despawnTime = 120.0f; // time ater player leaves range to despawn animals

    TerrainFunctions.TerrainPointData heightData;
    Transform playerTransform;

    // container to keep animals organized from everything else
    GameObject animalsHolder;
    
    readonly List<GameObject> worldAnimals = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        predatorSpawnChance = 100.0f - preySpawnChance;

        // create animal container in world
        animalsHolder = new GameObject("Animals");
        // begin spawning animals into the world
        StartCoroutine(SpawnCycle());

    }
    
    // Update is called once per frame
    void Update()
    {
        
    }

    // Return whether the chunks have been placed in world yet
    bool ChunksRendered()
    {
        return TerrainFunctions.GetTerrainPointData(new Vector2(playerTransform.position.x, playerTransform.position.y)).isHit;
    }

    // Return if the object is further from the player than specified by despawnRange
    bool OutOfPlayerRange(GameObject animal)
    {
        return Vector3.Distance(playerTransform.position, animal.transform.position) >= despawnRange;
    }
    IEnumerator SpawnCycle()
    {
        // cannot begin placing animals until there is a spot to put them in the world
        yield return new WaitUntil(ChunksRendered);

        Vector3 spawnLoc;
        heightData = TerrainFunctions.GetTerrainPointData(new Vector2(playerTransform.position.x, playerTransform.position.z));
        spawnLoc = GenerateSpawnLocation();


        GameObject[] spawnFrom;
        while(worldAnimals.Count < maxSpawns)
        {
            Vector3 spawnAt = GenerateSpawnLocation();
            GameObject newSpawn;
            // choose whether to spawn a predator or prey
            spawnFrom = GetSpawnSource();
            if (spawnFrom == commonPrey) Debug.Log("Spawning a prey");
            else Debug.Log("Spawning a predator");

            // spawn random animal from selected array and add it to the list of spawned animals
            newSpawn = Instantiate(spawnFrom[Random.Range(0, spawnFrom.Length)], spawnAt, Quaternion.identity, animalsHolder.transform);
            worldAnimals.Add(newSpawn);

            // clean up the object's name to help with photoID obviously won't work if we 
            string objectName = newSpawn.name;
            if (objectName.Contains("(Clone)")) newSpawn.name = objectName.Substring(0, objectName.LastIndexOf("(Clone)"));
            // start despawn timer
            StartCoroutine(DespawnTimer(newSpawn));
            Debug.Log($"{newSpawn.name} spawned into the world.");
        }
    }

    // Despawn animal
    IEnumerator DespawnTimer(GameObject animal)
    {
        do
        { // wait for despawn timer to hit, and then if player is out of range get rid of the animal
            yield return new WaitForSeconds(despawnTime);
        } while (Vector3.Distance(playerTransform.position, animal.transform.position) < despawnRange);

        worldAnimals.Remove(animal);
        Destroy(animal);
    }
    // returns the array of animals to spawn from
    GameObject[] GetSpawnSource()
    {
        return Random.Range(0.0f, 100.0f) <= preySpawnChance ? commonPrey : commonPredators;
    }
    // Return a valid spawn location for a new animal
    Vector3 GenerateSpawnLocation()
    {
        // these ranges will need changed, but for now I just want to spawn stuff near the player
        Vector3 location = new Vector3(), playerPos = playerTransform.position;

        do {
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
}
