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
        public int r, g, b;
        public float randomizationFactor;
        public string[] plantTypes;

        public static Biome CreateFromJSON(string jsonString)
        {
            Biome biome = JsonUtility.FromJson<Biome>(jsonString);
            biome.color = new Color(biome.r / 255f, biome.g / 255f, biome.b / 255f);
            return biome;
        }
    }

    public class Biomes : MonoBehaviour
    {
        public static Dictionary<string, Biome> biomes = new Dictionary<string, Biome>();

        void Awake()
        {
            // Read data from all files in 'biomes' folder of resources
            TextAsset[] files = Resources.LoadAll<TextAsset>("Biomes");
            foreach (TextAsset file in files)
            {
                Biome biome = Biome.CreateFromJSON(file.text);
                biomes.Add(biome.name, biome);

                /* 
                print("Added biome: " + biome.name);
                print("lowMoisture: " + biome.lowMoisture);
                print("highMoisture: " + biome.highMoisture);
                print("lowHeight: " + biome.lowHeight);
                print("highHeight: " + biome.highHeight);
                print("color: " + biome.color);
                print("randomizationFactor: " + biome.randomizationFactor);
                print("plantTypes: " + biome.plantTypes);
                */
            }
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
            // Iterate through keys of 'biomes'
            foreach (Biome biome in biomes.Values)
            {
                // If the height and moisture values are within the range of this biome's height and moisture values, return this biome
                if (height >= biome.lowHeight && height <= biome.highHeight && moisture >= biome.lowMoisture && moisture <= biome.highMoisture)
                {
                    return biome;
                }
            }
            throw new System.Exception(string.Format("No valid biome found with height {0} and moisture {1}!", height, moisture));
        }
    }
}