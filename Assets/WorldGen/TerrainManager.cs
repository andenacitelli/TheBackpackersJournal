using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/* Manages, spawns, and deletes chunks. */
public class TerrainManager : MonoBehaviour
{
    // Unfortunately can't really make static variales editable from the Editor
    static public int generateRadius = 8;

    [SerializeField]
    private GameObject tilePrefab;

    public GameObject player;

    public GameObject TreeRockDropper;

    public static Noise heightNoise; // Used for heightmap values
    public static Noise moistureNoise; // Used for moisture values in order to pick biomes
    public static Noise colorRandomizationNoise; // Used to randomize vertex colors a little bit
    public static Noise heightFuzzingNoise; // Used to semi-randomly alter biome height cutoffs so it's less jarring when we get a shift

    // Chunks that have been fully generated
    static public Dictionary<Vector2Int, GameObject> chunks = new Dictionary<Vector2Int, GameObject>();

    // Chunks that are mid-coroutine - i.e. are generating in steps across several frames
    static public Dictionary<Vector2Int, GameObject> generatingChunks = new Dictionary<Vector2Int, GameObject>();
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
        moistureNoise.scale = 500; // We want biomes to be pretty large; this equates to making the Perlin change per unit very small 

        // GenerateChunks();
    }

    private void OnDisable()
    {
        Debug.Log("OnDisable-TerrainManager");
        chunks = new Dictionary<Vector2Int, GameObject>();
        generatingAChunk = false;
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
            currentChunks.Add(((int)(chunk.Value.transform.position.x / 80), (int)(chunk.Value.transform.position.z / 80)));
        }
        return currentChunks;
    }

    // If nothing is generating, we start a coroutine, then that coroutine switches this back when it's done, 
    // causing this to start a new coroutine ... ad nauseum
    public static bool generatingAChunk = false;
    private int frame = 0;
    void Update()
    {
        frame++;
        //print("Frame: " + frame);
        if (!generatingAChunk)
        {
            GenerateNearestChunk();
        }

        // Note that we can't really turn CullChunks() into a coroutine, as we can (and do) already limit it
        // to one Destroy() call per frame and that's really the smallest atomic unit in that function
        // (as opposed to ChunkGen.GenerateChunk(), which has like five separate steps we can separate into five frames of work
        // CullChunks(); // Destroy generated, out-of-range chunks 
    }

    void GenerateNearestChunk()
    {
        // Instantiate a tile at the given position
        Vector2Int playerPosChunks = new Vector2Int(
            Mathf.RoundToInt(player.transform.position.x / ChunkGen.size),
            Mathf.RoundToInt(player.transform.position.z / ChunkGen.size));

        Vector2Int currentChunk = new Vector2Int(playerPosChunks.x, playerPosChunks.y);
        int counter = 0;

        for (int currentRadius = 0; currentRadius < generateRadius; currentRadius++)
        {
            for (int xIndex = currentChunk.x - currentRadius; xIndex <= currentChunk.x + currentRadius; xIndex++)
            {
                for (int zIndex = currentChunk.y - currentRadius; zIndex <= currentChunk.y + currentRadius; zIndex++)
                {
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

                        // Only generate this chunk if it doesn't already exist 
                        if (!generatingChunks.ContainsKey(pos) && !chunks.ContainsKey(pos))
                        {

                            generatingAChunk = true;
                            Vector3 chunkPos = new Vector3(xIndex * ChunkGen.size, this.gameObject.transform.position.y, zIndex * ChunkGen.size);
                            //print("(TM):Initializing chunk at " + chunkPos);

                            // 0th Child is for Chunks; 1st child is for Clouds (this is 100% just for organizational purposes)
                            GameObject tile = Instantiate(tilePrefab, chunkPos, Quaternion.identity, this.gameObject.transform.GetChild(0)) as GameObject;
                            tile.SetActive(true);
                            generatingChunks.Add(pos, tile);

                            counter++;
                            //print("Starting coroutine for this chunk: " + chunkPos);
                            StartCoroutine(tile.GetComponent<ChunkGen>().GenerateChunk());
                            //print("Started the coroutine for this chunk: " + chunkPos);
                            // Application.Quit();
                            return;
                        }
                    }
                }
            }

        }

        /* 
        if (counter == 0 && finishedFirstTimeGeneration == false)
        {
            TreeRockDropper.SetActive(true);
        }
        */
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
        const int CHUNKS_TO_CULL_PER_FRAME = 1;
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