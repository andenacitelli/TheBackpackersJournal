using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;

/* Class that provides a few different ways to generate points within a range, often with some degree of control
    * past strict randomization */
public class PointGeneration : MonoBehaviour
{
    // Randomly generates a Triangle.NET polygon with NUM_POINTS random points distributed within the provided bounds
    public static HashSet<Vertex> generatePointsRandom(int NUM_POINTS, Bounds bounds)
    {
        HashSet<Vertex> output = new HashSet<Vertex>();
        for (int i = 0; i < NUM_POINTS; i++)
        {
            // Generate random vertices within chunk boundaries
            output.Add(new Vertex(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.z, bounds.max.z)));
        }
        return output;
    }

    // Create a grid from the provided bounds and dimensions, then returns a Polygon with one random point generated per
    public static HashSet<Vertex> generatePointsGrid(Bounds bounds, int numRows, int numColumns, float horizPadding, float vertPadding)
    {
        /* Parameter Meanings:
            Bounds: Represents the overall area that the grid should be inside
            Rows: Number of rows in the grid we generate
            Columns: Number of columns in the grid we generate (note that this function does not assume square bounds) 
            horiz/vertPadding: The amount of padding (in meters) to apply to each side of each cell. 
                Used so that points don't generate on cell boundaries and then potentially end up really close to other vertices.
                Larger values result in more consistent and uniform polygons and consistent edge sizes. Small values result in more varied, often more extreme polygons.
        */

        // Useful Constants
        float width = bounds.max.x - bounds.min.x, height = bounds.max.z - bounds.min.z;
        float cellHeight = height / numRows, cellWidth = width / numColumns;

        // Assert: 2 * each padding is less than overall cell width/height
        if (2 * horizPadding > cellWidth) throw new System.Exception("ERROR: generatePointsGrid() called with invalid horizontal padding. Ensure horizPadding <= cellWidth.");
        if (2 * vertPadding > cellHeight) throw new System.Exception("ERROR: generatePointsGrid() called with invalid vertical padding. Ensure vertPadding <= cellHeight.");

        // Iterate through each grid cell, randomly selecting points within bounds
        HashSet<Vertex> output = new HashSet<Vertex>();
        for (int row = 0; row < height / cellHeight; row++)
        {
            for (int col = 0; col < width / cellWidth; col++)
            {
                // <Offset to get from world origin to chunk space> + <offset to get to right cell> + <random range within cell> 
                float y = bounds.min.z + (row * cellHeight) + Random.Range(0 + vertPadding, cellHeight - vertPadding);
                float x = bounds.min.x + (col * cellWidth) + Random.Range(0 + horizPadding, cellWidth - horizPadding);
                output.Add(new Vertex(x, y));
            }
        }
        return output;
    }

    // Generates a Triangle.NET polygon with NUM_POINTS *mostly* random points; with Poisson disk sampling,
    // some care is taken such that they don't end up too close or too far from each other, resulting in roughly
    // similarly spaced polygons
    public static HashSet<Vertex> generatePointsPoissonDiscSampling(int NUM_POINTS, Bounds bounds, float radius)
    {
        HashSet<Vertex> output = new HashSet<Vertex>();
        PoissonDiscSampler sampler = new PoissonDiscSampler(bounds.max.x - bounds.min.x, bounds.max.z - bounds.min.z, radius);

        int i = 0;
        foreach (Vector2 sample in sampler.Samples()) // This is a generator function, so we have to handle it a little weirdly 
        {
            i++;
            output.Add(new Vertex(
                bounds.min.x + sample.x,
                bounds.min.z + sample.y));

            if (i >= NUM_POINTS)
            {
                break;
            }
        }
        return output;
    }
}