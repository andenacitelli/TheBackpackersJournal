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
            (int) player.transform.position.x / tileWidth, 
            (int) player.transform.position.z / tileDepth);

        // Iterate through chunks around player to see if they need generated 
        int xStart, xEnd, zStart, zEnd;
        xStart =  playerPosChunks.x - generateRadius; 
        xEnd = playerPosChunks.x + generateRadius;
        for (int xIndex = xStart; xIndex < xEnd; xIndex++)
        {
            zStart = playerPosChunks.y - generateRadius; 
            zEnd = playerPosChunks.y + generateRadius;
            for (int zIndex = zStart; zIndex < zEnd; zIndex++)
            {
                // Chunk pos (in chunks)
                Vector2 pos = new Vector2(xIndex, zIndex);

                // Generate in circular radius; 
                // Simpler (and barely less optimal) than trying to generate iteration order through a sphere in the first place 
                if (Vector2.Distance(pos, playerPosChunks) > generateRadius)
                {
                    continue;
                }

                // Only generate chunk if it doesn't already exist 
                if (!chunks.ContainsKey(pos))
                {
                    // Chunk pos (in absolute world coordinates) 
                    Vector3 chunkPos = new Vector3(this.gameObject.transform.position.x + xIndex * tileWidth, 
                        this.gameObject.transform.position.y, 
                        this.gameObject.transform.position.z + zIndex * tileDepth);

                    // Instantiate new tile GameObject 
                    // Syntax: Instantiate(<prefab>, <parent transform>, <rotation>)
                    GameObject tile = Instantiate(tilePrefab, chunkPos, Quaternion.identity) as GameObject;
                    chunks[pos] = tile;
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
            (int) player.transform.position.x / tileWidth, 
            (int) player.transform.position.z / tileDepth);

        // Get a list of chunks to remove; we can't actually remove them yet, as you can't modify a dictionary during iterations 
        List<Vector2> keysToRemove = new List<Vector2>();
        foreach (KeyValuePair<Vector2, GameObject> chunkPair in chunks)
        {
            if (Vector2.Distance(chunkPair.Key, playerPosChunks) > generateRadius)
            {
                keysToRemove.Add(chunkPair.Key);
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