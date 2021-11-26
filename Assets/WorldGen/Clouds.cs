using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.WorldGen
{
    public class Clouds : MonoBehaviour
    {
        // Prefabs we need access to
        [SerializeField]
        private GameObject player;

        // Probably better to figure out how to make this an array, but it's whatever
        private GameObject[] cloudPrefabs;
        [SerializeField]
        private GameObject cloudPrefab1;

        [SerializeField]
        private GameObject cloudPrefab2;

        [SerializeField]
        private GameObject cloudPrefab3;

        [SerializeField]
        private GameObject cloudPrefab4;

        // Various cloud constants
        [SerializeField]
        private float cloudHeightUpperBound;

        [SerializeField]
        private float cloudHeightLowerBound;

        [SerializeField]
        private float timeBetweenCloudSpawns;

        [SerializeField]
        private float CLOUD_SPEED;

        private List<GameObject> clouds;


        // Bounds which we can generate clouds in between; this is updated every Update() call to be centered around the player
        private Bounds genBounds;

        // Generates
        private Vector2[] GenRandomPoints(int n, Bounds bounds, float MIN_DISTANCE_BETWEEN)
        {
            genBounds = bounds;

            // Algorithm is a super naive one that ensures the generated points are a certain distance away (just so clouds don't overlap)
            // Rather inefficient at O(n^2), but n should never be high enough for this to matter
            // Can also technically run infinitely, but we never run it with a MIN_DISTANCE_BETWEEN argument value that is anywhere near high enough
            Vector2[] points = new Vector2[n]; 
            for (int i = 0; i < n; i++)
            {
                points[i] = new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.z, bounds.max.z));
                /* 
                bool valid = false;
                Vector2 nextPoint = new Vector2(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.z, bounds.max.z));
                int num_times = 0;
                while (!valid)
                {
                    if (num_times >= 200)
                    {
                        print("hit max num_times!");
                        break; 
                    }

                    num_times++;
                    valid = true;
                    for (int j = 0; j < i; j++)
                    {
                        // If we find an already existing cloud within range of this one 
                        if (Vector2.Distance(nextPoint, points[j]) < MIN_DISTANCE_BETWEEN)
                        {
                            valid = false;
                            break; // NOTE: Only breaks out of the inner loop
                        }
                    }
                }
                points[i] = nextPoint;
                */
            }

            return points;
        }

        // Use this for initialization
        void Start()
        {
            // Spawn like eight clouds initially
            genBounds = new Bounds(Vector3.zero, new Vector3(TerrainManager.generateRadius * ChunkGen.size, 0, TerrainManager.generateRadius * ChunkGen.size));
            cloudPrefabs = new GameObject[4];
            cloudPrefabs[0] = cloudPrefab1;
            cloudPrefabs[1] = cloudPrefab2;
            cloudPrefabs[2] = cloudPrefab3;
            cloudPrefabs[3] = cloudPrefab4;

            clouds = new List<GameObject>();
            const int NUM_CLOUDS = 50;
            const int MIN_DISTANCE_BETWEEN = 5; // Currently unused; writing code that takes this into account can be difficult
            
            // TODO: Rather than pure randomness, use a Perlin map (yes, AGAIN - THEY ARE SO GOOD) to determine cloud density
            Vector2[] points = GenRandomPoints(NUM_CLOUDS, 
                genBounds,
                MIN_DISTANCE_BETWEEN);

            foreach(Vector2 point in points)
            {
                int randomCloudIndex = (int) Random.Range(0, 4); // Truncates to get to random [0, 3] integer
                GameObject newCloud = Instantiate(cloudPrefabs[randomCloudIndex], transform.GetChild(1));
                newCloud.transform.position = new Vector3(point.x, Random.Range(cloudHeightLowerBound, cloudHeightUpperBound), point.y);
                newCloud.transform.rotation = Random.rotation;
                clouds.Add(newCloud);
            }
        }

        // Update is called once per frame
        float timeSinceLastCloud = 10;
        void Update()
        {
            timeSinceLastCloud += Time.deltaTime;
            genBounds = new Bounds(player.transform.position, new Vector3(TerrainManager.generateRadius * ChunkGen.size, 0, TerrainManager.generateRadius * ChunkGen.size));

            // Spawn a new cloud every ten seconds or so, all the way in the negative x coord and at a random z coord
            if (timeSinceLastCloud >= timeBetweenCloudSpawns)
            {
                print("Spawning a new cloud!");
                timeSinceLastCloud = 0;
                int randomCloudIndex = (int)Random.Range(0, 4); // Truncates to get to random [0, 3] integer
                print("randomCloudIndex: " + randomCloudIndex);

                // 0th child is for chunks, 1st child is for Clouds; Purely an organizational difference 
                GameObject newCloud = Instantiate(cloudPrefabs[randomCloudIndex], transform.GetChild(1));
                newCloud.transform.position = new Vector3(genBounds.min.x, Random.Range(cloudHeightLowerBound, cloudHeightUpperBound), Random.Range(genBounds.min.z, genBounds.max.z)); 
                clouds.Add(newCloud);
            }

            // Translate clouds a little bit towards positive x-coord
            // Think this technically belongs in fixedUpdate b/c it's physics but doesn't really matter
            foreach (GameObject cloud in clouds)
            {
                // Slight random movement perpendicular to the main axis of motion
                cloud.transform.position += new Vector3(CLOUD_SPEED, 0, Random.Range(-.1f * CLOUD_SPEED, .1f * CLOUD_SPEED));
            }

            // Check every cloud in the game 
            List<GameObject> elementsToRemove = new List<GameObject>(); // C# doesn't let you remove stuff from a list live
            foreach (GameObject cloud in clouds)
            {
                // Technically not precise seeing as the clouds have a y-coord as well
                Vector2 cloudPos = new Vector2(cloud.transform.position.x, cloud.transform.position.z);
                Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.z); 
                if (Vector2.Distance(cloudPos, playerPos) > TerrainManager.generateRadius * ChunkGen.size)
                {
                    print("Cloud is too far, removing from the game.");
                    elementsToRemove.Add(cloud);
                }
            }

            // Actually cull the ones out of range and remove them from our tracking list
            foreach(GameObject cloud in elementsToRemove)
            {
                clouds.Remove(cloud);
                Destroy(cloud);
            }
        }
    }
}