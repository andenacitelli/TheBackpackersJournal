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

        public static List<Vector3> GetSpawnPoints(Bounds bounds, float frequency)
        {
            List<Vector3> spawnPoints = new List<Vector3>();
            HashSet<Vertex> vertices = PointGeneration.GeneratePointsRandom(Mathf.RoundToInt(Random.Range(.6f, 1.2f) * 50 * frequency), bounds);
            foreach (Vertex vertex in vertices)
            {
                Vector2 pos = new Vector2((float)vertex.x, (float)vertex.y);
                float height = TerrainFunctions.GetTerrainPointData(pos).height;
                spawnPoints.Add(new Vector3(pos.x, height, pos.y));
            }
            return spawnPoints;
        }

        public static HashSet<Biome> GetBiomesInChunk(Bounds bounds)
        {
            HashSet<Biome> biomes = new HashSet<Biome>();
            const float SAMPLING_INTERVAL = 1;
            for (float x = bounds.min.x; x < bounds.max.x; x += SAMPLING_INTERVAL)
            {
                for (float z = bounds.min.z; z < bounds.max.z; z += SAMPLING_INTERVAL)
                {
                    Biome biome = Biomes.GetBiomeAtPoint(new Vector3(x, 0, z));
                    if (!biomes.Contains(biome))
                    {
                        biomes.Add(biome);
                    }
                }
            }
            return biomes;
        }

        public static IEnumerator GenerateFlora(Bounds bounds, Transform parent)
        {
            // Do a rough sampling of the chunk 
            HashSet<Biome> biomes = GetBiomesInChunk(bounds);

            // For each biome found in this chunk, get spawn points of every associated 
            // bit of flora, trimming any that aren't that biome 
            foreach (Biome biome in biomes)
            {
                foreach (PlantInfo plant in biome.plantTypes)
                {
                    // Get the spawn points for this plant type, trimming to only the ones in this biome
                    List<Vector3> spawnPoints = GetSpawnPoints(bounds, plant.frequency);
                    spawnPoints = spawnPoints.FindAll(point => Biomes.GetBiomeAtPoint(point) == biome);
                    foreach (Vector3 spawnPoint in spawnPoints)
                    {
                        TerrainFunctions.TerrainPointData terrainPointData = TerrainFunctions.GetTerrainPointData(new Vector2(spawnPoint.x, spawnPoint.z));
                        if (!terrainPointData.isHit)
                        {
                            throw new System.Exception(string.Format("GenerateFlora ordered a raycast {0} to hit the terrain, but it didn't hit anything!", spawnPoint));
                        }

                        // If terrain is too steep and this is a type of tree, skip it
                        // ~roughly equivalent to "no more than 30deg tilt" but the math is probably off b/c I think the normal is a normalized vector
                        // TODO: Make it so only bigger plants are affected by this (e.g. trees)
                        if (terrainPointData.normal.y < 0.9f && plant.name.Contains("Tree"))
                        {
                            continue;
                        }

                        // Prevent stuff other than reeds from spawning in the water
                        if (terrainPointData.height < 28 && !plant.name.Contains("Reeds"))
                        {
                            continue;
                        }

                        // Handle bunched flora spawns 
                        if (plant.bunchChance > 0 && Random.Range(0f, 1f) < plant.bunchChance)
                        {
                            int bunchSize = Random.Range(plant.minBunchSize, plant.maxBunchSize + 1);
                            for (int i = 0; i < bunchSize; i++)
                            {
                                Object prefab = GetRandomPrefabOfType(plant.name);
                                Vector3 position = new Vector3(Random.Range(spawnPoint.x - plant.bunchRadius, spawnPoint.x + plant.bunchRadius),
                                                                0,
                                                                Random.Range(spawnPoint.z - plant.bunchRadius, spawnPoint.z + plant.bunchRadius));
                                position.y = TerrainFunctions.GetTerrainPointData(new Vector2(position.x, position.z)).height;
                                GameObject go = (GameObject)Instantiate(prefab, position, Quaternion.identity, parent);
                                go.transform.Rotate(go.transform.up, Random.Range(0, 360)); // Vary direction it's facing
                                go.transform.up = terrainPointData.normal; // Make normal to ground 
                                go.transform.localScale = new Vector3(Random.Range(.7f, 1.5f), Random.Range(.7f, 1.5f), Random.Range(.7f, 1.5f)); // Vary size
                            }
                        }

                        // Otherwise, just spawn a single one 
                        else
                        {
                            Vector3 pointWithHeight = new Vector3(spawnPoint.x, terrainPointData.height, spawnPoint.z);
                            Object prefab = GetRandomPrefabOfType(plant.name);
                            GameObject go = (GameObject)Instantiate(prefab, pointWithHeight, Quaternion.identity, parent);
                            go.transform.Rotate(go.transform.up, Random.Range(0, 360)); // Vary direction it's facing
                            go.transform.up = terrainPointData.normal; // Make normal to ground 
                            go.transform.localScale = new Vector3(Random.Range(.7f, 1.5f), Random.Range(.7f, 1.5f), Random.Range(.7f, 1.5f)); // Vary size
                        }
                    }
                }
                yield return null;
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