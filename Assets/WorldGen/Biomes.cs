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
        // TODO: Potential optimization based on being able to split on height lines (approx.) halves the work at the expense of memory 
        // TODO: Potential optimization by sorting biomes by height and/or moisture
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

            infrequent = new HashSet<Biome>(biomes.Values);
        }

        public static Biome GetBiomeAtPoint(Vector3 point)
        {
            // Get height and moisture for point 
            float height = TerrainManager.heightNoise.GetNoiseAtPoint(point.x, point.z);
            float moisture = TerrainManager.moistureNoise.GetNoiseAtPoint(point.x, point.z);

            // Return result of ChooseBiome call 
            return ChooseBiome(height, moisture);
        }

        // TODO: Probably doable to keep a few of the most recent biomes returned, but this is simpler and represents the majority of cost savings
        static Biome lastBiome = null;

        // Convert dictionary values to hash set
        static HashSet<Biome> infrequent;
        public static Biome ChooseBiome(float height, float moisture)
        {
            bool FitsBiome(Biome biome, float height, float moisture)
            {
                return biome.minHeight <= height && biome.maxHeight >= height &&
                    biome.minMoisture <= moisture && biome.maxMoisture >= moisture;
            }

            // As sequential calls to ChooseBiome are more likely than not to be nearby, we cache the last result and check that first
            if (lastBiome != null && FitsBiome(lastBiome, height, moisture)) return lastBiome;

            // Otherwise, do (relatively) expensive linear search on the rest
            Biome output = null;
            foreach (Biome biome in infrequent)
            {
                if (FitsBiome(biome, height, moisture))
                {
                    output = biome;
                    break;
                }
            }

            if (output == null) throw new System.Exception(string.Format("No valid biome found with height {0} and moisture {1}!", height, moisture));

            // Seeing as cached version wasn't correct, this becomes new cache
            if (lastBiome == null)
            {
                infrequent.Remove(output);
                lastBiome = output;
            }
            else
            {
                infrequent.Add(lastBiome);
                infrequent.Remove(output);
                lastBiome = output;
            }

            return output;
        }
    }
}