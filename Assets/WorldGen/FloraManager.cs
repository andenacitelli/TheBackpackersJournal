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

        /* 
        // Generates points (absolute)
        public static void GeneratePlant(Bounds bounds, Transform parent, string plant)
        {


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
        */

        public static List<Vector3> GetSpawnPoints(Bounds bounds)
        {
            const int NUM_ROWS = 15, NUM_COLS = 15;
            float HORIZ_PADDING = .5f, VERT_PADDING = .5f;
            float cellWidth = Mathf.RoundToInt(ChunkGen.size / NUM_COLS);
            float cellHeight = Mathf.RoundToInt(ChunkGen.size / NUM_ROWS);
            HashSet<Vertex> vertices = PointGeneration.generatePointsGrid(bounds, NUM_ROWS, NUM_COLS, HORIZ_PADDING, VERT_PADDING);
            List<Vector3> spawnPoints = new List<Vector3>();
            foreach (Vertex vertex in vertices)
            {
                Vector2 pos = new Vector2((float)vertex.x, (float)vertex.y);
                float height = TerrainFunctions.GetTerrainPointData(pos).height;
                spawnPoints.Add(new Vector3(pos.x, height, pos.y));
            }
            return spawnPoints;
        }

        public static IEnumerator GenerateFlora(Bounds bounds, Transform parent)
        {
            // 1. Get a list of points in this chunk at which we'd like to spawn something
            List<Vector3> spawnPoints = GetSpawnPoints(bounds);

            // 2. For each point, determine its biome, then spawn a random prefab from that biome's list of prefabs
            foreach (Vector3 spawnPoint in spawnPoints)
            {
                // 1. Get the biome at this point
                Biome biome = Biomes.GetBiomeAtPoint(spawnPoint);

                // Don't spawn stuff at / near water (water is at 26, but the shader makes it go a little up/down, hence the wiggle room)
                // TODO: Shouldn't be hardcoded
                if (spawnPoint.y < 28 && !biome.name.Equals("beach")) continue;

                // 2. Get a random prefab from that biome's list of prefabs
                Object prefab = GetRandomPrefabOfType(biome.plantTypes[Random.Range(0, biome.plantTypes.Length)]);

                // 3. Instantiate the prefab at this point
                GameObject bush = (GameObject)Instantiate(prefab, spawnPoint, Quaternion.identity, parent);

                // 4. Set rotation to be perpendicular to the normal vector of the ground
                // TODO: Should probably be set to some middle ground between "up" and the normal vector, as things don't actually grow at precisely the normal vector IRL
                // Note that Unity will let you pass in a Vector3 when a Vector2 is the correct parameter type; 
                // However, it will set Vector2(x, y) to Vector3(x, y, 0) rather than the Vector3(x, 0, y) we want
                // ...was a "fun" bug to fix
                Vector2 compressed = new Vector2(spawnPoint.x, spawnPoint.z);
                TerrainFunctions.TerrainPointData terrainPointData = TerrainFunctions.GetTerrainPointData(compressed);
                if (!terrainPointData.isHit)
                {
                    throw new System.Exception(string.Format("GenerateFlora ordered a raycast {0} to hit the terrain, but it didn't hit anything!", spawnPoint));
                }
                bush.transform.up = terrainPointData.normal;

                // 5. Spin the prefab randomly around its y axis (helps world look less uniform)
                bush.transform.Rotate(bush.transform.up, Random.Range(0, 360));
            }
            yield return null;
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
            floraPrefabs.Add("Reeds", Resources.LoadAll(VegetationBasePath + "/Plants/Reeds", typeof(Object)));

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
            try
            {
                return floraPrefabs[floraType][Random.Range(0, floraPrefabs[floraType].Length)];
            }
            catch (System.Exception e)
            {
                Debug.LogError("Could not find prefab of type " + floraType);
                throw e;
            }
        }
    }
}