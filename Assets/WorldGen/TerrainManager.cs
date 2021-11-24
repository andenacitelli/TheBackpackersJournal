using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* Manages, spawns, and deletes chunks. */
public class TerrainManager : MonoBehaviour
{
    // Unfortunately can't really make static variales editable from the Editor
    static public int generateRadius = 6; 

    [SerializeField]
    private GameObject tilePrefab;

    public GameObject player; 

    public GameObject TreeRockDropper;

    public static Noise heightNoise; // Used for heightmap values
    public static Noise moistureNoise; // Used for moisture values in order to pick biomes
    public static Noise colorRandomizationNoise; // Used to randomize vertex colors a little bit
    public static Noise heightFuzzingNoise; // Used to semi-randomly alter biome height cutoffs so it's less jarring when we get a shift

    // Holds references to chunks we've generated so that we don't regenerate them 
    static private Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();

    private static bool finishedFirstTimeGeneration = false;
    private void Start()
    {
        Debug.Log("Start - TerrainManager");
        Physics.autoSyncTransforms = true;
        //player = GameObject.Find("WorldGenPlayer");

        // Initialize noise functions used by World Generation
        heightNoise = gameObject.AddComponent<Noise>(); // Heightmap
        heightNoise.scale = 170;
        colorRandomizationNoise = gameObject.AddComponent<Noise>(); // Used to tweak vertex colors a bit to give us a more tesselated look
        colorRandomizationNoise.scale = 50;
        heightFuzzingNoise = gameObject.AddComponent<Noise>(); // Used to make biome height boundaries a little more fuzzy to avoid the discrete layers look
        heightFuzzingNoise.scale = 50;
        moistureNoise = gameObject.AddComponent<Noise>();
        moistureNoise.scale = 400; // We want biomes to be pretty large; this equates to making the Perlin change per unit very small 

        GenerateChunks();

    }

    private void OnDisable()
    {
        Debug.Log("OnDisable-TerrainManager");
        chunks = new Dictionary<Vector2Int, GameObject>();
    }

    // Essentially just a getter
    // Used by chunks to get info about topography of other neighboring chunks when interpolating Delaunay stuff between them 
    public static GameObject GetChunkAtCoords(Vector2Int coords)
    {
        return chunks.ContainsKey(coords) ? chunks[coords] : null; 
    }

    public static List<(int, int)> getChunksCords()
    {
        List<(int, int)> currentChunks = new List<(int int1, int int2)>();
        foreach (KeyValuePair<Vector2Int, GameObject> chunk in chunks)
        {
            currentChunks.Add(((int)(chunk.Value.transform.position.x/80), (int)(chunk.Value.transform.position.z / 80)));
        }
        return currentChunks;
    }

    void Update() 
    {
        GenerateChunks(); // Generate ungenerated, in-range chunks
        CullChunks(); // Destroy generated, out-of-range chunks 
    }

