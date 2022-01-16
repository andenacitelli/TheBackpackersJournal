using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Noise is constructed by a combination of waves
public class Wave
{
    public float seed;
    public float frequency;
    public float amplitude;
}

public class Noise : MonoBehaviour
{
    public Wave[] waves;
    public float scale;

    private const float DEFAULT_SCALE = 100;

    // Get a random set of waves that will perform well
    // If the implementer wants a non-random set of waves, they can modify this.waves after constructing
    private Wave[] GenWaves()
    {
        Wave[] waves = new Wave[4];
        waves[0] = new Wave();
        waves[1] = new Wave();
        waves[2] = new Wave();
        waves[3] = new Wave();

        // Randomize Seeds
        waves[0].seed = Random.Range(0, 10000);
        waves[1].seed = Random.Range(0, 10000);
        waves[2].seed = Random.Range(0, 10000);
        waves[3].seed = Random.Range(0, 10000);

        // Essentially simulating octaves, which are weaker, but more detailed layers added on top of each other
        waves[0].amplitude = 4;
        waves[1].amplitude = 2;
        waves[2].amplitude = 2;
        waves[3].amplitude = 1;
        waves[0].frequency = 1;
        waves[1].frequency = 2;
        waves[2].frequency = 4;
        waves[3].frequency = 6;

        return waves;
    }

    void Awake()
    {
        this.waves = GenWaves();
        this.scale = DEFAULT_SCALE; // Arbitrary number modifiable by the callee after constructor
    }

    // Main interface function for noise, allowing you to get the value of this Noise at a given (*world space*) x,z coord
    public float GetNoiseAtPoint(float x, float z)
    {
        float sampleX = x / scale;
        float sampleZ = z / scale;

        float noise = 0f;
        float normalization = 0f;
        foreach (Wave wave in waves)
        {
            noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency + wave.seed, sampleZ * wave.frequency + wave.seed);
            normalization += wave.amplitude;
        }

        // Normalize to [0, 1] by saving the amplification we did 
        noise /= normalization;
        if (noise < .5f && noise > .25f) noise = noise - (.5f - noise);
        if (noise > .5f && noise < .75f) noise = noise + (noise - .5f);
        return noise;
    }

    // ADVISED NOT TO USE THIS - An old version just kept for usage with heightmap generation, and thus doesn't really follow good design patterns
    public List<float> GenerateNoiseMap(List<Vector2> localPoints, float offsetX, float offsetZ)
    {
        List<float> noises = new List<float>();
        foreach (Vector2 point in localPoints)
        {
            float sampleX = (offsetX + point.x) / scale;
            float sampleZ = (offsetZ + point.y) / scale;

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