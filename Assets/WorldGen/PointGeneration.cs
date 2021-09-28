using System.Collections;
using UnityEngine;
using TriangleNet.Geometry;

/* Class that provides a few different ways to generate points within a range, often with some degree of control
    * past strict randomization */
public class PointGeneration
{
    // Randomly generates a Triangle.NET polygon with NUM_POINTS random points distributed within the provided bounds
    public static Polygon generatePointsRandom(int NUM_POINTS, Bounds bounds)
    {
        Polygon polygon = new Polygon();
        for (int i = 0; i < NUM_POINTS; i++)
        {
            // Generate random vertices within chunk boundaries
            polygon.Add(new Vertex(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.z, bounds.max.z)));
        }
        return polygon;
    }

    // Generates a Triangle.NET polygon with NUM_POINTS *mostly* random points; with Poisson disk sampling,
    // some care is taken such that they don't end up too close or too far from each other, resulting in roughly
    // similarly spaced polygons
    public static Polygon generatePointsPoissonDiscSampling(int NUM_POINTS, Bounds bounds, float radius)
    {
        Polygon polygon = new Polygon();
        PoissonDiscSampler sampler = new PoissonDiscSampler(bounds.max.x - bounds.min.x, bounds.max.z - bounds.min.z, radius);

        int i = 0;
        foreach (Vector2 sample in sampler.Samples()) // This is a generator function, so we have to handle it a little weirdly 
        {
            i++;
            polygon.Add(new Vertex(
                bounds.min.x + sample.x,
                bounds.min.z + sample.y));

            if (i >= NUM_POINTS)
            {
                break;
            }
        }
        return polygon; 
    }
}