    void GenerateChunks()
    {
        // Instantiate a tile at the given position
        Vector2Int playerPosChunks = new Vector2Int(
            Mathf.RoundToInt(player.transform.position.x / ChunkGen.size), 
            Mathf.RoundToInt(player.transform.position.z / ChunkGen.size));

        // Generate a fixed amount of frames per UPDATE call
        // If this were FixedUpdate, we'd run into an issue where we could only hit a certain render distance
        // However, because this is Update, update should get called more frequently on a more powerful computer and thus
        // world generation can scale to higher chunk distances
        const byte CHUNKS_TO_GENERATE_PER_FRAME = 3;
        int chunksGeneratedThisFrame = 0;

        // While the easiest implementation iterates through things minimum x/z to maximum x/z, 
        // we want to generate the closest frames (measured by simple euclidian distance) first.
        // Option 1: Slap every close chunk coord in a list, then take the minimum out each time. This is incredibly unperformant and we can do better.
        // Option 2: Throw them in a heap or some data structure that lets us take the minimum one out each time.
        // Option 3: Keep a variable generating a "current radius" that tracks how far out we are.
        //      We basically generate the chunk right under the player, then all chunks radius 1 away, then all chunks radius 2 away... 
        // Option 3: Find a way to preserve some of the work across frames.
        //      Theoretically, the most we should ever recalculate and reorder is
        //      the frame that a player switches chunks, if not even less frequently.
        Vector2Int currentChunk = new Vector2Int(playerPosChunks.x, playerPosChunks.y);
        int counter = 0;
        for (int currentRadius = 0; currentRadius < generateRadius; currentRadius++)
        {
            for (int xIndex = currentChunk.x - currentRadius; xIndex <= currentChunk.x + currentRadius; xIndex++)
            {
                for (int zIndex = currentChunk.y - currentRadius; zIndex <= currentChunk.y + currentRadius; zIndex++)
                {
                    // TODO: Probably possible and more performant to "save" where we were in this loop and only fully reset
                    // iteration when we switch chunks (maximum theoretical we need) or even less frequently (i.e. 3 chunks)
                    if (chunksGeneratedThisFrame > CHUNKS_TO_GENERATE_PER_FRAME)
                    {
                        return;
                    }

                    // Skip any chunk that's within the square-wise radius but not within the circle-wise radius
                    // TODO: Probably a better spiraling algorithm possible that doesn't require a `continue`, but better things to work on rn
                    if (Vector2Int.Distance(playerPosChunks, new Vector2Int(xIndex, zIndex)) > generateRadius)
                    {
                        continue;
                    }

                    // Only generate if it's on the outside border x-wise or z-wise
                    if (xIndex != currentChunk.x - currentRadius && xIndex != currentChunk.x + currentRadius && 
                        zIndex != currentChunk.y - currentRadius && zIndex != currentChunk.y + currentRadius)
                    {
                        // Chunk pos (in chunks)
                        Vector2Int pos = new Vector2Int(xIndex, zIndex);

                        // Only generate chunk if it doesn't already exist 
                        if (!chunks.ContainsKey(pos))
                        {
                            Vector3 chunkPos = new Vector3(xIndex * ChunkGen.size, this.gameObject.transform.position.y, zIndex * ChunkGen.size);
                            print("(TM):Initializing chunk at " + chunkPos);
                            counter++;
                            GameObject tile = Instantiate(tilePrefab, chunkPos, Quaternion.identity, this.gameObject.transform) as GameObject;
                            tile.GetComponent<ChunkGen>().GenerateChunk();
                            tile.SetActive(true);
                            tile.layer = LayerMask.NameToLayer("Terrain");
                            chunks[pos] = tile;
                            chunksGeneratedThisFrame++;
                        }
                    }
                }
            }
        }
        if(counter == 0 && finishedFirstTimeGeneration == false)
        {
            TreeRockDropper.SetActive(true);
        }
    }

    // Removes elements 
    void CullChunks()
    {
        // Instantiate a tile at the given position
        Vector2Int playerPosChunks = new Vector2Int(
            Mathf.RoundToInt(player.transform.position.x / ChunkGen.size),
            Mathf.RoundToInt(player.transform.position.z / ChunkGen.size));

        // Get a list of chunks to remove; we can't actually remove them yet, as you can't modify a dictionary during iterations 
        List<Vector2Int> keysToRemove = new List<Vector2Int>();
        const int CHUNKS_TO_CULL_PER_FRAME = 5;
        int chunksCulled = 0;
        foreach (KeyValuePair<Vector2Int, GameObject> chunkPair in chunks)
        {
            // So, I need to make sure I entirely generate the 
            if (Vector2Int.Distance(chunkPair.Key, playerPosChunks) > generateRadius)
            {
                keysToRemove.Add(chunkPair.Key);

                // If we've hit our limit, stop searching for more chunks to cull this frame 
                chunksCulled++;
                if (chunksCulled >= CHUNKS_TO_CULL_PER_FRAME)
                {
                    break;
                }
            }
        }

        // Actually destroy the ones that were out of range 
        foreach (Vector2Int chunkKey in keysToRemove)
        {
            Destroy(chunks[chunkKey]);
            chunks.Remove(chunkKey);
        }
    }
}