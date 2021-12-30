using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;

namespace Assets.WorldGen
{
    public class FloraManager : MonoBehaviour
    {
        private Noise noise;
        private static FloraManager instance;

        // List of all Flora prefabs
        // A lot more scalable to just define file paths and dynamically load these each time, rather than manually drag them all over in the editor
        private static Dictionary<string, Object[]> floraPrefabs = new Dictionary<string, Object[]>();

        void Awake()
        {
            instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            // TestNoise();
            LoadPrefabs();
        }

        void TestNoise()
        {
            List<float> noises = new List<float>();
            noise = gameObject.AddComponent<Noise>();
            for (float i = -10000; i < 10000; i += 500)
            {
                for (float j = -10000; j < 10000; j += 500)
                {
                    // float noise = TerrainFunctions.GetTerrainPointData(new Vector2(i, j)).height;
                    noises.Add(gameObject.GetComponent<Noise>().GetNoiseAtPoint(i, j));
                    // print("noiseValue: " + noise.GetNoiseAtPoint(i, j));
                }
            }
            float[] buckets = new float[100];
            for (int i = 0; i < 10; i++) buckets[i] = 0;
            foreach (float noiseValue in noises)
            {
                buckets[Mathf.FloorToInt(noiseValue * 100)]++;
            }
            for (int i = 0; i < 100; i++)
            {
                print("bucket " + i + ": " + buckets[i]);
            }
        }

        // Generates points (absolute)
        public static void GeneratePlant(Bounds bounds, Transform parent, string plant)
        {
            // 1. Generate points
            const int NUM_ROWS = 10, NUM_COLS = 10;
            float HORIZ_PADDING = .5f, VERT_PADDING = .5f;
            float cellWidth = Mathf.RoundToInt(ChunkGen.size / NUM_COLS);
            float cellHeight = Mathf.RoundToInt(ChunkGen.size / NUM_ROWS);
            HashSet<Vertex> vertices = PointGeneration.generatePointsGrid(bounds, NUM_ROWS, NUM_COLS, HORIZ_PADDING, VERT_PADDING);

            // 2. Generate heightmap value for those points
            // 3. Actually instantiate grass at those points
            foreach (Vertex vertex in vertices)
            {
                Vector2 pos = new Vector2((float)vertex.x, (float)vertex.y);
                float absoluteX = bounds.center.x * ChunkGen.size + pos.x;
                float absoluteZ = bounds.center.y * ChunkGen.size + pos.y;
                float height = TerrainFunctions.GetTerrainPointData(pos).height;
                GameObject bush = Instantiate(
                    (GameObject)FloraManager.GetRandomPrefabOfType(plant),
                    new Vector3(pos.x, height, pos.y),
                    Quaternion.identity);
                bush.transform.parent = parent;
            }
        }

        public static IEnumerator GenerateFlora(Biome[] biomes, Bounds bounds, Transform parent)
        {
            // Probably best organizationally to keep a list of which plant types should be in each biome, 
            // and give each plant or plant type its own spawning function
            Biome biome = TerrainFunctions.ChooseBiome(
                    TerrainManager.heightNoise.GetNoiseAtPoint(bounds.center.x, bounds.center.z),
                    TerrainManager.moistureNoise.GetNoiseAtPoint(bounds.center.x, bounds.center.z));
            foreach (string plant in floraPrefabs.Keys)
            {
                // Iterate through keys of floraPrefabs
                if (System.Array.IndexOf(biome.plantTypes, plant) != -1)
                {
                    print("Biome " + biome.name + " has plant type " + plant);
                    GeneratePlant(bounds, parent, plant);
                }

                // Check one plant type per frame
                print("Checked one biome!");
                yield return null;

                /* Not yet implemented 
                if (System.Array.IndexOf(biome.plantTypes, "DeadBush") != -1)
                    yield return instance.StartCoroutine(GenerateDeadBush(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "FlowerBush") != -1)
                    yield return instance.StartCoroutine(GenerateFlowerBush(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "CactusNoBottoms") != -1)
                    yield return instance.StartCoroutine(GenerateCactusNoBottoms(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "CactusWithBottoms") != -1)
                    yield return instance.StartCoroutine(GenerateCactusWithBottoms(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "FlowersOneSided") != -1)
                    yield return instance.StartCoroutine(GenerateFlowersOneSided(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "FlowersTwoSided") != -1)
                    yield return instance.StartCoroutine(GenerateFlowersTwoSided(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "Grass3D") != -1)
                    yield return instance.StartCoroutine(GenerateGrass(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "GrassPlane") != -1)
                    yield return instance.StartCoroutine(GenerateGrassPlane(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "MeshGrass") != -1)
                    yield return instance.StartCoroutine(GenerateMeshGrass(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "Mushrooms") != -1)
                    yield return instance.StartCoroutine(GenerateMushrooms(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "OtherPlants") != -1)
                    yield return instance.StartCoroutine(GenerateOtherPlants(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "Reeds") != -1)
                    yield return instance.StartCoroutine(GenerateReeds(bounds, parent));

                if (System.Array.IndexOf(biome.plantTypes, "BirchTree") != -1)
                    yield return instance.StartCoroutine(GenerateBirchTree(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "BirchTreeLeafless") != -1)
                    yield return instance.StartCoroutine(GenerateBirchTreeLeafless(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "OakTree") != -1)
                    yield return instance.StartCoroutine(GenerateOakTree(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "PineTree") != -1)
                    yield return instance.StartCoroutine(GeneratePineTree(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "Rocks") != -1)
                    yield return instance.StartCoroutine(GenerateRocks(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "TreeLeafless") != -1)
                    yield return instance.StartCoroutine(GenerateTreeLeafless(bounds, parent));
                if (System.Array.IndexOf(biome.plantTypes, "TreeStump") != -1)
                    yield return instance.StartCoroutine(GenerateTreeStump(bounds, parent));
                */
            }
        }

