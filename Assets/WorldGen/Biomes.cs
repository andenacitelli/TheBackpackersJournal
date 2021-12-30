using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.WorldGen
{
    public class Biome
    {
        public string name; // Mostly just makes things more readable from the editor
        public float lowMoisture, highMoisture;
        public float lowHeight, highHeight;
        public Color color;
        public float randomizationFactor;
        public string[] plantTypes;
    }

    public class Biomes : MonoBehaviour
    {
        public static Biome[] biomes;

        // Helper method that returns which color should be used for a given vertex height (technically a noise value, but it's basically the same thing)
        public static Biome ChooseBiome(float height, float moisture)
        {
            foreach (Biome biome in biomes)
            {
                if (height >= biome.lowHeight && height < biome.highHeight &&
                    moisture >= biome.lowMoisture && moisture < biome.highMoisture)
                {
                    return biome;
                }
            }

            // If we didn't hit one, print an error message and assign the last biome as a default
            Debug.LogError(string.Format("Height {0} and Moisture {1} did not fit into a biome's description.", height, moisture));
            return biomes[biomes.Length - 1];
        }
    }
}