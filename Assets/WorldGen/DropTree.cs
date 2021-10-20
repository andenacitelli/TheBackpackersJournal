using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WorldGen;
using Terrain = Assets.WorldGen.Terrain;
using System;
using Random = System.Random;

public class DropTree : MonoBehaviour
{

    static private Dictionary<Vector3, GameObject> trees = new Dictionary<Vector3, GameObject>();
    static private List<(int, int)> currentChunks;
    static private List<(int, int)> toDoChunks;
    static private List<(int, int)> finishedChunks;

    public GameObject treePrefab;

    public Terrain terrain;

    // Start is called before the first frame update
    void Start()
    {
        toDoChunks = currentChunks;
        finishedChunks = new List<(int int1, int int2)>();
    }

    // Update is called once per frame
    void Update()
    {
        GenerateTrees(); // Generate trees in in-range chunks
        RemoveTrees(); // Destroy trees in out-range chunks
    }

    void GenerateTrees()
    {
        while (toDoChunks.Count != 0)
        {
            int xCord = toDoChunks[0].Item1;
            int yCord = toDoChunks[0].Item2;
            string chunkCord = "(" + xCord + ", " + yCord + ")";
            int seed = chunkCord.GetHashCode();
            Random chunkRand = new Random(seed);
            for (int i = 0; i < 5; i++)
            {
                Vector2 random2D = RandomPointInChunk(xCord, yCord, chunkRand);
                Vector3 pos = new Vector3(random2D.x, 50f, random2D.y);
                trees.Add(pos, Instantiate(treePrefab, pos, Quaternion.identity));
            }
            finishedChunks.Add( (xCord, yCord) );
            toDoChunks.RemoveAt(0);
        }
    }

    void RemoveTrees()
    {
        (int, int) toRemoveFromFinished = (0,0);
        bool yesRemove = false;
        foreach ((int, int) chunk in finishedChunks)
        {
            if (!currentChunks.Contains(chunk))
            {
                List<Vector3> toRemove = new List<Vector3>();
                int xCord = chunk.Item1;
                int yCord = chunk.Item2;

                float xFix = -40f + 80f * xCord;
                float yFix = -40f + 80f * yCord;
                foreach (KeyValuePair<Vector3, GameObject> tree in trees)
                {
                    if (tree.Key.x <= xFix + 80f && tree.Key.x >= xFix)
                    {
                        Destroy(tree.Value);
                        toRemove.Add(tree.Key);
                    }
                }

                foreach (Vector3 tree in toRemove)
                {
                    trees.Remove(tree);
                }
                toRemoveFromFinished = chunk;
                yesRemove = true;
                goto RemoveChunkFromFinished;
            }
        }
    RemoveChunkFromFinished:
        if (yesRemove)
        {
            finishedChunks.Remove(toRemoveFromFinished);
        }

    }

    private static Vector2 RandomPointInChunk(int xCord, int yCord, Random chunkRand)
    {
        float range = 80f;
        float xFix = -40f + 80f * xCord;
        float yFix = -40f + 80f * yCord;
        
        float x = (float)((chunkRand.NextDouble() * range) + xFix);
        float z = (float)((chunkRand.NextDouble() * range) + yFix);

        return new Vector2(x, z);
    }
}