        const string VegetationBasePath = "Low Poly Vegetation Pack/Vegetation Assets/Prefabs";
        const string TreeBasePath = "LowPolyTreePack/Prefabs";
        void LoadPrefabs()
        {
            // Low Poly Vegetation Pack
            floraPrefabs.Add("Bush", Resources.LoadAll(VegetationBasePath + "/Bushes/Bush", typeof(Object)));
            floraPrefabs.Add("DeadBush", Resources.LoadAll(VegetationBasePath + "/Bushes/DeadBush", typeof(Object)));
            floraPrefabs.Add("FlowerBush", Resources.LoadAll(VegetationBasePath + "/Bushes/FlowerBush", typeof(Object)));
            floraPrefabs.Add("CactusNoBottoms", Resources.LoadAll(VegetationBasePath + "/Cactus/No_Bottoms", typeof(Object)));
            floraPrefabs.Add("CactusWithBottoms", Resources.LoadAll(VegetationBasePath + "/Cactus/With_Bottoms", typeof(Object)));
            floraPrefabs.Add("FlowersOneSided", Resources.LoadAll(VegetationBasePath + "/Flowers/OneSided", typeof(Object)));
            floraPrefabs.Add("FlowersTwoSided", Resources.LoadAll(VegetationBasePath + "/Flowers/TwoSided", typeof(Object)));
            floraPrefabs.Add("Grass3D", Resources.LoadAll(VegetationBasePath + "/Grass/Grass3D", typeof(Object)));
            floraPrefabs.Add("GrassPlane", Resources.LoadAll(VegetationBasePath + "/Grass/GrassPlane", typeof(Object)));
            floraPrefabs.Add("MeshGrass", Resources.LoadAll(VegetationBasePath + "/Grass/MeshGrass", typeof(Object)));
            floraPrefabs.Add("Mushrooms", Resources.LoadAll(VegetationBasePath + "/Mushrooms", typeof(Object)));
            floraPrefabs.Add("OtherPlants", Resources.LoadAll(VegetationBasePath + "/Plants/Other_Plants", typeof(Object)));
            floraPrefabs.Add("Reeds", Resources.LoadAll(VegetationBasePath + "/Reeds", typeof(Object)));

            // Low Poly Tree Pack
            floraPrefabs.Add("BirchTree", Resources.LoadAll(TreeBasePath + "/BirchTree", typeof(Object)));
            floraPrefabs.Add("BirchTreeLeafless", Resources.LoadAll(TreeBasePath + "/BirchTreeLeafless", typeof(Object)));
            floraPrefabs.Add("OakTree", Resources.LoadAll(TreeBasePath + "/OakTree", typeof(Object)));
            floraPrefabs.Add("PineTree", Resources.LoadAll(TreeBasePath + "/PineTree", typeof(Object)));
            floraPrefabs.Add("Rocks", Resources.LoadAll(TreeBasePath + "/Rocks", typeof(Object)));
            floraPrefabs.Add("TreeLeafless", Resources.LoadAll(TreeBasePath + "/TreeLeafless", typeof(Object)));
            floraPrefabs.Add("TreeStump", Resources.LoadAll(TreeBasePath + "/TreeStump", typeof(Object)));
        }

        public static Object GetRandomPrefabOfType(string floraType)
        {
            return floraPrefabs[floraType][Random.Range(0, floraPrefabs[floraType].Length)];
        }
    }
}