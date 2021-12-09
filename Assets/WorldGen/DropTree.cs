using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.WorldGen;
using Terrain = Assets.WorldGen;
using System;
using Random = System.Random;



public class DropTree : MonoBehaviour
{

    public TerrainManager terrainManager;

    static private Dictionary<(int, int), List<GameObject>> treesD = new Dictionary<(int, int), List<GameObject>>();
    static private List<(int, int)> currentChunks;
    static private List<(int, int)> toDoChunks;
    static private List<(int, int)> toRemoveChunks;
    static private List<(int, int)> finishedChunks;
    static private List<(int, int)> touchedChunks;
    static private Noise treeNoise;
    static private Noise mushroomNoise;
    static private Noise grassNoise;
    static private List<Random> plantRandoms;
    static private Random chunkRand;
    static private (int, int) gridCount;
    public  int GRID_TO_GENERATE_PER_FRAME;
    public int PLANTS_TO_REMOVE_PER_FRAME;

    static private GameObject player;
    static private Vector3 playerPos;
    static private (int, int) currentChunk;
    static private GFG gg;
    public List<GameObject> bushPrefabs;
    public List<GameObject> flowerPrefabs;
    public List<GameObject> grassPrefabs;
    public List<GameObject> mushroomPrefabs;
    public List<GameObject> waterPlantPrefabs;
    public GameObject rock1prefab, rock2prefab, rock3prefab;
    public List<GameObject> treePrefabs;

    private class GFG : IComparer<(int, int)>
    {
        public int distance((int, int) x)
        {
            int output;
            if (Mathf.Abs(x.Item1 - currentChunk.Item1) > Mathf.Abs(x.Item2 - currentChunk.Item2))
            {
                output = Mathf.Abs(x.Item1 - currentChunk.Item1);
            }
            else
            {
                output = Mathf.Abs(x.Item2 - currentChunk.Item2);
            }
            return output;
        }

