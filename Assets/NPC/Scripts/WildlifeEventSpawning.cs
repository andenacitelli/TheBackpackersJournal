using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WorldGen;

public class WildlifeEventSpawning : MonoBehaviour
{
    [Header("Wildlife Events to Spawn")]
    [SerializeField] private GameObject[] commonEvents;
    [SerializeField] private GameObject[] uncommonEvents;
    [SerializeField] private GameObject[] rareEvents;

    [Header("Wildlife Event Spawning")]
    readonly List<GameObject> worldEvents = new List<GameObject>();
    [SerializeField] int maxEvents = 2;
    // chance that an additional wildlife event will spawn when one is spawning
//    [SerializeField] float multiEventChance = 25.0f;

    [SerializeField] [Range(0.0f, 600.0f)] float eventSpawnDelaySeconds = 300.0f;
    [SerializeField] float eventDistanceFromPlayer = 50.0f;
    [SerializeField] float despawnEventAfterDistance = 200.0f;

    [Header("Should add up to less than 100")]
    // the common rate will be 100 - (uncommonEventRate + rareEventRate)
    // ranges for uncommon and rare are just what made sense to me  
    [SerializeField] [Range(20.0f, 40.0f)] private float uncommonEventRate = 38.25f;
    [SerializeField] [Range(0.0f, 10.0f)] private float rareEventRate = 1.5f;
    GameObject eventsHolder;
    SpawnSettings spawnSettings;
    Transform playerTransform;
    TerrainFunctions.TerrainPointData heightData;

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
            //CleanName(newEvent);

            // start despawn timer
            StartCoroutine(WildlifeEventDespawnTimer(newEvent));
            Debug.Log($"{newEvent.name} spawned into the world.");
        }

        return newEvent;
    }

    // Return whether less than the maximum number of events are spawned
    bool CanAddWildlifeEvent()
    {
        return worldEvents.Count < maxEvents;
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

    // Generate a random event rarity using the event rarities
    WildlifeEvent.EventRarity GenerateWildlifeEventRarity()
    {
        float random = Random.Range(0.0f, 100.0f);
        if (random < rareEventRate) return WildlifeEvent.EventRarity.RARE;
        else if (random < uncommonEventRate) return WildlifeEvent.EventRarity.UNCOMMON;
        else return WildlifeEvent.EventRarity.COMMON;
    }

    IEnumerator WildlifeEventSpawner()
    {
        // cannot begin placing events until there is a spot to put them in the 
        //yield return new WaitUntil(PlayerChunksHaveGenerated);

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

    // Return if the WildlifeEvent is still within distance of player as set by despawnRange
    bool WildlifeEventInRange(GameObject wildlifeEvent)
    {
        return Vector3.Distance(playerTransform.position, wildlifeEvent.transform.position) < despawnEventAfterDistance;
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
