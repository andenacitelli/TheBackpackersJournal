using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WorldGen;
using Terrain = Assets.WorldGen.Terrain;
using System;
using Random = System.Random;

public class DropTree : MonoBehaviour
{

    public TerrainManager terrainManager;

    static private Dictionary<Vector3, GameObject> trees = new Dictionary<Vector3, GameObject>();
    static private Dictionary<(int, int), List<GameObject>> treesD = new Dictionary<(int, int), List<GameObject>>();
    static private List<(int, int)> currentChunks;
    static private List<(int, int)> toDoChunks;
    static private List<(int, int)> toRemoveChunks;
    static private List<(int, int)> finishedChunks;
    static private bool finishedFirstGeneration = false;

    public GameObject treePrefab;

    public Terrain terrain;

    // Start is called before the first frame update
    void Start()
    {
        currentChunks = new List<(int int1, int int2)>();
        currentChunks.Add((0,0));
        currentChunks.Add((0, 1));
        currentChunks.Add((0, -1));
        currentChunks.Add((1, 1));
        currentChunks.Add((1, 0));
        currentChunks.Add((1, -1));
        currentChunks.Add((-1, 1));
        currentChunks.Add((-1, 0));
        currentChunks.Add((-1, -1));
        toDoChunks = currentChunks;
        finishedChunks = new List<(int int1, int int2)>();
        toRemoveChunks = new List<(int int1, int int2)>();
    }


        // Update is called once per frame
        void Update()
        {
            if (TerrainManager.getChunksCords().Count == 9)
            {
                finishedFirstGeneration = true;
            }

        if (finishedFirstGeneration)
        {
            currentChunks = TerrainManager.getChunksCords();
        }
        toDoChunks = new List<(int, int)>();
        foreach ((int, int) chunk in currentChunks)
        {
            if (!finishedChunks.Contains(chunk))
            {
                toDoChunks.Add(chunk);
            }
        }
        GenerateTrees(); // Generate trees in in-range chunks
        toRemoveChunks = new List<(int, int)>();
        foreach ((int, int) chunk in finishedChunks)
        {
            if (!currentChunks.Contains(chunk))
            {
                toRemoveChunks.Add(chunk);
            }
        }
        if (currentChunks.Count != 0)
        {
            RemoveTrees(); // Destroy trees in out-range chunks
        }
        

    }

    void GenerateTrees()
    {        
        if (toDoChunks.Count != 0)
        {
            int xCord = toDoChunks[0].Item1;
            int yCord = toDoChunks[0].Item2;
            treesD.Add((xCord, yCord), new List<GameObject>());
            string chunkCord = "(" + xCord + ", " + yCord + ")";
            int seed = chunkCord.GetHashCode();
            Random chunkRand = new Random(seed);
            for (int i = 0; i < 1; i++)
            {
                Vector2 random2D = RandomPointInChunk(xCord, yCord, chunkRand);
                Vector3 pos = new Vector3(random2D.x, 50f, random2D.y);
                GameObject thisTree = Instantiate(treePrefab, pos, Quaternion.identity);
                thisTree.transform.localScale = new Vector3(50f,50f,50f);
                treesD[(xCord, yCord)].Add(thisTree);

/*                trees.Add(pos, Instantiate(treePrefab, pos, Quaternion.identity));*/
/*              Debug.Log(xCord + ", " + yCord + " tree dropped at " + pos.x + ", " + pos.z);*/
            }
            finishedChunks.Add( (xCord, yCord) );
/*            Debug.Log("DROPTREE__finishedGenerateing: " + xCord + ", " + yCord);*/
        }
    }

    void RemoveTrees()
    {
/*        (int, int) toRemoveFromFinished = (0,0);
        bool yesRemove = false;*/
        /*        foreach ((int, int) chunk in finishedChunks)
                {
                    Debug.Log("finishedChunks: " + chunk.Item1 + ", " + chunk.Item2);
                }
                Debug.Log("finishedChunks: DONE!");
                foreach ((int, int) chunk in currentChunks)
                {
                    Debug.Log("currentChunkss: " + chunk.Item1 + ", " + chunk.Item2);
                }
                Debug.Log("currentChunkss: DONE!");*/
        if (toRemoveChunks.Count != 0)
        {
            List<Vector3> toRemove = new List<Vector3>();
            int xCord = toRemoveChunks[0].Item1;
            int yCord = toRemoveChunks[0].Item2;
            float xFix = -40f + 80f * xCord;
            float yFix = -40f + 80f * yCord;
            foreach (GameObject tree in treesD[(xCord, yCord)])
            {
                Destroy(tree);
            }
            treesD.Remove((xCord, yCord));
            /*foreach (KeyValuePair<Vector3, GameObject> tree in trees)
            {                
                if (tree.Key.x <= xFix + 80f && tree.Key.x >= xFix && tree.Key.y <= yFix + 80f && tree.Key.y >= yFix)
                {
                    Destroy(tree.Value);
                    toRemove.Add(tree.Key);
                }
            }
            foreach (Vector3 tree in toRemove)
            {
                *//*                    Debug.Log(xCord + ", " + yCord + " tree culled at " + tree.x + ", " + tree.z);*//*
                trees.Remove(tree);
            }*/
            finishedChunks.Remove((xCord, yCord));
        }

/*        foreach ((int, int) chunk in finishedChunks)
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
                    if (tree.Key.x <= xFix + 80f && tree.Key.x >= xFix && tree.Key.y <= yFix + 80f && tree.Key.y >= yFix)
                    {
                        Destroy(tree.Value);
                        toRemove.Add(tree.Key);
                    }
                }

                foreach (Vector3 tree in toRemove)
                {
*//*                    Debug.Log(xCord + ", " + yCord + " tree culled at " + tree.x + ", " + tree.z);*//*
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
            Debug.Log("DROPTREE___REMOVEDD: " + toRemoveFromFinished.Item1 + ", " + toRemoveFromFinished.Item2);
        }*/

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