        public int Compare((int, int) x, (int, int) y)
        {
            return distance(x).CompareTo(distance(y));

        }
    }
    // Start is called before the first frame update
    void Start()
    {
        treeNoise = gameObject.AddComponent<Noise>();
        mushroomNoise = gameObject.AddComponent<Noise>();
        grassNoise = gameObject.AddComponent<Noise>();
        currentChunks = TerrainManager.getChunksCords();
        currentChunks = new List<(int int1, int int2)>();
        toDoChunks = currentChunks;
        finishedChunks = new List<(int int1, int int2)>();
        toRemoveChunks = new List<(int int1, int int2)>();
        touchedChunks = new List<(int int1, int int2)>();
        player = GameObject.Find("Player");
        gg = new GFG();
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
        treesD = new Dictionary<(int, int), List<GameObject>>();

        /*GENERAL NOTES ON DESIGN of foliage:
         * -----> NEEDS FIXES <------
         * 
         * On testing, this method of PCG is not producing content on new chunk generation.
         * That is, the plants don't keep going forever. I think a big cause of  this issue is that 
         * the vegation isnt being instantiated as children of the chunks they belong to. I'm not entirely
         * sure how counter-intuitive it is, just my thoughts.
         * 
         */
/*        Debug.Log("DropTree: OnDisable Ran");*/
    }
    // Update is called once per frame
    void Update()
    {
/*        adding a line to refresh git
*/        player = GameObject.Find("Player");
        if (player == null)
        {
            playerPos = Vector3.zero;
            currentChunk = (0,0);
        }
        else
        {
            playerPos = player.transform.position;
            currentChunk = ((int)((playerPos.x - 40) / 80), (int)((playerPos.z - 40) / 80));
        }
        currentChunks = TerrainManager.getChunksCords();

        toDoChunks = new List<(int, int)>();
        foreach ((int, int) chunk in currentChunks)
        {
            if (!finishedChunks.Contains(chunk))
            {
                toDoChunks.Add(chunk);
            }
        }
        toDoChunks.Sort(gg);
        GenerateTrees(); // Generate trees in in-range chunks
        toRemoveChunks = new List<(int, int)>();
        foreach ((int, int) chunk in touchedChunks)
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
        int gridGeneratedThisFrame = 0;
        
        if (toDoChunks.Count != 0)
        {            
            int xCord = toDoChunks[0].Item1;
            int yCord = toDoChunks[0].Item2;
            string chunkCord = "(" + xCord + ", " + yCord + ")";
            int seed = chunkCord.GetHashCode();
            int grassSeed = (chunkCord + "grass").GetHashCode();
            int flowerSeed = (chunkCord + "flower").GetHashCode();
            int bushSeed = (chunkCord + "bush").GetHashCode();
            int shroomSeed = (chunkCord + "shroom").GetHashCode();
            int waterPlantSeed = (chunkCord + "waterPlant").GetHashCode();
            int treeSeed = (chunkCord + "tree").GetHashCode();
            //If this is the first time GenerateTrees() is been used to generate plants for chunk:(xCord, yCord), we need to initial a pair in treesD and all the Random generator
            if (!treesD.ContainsKey((xCord, yCord)))
            {
                gridCount = (0,0);
                treesD.Add((xCord, yCord), new List<GameObject>());
                chunkRand = new Random(seed);
                plantRandoms = new List<Random>();
                plantRandoms.Add(new Random(grassSeed));
                plantRandoms.Add(new Random(flowerSeed));
                plantRandoms.Add(new Random(bushSeed));
                plantRandoms.Add(new Random(shroomSeed));
                plantRandoms.Add(new Random(waterPlantSeed));
                plantRandoms.Add(new Random(treeSeed));
                touchedChunks.Add((xCord,yCord));
            }

            for (int i = gridCount.Item1; i < 80; i++)
            {
                float tempX = xCord * 80f - 40f + i;
                for (int j = gridCount.Item2; j < 80; j++)
                {
                    float tempZ = yCord * 80f - 40f + j;
                    //Generate Tree and Shrooms
                    if (tempX % 2 == 0f && tempZ % 2 == 0f)
                    {
                        //GenerateTree
                        if (treeNoise.GetNoiseAtPoint(tempX, tempZ) > 0.7)
                        {
                            if (chunkRand.Next(1, 10) < 5)
                            {
                                float xFix = (float)chunkRand.NextDouble() - 0.5f;
                                float zFix = (float)chunkRand.NextDouble() - 0.5f;
                                float treeX = tempX + xFix;
                                float treeZ = tempZ + zFix;
                                TerrainFunctions.TerrainPointData heightData = TerrainFunctions.GetTerrainPointData(new Vector2(treeX, treeZ));
                                Vector3 pos = new Vector3(treeX, heightData.height, treeZ);
                                if (heightData.height >= 30)
                                {                                    
                                    GameObject tempTree = Instantiate(treePrefabs[plantRandoms[5].Next(0, 6)], pos, Quaternion.identity);
                                    tempTree.transform.parent = this.transform;
                                    tempTree.transform.up = heightData.normal;
                                    treesD[(xCord, yCord)].Add(tempTree);
                                }
                                else
                                {
                                    generateWaterPlant(pos, (xCord, yCord), plantRandoms);
                                }                                
                            }
                        }
                        //GenerateMushroom
                        if (mushroomNoise.GetNoiseAtPoint(tempX, tempZ) > 0.7)
                        {
                            if (chunkRand.Next(1, 10) < 5)
                            {
                                float xFix = (float)chunkRand.NextDouble() - 0.5f;
                                float zFix = (float)chunkRand.NextDouble() - 0.5f;
                                float shroomX = tempX + xFix;
                                float shroomZ = tempZ + zFix;
                                TerrainFunctions.TerrainPointData heightData = TerrainFunctions.GetTerrainPointData(new Vector2(shroomX, shroomZ));
                                Vector3 pos = new Vector3(shroomX, heightData.height, shroomZ);
                                if (heightData.height >= 30)
                                {
                                    GameObject tempTree = Instantiate(mushroomPrefabs[plantRandoms[3].Next(0, 67)], pos, Quaternion.identity);
                                    tempTree.transform.parent = this.transform;
                                    tempTree.transform.up = heightData.normal;
                                    treesD[(xCord, yCord)].Add(tempTree);
                                }
                                else
                                {
                                    generateWaterPlant(pos, (xCord, yCord), plantRandoms);
                                }
                            }
                        }
                    }
                    //Generate Grass flower, bush

                    if (grassNoise.GetNoiseAtPoint(tempX, tempZ) > 0.6)
                    {
                        float xFix = (float)chunkRand.NextDouble() - 0.5f;
                        float ZFix = (float)chunkRand.NextDouble() - 0.5f;
                        float grassX = tempX + xFix;
                        float grassZ = tempZ + ZFix;
                        TerrainFunctions.TerrainPointData heightData = TerrainFunctions.GetTerrainPointData(new Vector2(grassX, grassZ));
                        Vector3 pos = new Vector3(grassX, heightData.height, grassZ);
                        if (heightData.height >= 30)
                        {
                            GameObject prefab = grassPrefabs[plantRandoms[0].Next(0, 51)];
                            float tempRand = (float)chunkRand.NextDouble();
                            if (tempRand < 0.01)
                            {
                                prefab = bushPrefabs[plantRandoms[2].Next(0, 9)];
                            }
                            else if (tempRand < 0.55)
                            {
                                prefab = flowerPrefabs[plantRandoms[1].Next(0, 19)];
                            }else if (tempRand > 0.99)
                            {
                                prefab = rock2prefab;
                            }
                            GameObject tempTree = Instantiate(prefab, pos, Quaternion.identity);
                            tempTree.transform.parent = this.transform;
                            tempTree.transform.up = heightData.normal;
                            treesD[(xCord, yCord)].Add(tempTree);
                        }
                        else
                        {
                            generateWaterPlant(pos, (xCord, yCord), plantRandoms);
                        }
                    }
                    gridGeneratedThisFrame++;
                    gridCount.Item2++;
                    if (j == 79)
                    {
                        int temp = gridCount.Item1;
                        gridCount = (temp + 1, 0);
                    }
                    if (gridGeneratedThisFrame > GRID_TO_GENERATE_PER_FRAME)
                    {
                        return;
                    }
                }
            }

            finishedChunks.Add((xCord, yCord));
/*            Debug.Log("(" + xCord + ", " + yCord + ") Generation SpeedTrack");*/
        }
    }

