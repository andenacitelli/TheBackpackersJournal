using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.WorldGen
{
    public class PlantInfo
    {
        public string name;
        public float frequency; // 1 => Everywhere, .7 = Common, .5 = Sparse, .3 = Rare, .1 = Very Rare
        public float bunchChance;
        public int minBunchSize;
        public int maxBunchSize;
        public float bunchRadius;
    }

    public class Biome
    {
        public string name;
        public float minHeight, maxHeight;
        public float minMoisture, maxMoisture;
        public float randomizationFactor;
        public Color color;
        public List<PlantInfo> plantTypes;
    }

    public class Biomes : MonoBehaviour
    {
        public static Dictionary<string, Biome> biomes = new Dictionary<string, Biome>();

        void Awake()
        {
            biomes.Add("beach", new Beach());
            biomes.Add("coniferous", new Coniferous());
            biomes.Add("deciduous", new Deciduous());
            biomes.Add("desert", new Desert());
            biomes.Add("halloween", new Halloween());
            biomes.Add("Mountain", new Mountain());
            biomes.Add("Tundra", new Tundra());
            biomes.Add("Wetlands", new Wetlands());
        }

        public static Biome GetBiomeAtPoint(Vector3 point)
        {
            // Get height and moisture for point 
            float height = TerrainManager.heightNoise.GetNoiseAtPoint(point.x, point.z);
            float moisture = TerrainManager.moistureNoise.GetNoiseAtPoint(point.x, point.z);

            // Return result of ChooseBiome call 
            return ChooseBiome(height, moisture);
        }

        // Helper method that returns which color should be used for a given vertex height (technically a noise value, but it's basically the same thing)
        public static Biome ChooseBiome(float height, float moisture)
        {
            foreach (Biome biome in biomes.Values)
            {
                if (height >= biome.minHeight && height <= biome.maxHeight && moisture >= biome.minMoisture && moisture <= biome.maxMoisture)
                {
                    return biome;
                }
            }
            throw new System.Exception(string.Format("No valid biome found with height {0} and moisture {1}!", height, moisture));
        }
    }
}