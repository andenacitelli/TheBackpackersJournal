using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Modifying the default Perlin noise generation attributes to make our noise generation more realistic 
[System.Serializable]
public class Wave
{
    public float seed;
    public float frequency;
    public float amplitude; 
}

public class Noise : MonoBehaviour
{
    // Given a List of Vector2 objects, returns a same-ordered list of corresponding noise values.
    public List<float> GenerateNoiseMap(List<Vector2> localPoints, float scale, float offsetX, float offsetZ, Wave[] waves)
    {
        List<float> noises = new List<float>();
        foreach (Vector2 point in localPoints)
        {
            float sampleX = (point.x + offsetX) / scale;
            float sampleZ = (point.y + offsetZ) / scale;

            float noise = 0f;
            float normalization = 0f;
            foreach (Wave wave in waves)
            {
                // Mathf.PerlinNoise can return slightly above or below 1.0; this is alright, and clamping the behavior will actually give
                // really unnatural-looking terrain (sharply cut off mountaintops, extremely flat seabeds)
                noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + wave.seed, sampleZ * wave.frequency + wave.seed);
                normalization += wave.amplitude;
            }

            // Normalize to [0, 1] by saving the amplification we did 
            noise /= normalization;
            noises.Add(noise);
        }
                
        return noises;
    }
}