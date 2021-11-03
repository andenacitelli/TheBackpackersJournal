using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WorldGen;
using Terrain = Assets.WorldGen;
using System;
using Random = System.Random;

[System.Serializable]
public class plantsGroup
{
    public int groupType; // which pattern are we following
    public List<GameObject> gameObjects; //GameObjects in this group
    public (float, float) rangeX; // boundaries of x of this group
    public (float, float) rangeZ; // boundaries of z of this group

    public plantsGroup()
    {
        gameObjects = new List<GameObject>();        
    }

    public void removePlantsGroup()
    {
        foreach (GameObject gameObject in gameObjects)
        {
            UnityEngine.Object.Destroy(gameObject);
        }
        
    }
}

public class DropTree : MonoBehaviour
{

    public TerrainManager terrainManager;

    static private Dictionary<Vector3, GameObject> trees = new Dictionary<Vector3, GameObject>();
    static private Dictionary<(int, int), List<GameObject>> treesD = new Dictionary<(int, int), List<GameObject>>();
    static private Dictionary<(int, int), List<plantsGroup>> plantsD = new Dictionary<(int, int), List<plantsGroup>>();
    static private List<(int, int)> currentChunks;
    static private List<(int, int)> toDoChunks;
    static private List<(int, int)> toRemoveChunks;
    static private List<(int, int)> finishedChunks;
    static private bool finishedFirstGeneration = false;

    public List<GameObject> bushPrefabs;
    public List<GameObject> flowerPrefabs;
    public List<GameObject> grassPrefabs;
    public List<GameObject> mushroomPrefabs;
    public List<GameObject> waterPlantPrefabs;
    public GameObject treePrefab;
    
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

    private void OnDisable()
    {
        // JAMES SEE HERE:
        /*
         * This function [^OnDisable()] is called everytime the scene ends.
         * The code below shows a good example of what is going on with how
         * the additive wilderness scene is affecting your code. To fix the error thrown
         * when running the game from the main menu, just uncomment the line below:
         */
        //plantsD = new Dictionary<(int, int), List<plantsGroup>>();

        /*GENERAL NOTES ON DESIGN of foliage:
         * -----> NEEDS FIXES <------
         * 
         * On testing, this method of PCG is not producing content on new chunk generation.
         * That is, the plants don't keep going forever. I think a big cause of  this issue is that 
         * the vegation isnt being instantiated as children of the chunks they belong to. I'm not entirely
         * sure how counter-intuitive it is, just my thoughts.
         * 
         */
        Debug.Log("DropTree: OnDisable Ran");
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
            plantsD.Add((xCord, yCord), new List<plantsGroup>());
            /*treesD.Add((xCord, yCord), new List<GameObject>());*/
            string chunkCord = "(" + xCord + ", " + yCord + ")";
            int seed = chunkCord.GetHashCode();
            int grassSeed = (chunkCord + "grass").GetHashCode();
            int flowerSeed = (chunkCord + "flower").GetHashCode();
            int bushSeed = (chunkCord + "bush").GetHashCode();
            int shroomSeed = (chunkCord + "shroom").GetHashCode();
            int waterPlantSeed = (chunkCord + "waterPlant").GetHashCode();
            Random chunkRand = new Random(seed);
            List<Random> plantRandoms = new List<Random>();
            plantRandoms.Add(new Random(grassSeed));
            plantRandoms.Add(new Random(flowerSeed));
            plantRandoms.Add(new Random(bushSeed));
            plantRandoms.Add(new Random(shroomSeed));
            plantRandoms.Add(new Random(waterPlantSeed));
            for (int i = 0; i < 3; i++)
            {
                Vector2 random2D = RandomPointInChunk(xCord, yCord, chunkRand);
                plantsGroup toAdd = GenerateGroup(chunkRand.Next(1, 5), random2D, plantRandoms);
                plantsD[(xCord, yCord)].Add(toAdd);
/*                GameObject thisTree = Instantiate(treePrefabs[treeRand.Next(0,4)], pos, Quaternion.identity);
                thisTree.transform.localScale = new Vector3(3f, 3f, 3f);
                treesD[(xCord, yCord)].Add(thisTree);*/

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
            foreach (plantsGroup plantsGroup in plantsD[(xCord, yCord)])
            {
                plantsGroup.removePlantsGroup();
            }
            plantsD.Remove((xCord, yCord));
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

    private plantsGroup GenerateGroup(int i, Vector2 center, List<Random> plantRandoms)
    {
        plantsGroup plants = new plantsGroup();
        /*      grass 0;
        flower 1;
        bush 2;
        shoorm 3;*/
        switch (i)
        {
            case 1:
                /*                GameObject temp1 = Instantiate(flowerPrefabs[plantRandoms[1].Next(0, 19)]);
                */
                generateSinglePlant(plants, center, new Vector2(0f, 0f), 1, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(-2f, -4f), 1, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(0f, 4f), 0, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(2f, -4f), 0, plantRandoms);

                break;
            case 2:
                generateSinglePlant(plants, center, new Vector2(0f, 0f), 2, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(-4f, -2f), 0, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(-4f, 2f), 0, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(4f, -2f), 0, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(4f, 2f), 0, plantRandoms);
                break;
            case 3:
                generateSinglePlant(plants, center, new Vector2(0f, 0f), 2, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(-2f, -4f), 1, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(-2f, 4f), 1, plantRandoms);
                break;
            case 4:
                generateSinglePlant(plants, center, new Vector2(0f, 0f), 0, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(4f, -4f), 0, plantRandoms);
                generateSinglePlant(plants, center, new Vector2(4f, 4f), 0, plantRandoms);
                generateSinglePlant(plants, center, new Vector2(-4f, -4f), 0, plantRandoms);
                generateSinglePlant(plants, center, new Vector2(-4f, 4f), 0, plantRandoms);
                
                break;
            case 5:
                generateSinglePlant(plants, center, new Vector2(0f, 0f), 2, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(-4f, -2f), 0, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(-4f, 2f), 0, plantRandoms);

                generateSinglePlant(plants, center, new Vector2(4f, 2f), 3, plantRandoms);
                break;
        }
        return plants;
    }

    private void generateSinglePlant(plantsGroup plants, Vector2 center, Vector2 relativeToCenter, int type, List<Random> plantRandoms)
    {
        /*      grass 0;
        flower 1;
        bush 2;
        shoorm 3;*/
        Vector2 pos2D = center + relativeToCenter;
        TerrainFunctions.TerrainPointData heightData = TerrainFunctions.GetTerrainPointData(pos2D);
        Vector3 pos = new Vector3(pos2D.x, heightData.height, pos2D.y);
        if (heightData.height < 30)
            type = 4;
        switch (type)
        {
            case 0:
                plants.gameObjects.Add(Instantiate(grassPrefabs[plantRandoms[0].Next(0, 39)], pos, Quaternion.identity));
                break;
            case 1:
                plants.gameObjects.Add(Instantiate(flowerPrefabs[plantRandoms[1].Next(0, 19)], pos, Quaternion.identity));
                break;
            case 2:
                plants.gameObjects.Add(Instantiate(bushPrefabs[plantRandoms[2].Next(0, 9)], pos, Quaternion.identity));
                break;
            case 3:
                plants.gameObjects.Add(Instantiate(mushroomPrefabs[plantRandoms[3].Next(0, 32)], pos, Quaternion.identity));
                break;
            case 4:
                plants.gameObjects.Add(Instantiate(waterPlantPrefabs[plantRandoms[4].Next(0, 11)], pos, Quaternion.identity));
                break;
        }


        
    }
}
