using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private GameObject player; 

    [SerializeField]
    private int generateRadius;

    // Holds references to chunks we've generated so that we don't regenerate them 
    private Dictionary<Vector2, GameObject> chunks = new Dictionary<Vector2, GameObject>();

    private void Start()
    {
        Physics.autoSyncTransforms = true;    
    }

    void Update() 
    {
        GenerateChunks(); // Generate ungenerated, in-range chunks
        CullChunks(); // Destroy generated, out-of-range chunks 
    }

    void GenerateChunks()
    {
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int) tileSize.x;
        int tileDepth = (int) tileSize.z;

        // Instantiate a tile at the given position
        Vector2Int playerPosChunks = new Vector2Int(
            (int) player.transform.position.x / (tileWidth * (tileWidth / 10)), 
            (int) player.transform.position.z / (tileWidth * (tileWidth / 10)));

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
        List<Vector2Int> iterationOrder = new List<Vector2Int>();
        Vector2Int currentChunk = new Vector2Int(playerPosChunks.x, playerPosChunks.y);
        for (int currentRadius = 0; currentRadius < generateRadius; currentRadius++)
        {
            for (int xIndex = currentChunk.x - currentRadius; xIndex < currentChunk.x + currentRadius; xIndex++)
            {
                for (int zIndex = currentChunk.y - currentRadius; zIndex < currentChunk.y + currentRadius; zIndex++)
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
                            Vector3 chunkPos = new Vector3(xIndex * tileWidth, this.gameObject.transform.position.y, zIndex * tileDepth);
                            GameObject tile = Instantiate(tilePrefab, chunkPos, Quaternion.identity, this.gameObject.transform) as GameObject;
                            tile.GetComponent<ChunkGen>().coords = pos;
                            chunks[pos] = tile;
                            chunksGeneratedThisFrame++;
                        }
                    }
                }
            }
        }
    }

    // Removes elements 
    // TODO: Probably more efficient to change camera render distance and Unity will automatally "unload" GameObjects without having to regenerate them when we get back in range 
    void CullChunks()
    {
        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int tileWidth = (int) tileSize.x;
        int tileDepth = (int) tileSize.z;

        Vector2Int playerPosChunks = new Vector2Int(
            (int) player.transform.position.x / (tileWidth * (tileWidth / 10)), 
            (int) player.transform.position.z / (tileDepth * (tileDepth / 10)));

        // Get a list of chunks to remove; we can't actually remove them yet, as you can't modify a dictionary during iterations 
        List<Vector2> keysToRemove = new List<Vector2>();
        const int CHUNKS_TO_CULL_PER_FRAME = 5;
        int chunksCulled = 0;
        foreach (KeyValuePair<Vector2, GameObject> chunkPair in chunks)
        {
            if (Vector2.Distance(chunkPair.Key, playerPosChunks) > generateRadius)
            {
                keysToRemove.Add(chunkPair.Key);

                // If we've hit our limit, stop searching for more chunks to cull
                chunksCulled++;
                if (chunksCulled >= CHUNKS_TO_CULL_PER_FRAME)
                {
                    break;
                }
            }
        }

        // Actually destroy the ones that were out of range 
        foreach (Vector2 chunkKey in keysToRemove)
        {
            Destroy(chunks[chunkKey]);
            chunks.Remove(chunkKey);
        }
    }
}