    private void generateWaterPlant(Vector3 pos, (int xCord, int yCord) chunkCord, List<Random> plantRandoms)
    {
        GameObject tempWaterPlants = Instantiate(waterPlantPrefabs[plantRandoms[4].Next(0, 11)], pos, Quaternion.identity);
        tempWaterPlants.transform.parent = this.transform;
        treesD[chunkCord].Add(tempWaterPlants);
    }

    void RemoveTrees()
    {
        int plantsRemovedThisFrame = 0;
        if (toRemoveChunks.Count != 0)
        {

            int xCord = toRemoveChunks[0].Item1;
            int yCord = toRemoveChunks[0].Item2;
/*            Debug.Log("At lease im here" + "removing: " + xCord + "  " + yCord);*/
            for (int i = 0; i < treesD[(xCord, yCord)].Count; i++)
            {
                Destroy(treesD[(xCord, yCord)][i]);
                treesD[(xCord, yCord)].RemoveAt(i);
                plantsRemovedThisFrame++;
                if (plantsRemovedThisFrame > PLANTS_TO_REMOVE_PER_FRAME)
                {
/*                    Debug.Log("returned");*/
                    return;
                }
            }
            treesD.Remove((xCord, yCord));
            if (touchedChunks.Contains((xCord, yCord)))
            {
                touchedChunks.Remove((xCord, yCord));
            }
            if (finishedChunks.Contains((xCord, yCord)))
            {
                finishedChunks.Remove((xCord, yCord));
            }
            
/*            Debug.Log("(" + xCord + ", " + yCord + ") Remove SpeedTrack");
*/        }
    }
}
