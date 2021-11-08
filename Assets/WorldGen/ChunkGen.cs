using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using Assets.WorldGen;

// Selectively shade chunks 
[System.Serializable]
public class TerrainType {
    public float randomizationFactor; // Allows us to vary the amount we randomize colors a bit in each biome
	public string name;
	public float height;
	public Color color;
}

// Generate chunks 
public class ChunkGen : MonoBehaviour
{
    [SerializeField]
    static public float size = 80; // Length, in meters, of each chunk's edge
    private Bounds bounds = new Bounds(Vector3.zero, new Vector3(size, 0, size));
    Color gizmoColor;

    [SerializeField]
    private GameObject tilePrefab;
    private int tileWidth, tileDepth;

    // Edit different "biomes" from the editor 
    [SerializeField]
    private TerrainType[] terrainTypes;

    [SerializeField]
    Noise noise;

    [SerializeField]
    private Wave[] waves;

    [SerializeField]
    private MeshRenderer meshRenderer;

    [SerializeField]
    private MeshFilter meshFilter;

    [SerializeField]
    private MeshCollider meshCollider;

    private Noise heightNoise; // Used for heightmap values
    private Noise colorRandomizationNoise; // Used to randomize vertex colors a little bit

    // Holds the inner triangulation of this chunk in Triangle.NET's format
    private TriangleNet.Mesh mesh;

    // The mesh above ends up holding vertices in other chunks as well; this is a simple list of just the inner vertices
    int cellWidth, cellHeight; 
    private HashSet<Vertex> vertices = new HashSet<Vertex>(); 

    // Represents how much to spread the Perlin noise out; for a flatter, smoother map, make this value high
    [SerializeField]
    private float mapScale;

    // Set by the class Instanting the gameObject linked to this script
    public Vector2Int coords;

    /* So: 
     * On generating a given chunk, I need surrounding chunks to be already generated.
     * The current issue is chunks are getting added to TerrainManager's `chunks` after their Awake, but before any mesh
     * gets generated in Start().
     * 
     * Solutions:
     * - Either find a way to pass stuff into Awake, or reverse engineer chunk coords, then generate the mesh in Awake(), which happens when the object gets initialized
     * - Do something likely more complicated with async/awake where I wait for a chunk to finish generation before doing another
     * */

    // Need to generate the chunk immediately on initialization, otherwise the parallelism-type stuff with connective tissue gets really complicated
    private void Start()
    {
        // Rounding necessary otherwise, for example, chunk (1, 0) at real coords (40, 0) would get set as (0, 0) if floating precision 
        // reported it as very slightly below 40, if we were doing integer division/truncation
        coords = new Vector2Int(Mathf.RoundToInt(transform.position.x / size), Mathf.RoundToInt(transform.position.z / size));
        print("Initializing chunk at " + coords);
        //GenerateChunk();

        // For debug; needs to be a light enough color to show up against Unity's dark skybox 
        gizmoColor = new Color(Random.Range(.3f, 1), Random.Range(.3f, 1), Random.Range(.3f, 1));
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable - ChuckGen");
    }

    // Makes chunk direction-related code much more readable
    public enum Direction { UP, LEFT, RIGHT, DOWN }

    // Returns the border vertices of the current chunk
    public List<Vertex> GetBorderVertices(Direction direction)
    {
        List<Vertex> output = new List<Vertex>();
        foreach (Vertex v in vertices)
        {
            switch (direction)
            {
                case Direction.UP:
                    {
                        if (v.y <= bounds.max.z && Mathf.Abs(bounds.max.z - (float)v.y) <= cellHeight) output.Add(v);
                        break; 
                    }
                case Direction.LEFT:
                    {
                        if (v.x >= bounds.min.x && Mathf.Abs(bounds.min.x - (float)v.x) <= cellWidth) output.Add(v);
                        break;
                    }
                case Direction.RIGHT:
                    {
                        if (v.x <= bounds.max.x && Mathf.Abs(bounds.max.x - (float)v.x) <= cellWidth) output.Add(v);
                        break;
                    }
                case Direction.DOWN:
                    {
                        if (v.y >= bounds.min.z && Mathf.Abs(bounds.min.z - (float)v.y) <= cellHeight) output.Add(v);
                        break;
                    }
            }
        }
        return output; 
    }

    // Update vertex heights and apply height-based coloration
    private List<Vector2> Vector3ToVector2(Vector3[] vList)
    {
        List<Vector2> output = new List<Vector2>();
        foreach (Vector3 v in vList)
        {
            output.Add(new Vector2(v.x, v.z));
        }
        return output;
    }

    public void GenerateChunk()
    {
        // Seed this random generation off chunk coords so each chunk generates the same every time.
        Random.InitState(coords.GetHashCode());

        // Generate set of vertices to feed into triangulation
        const int NUM_ROWS = 20, NUM_COLS = 20;
        float HORIZ_PADDING = .5f, VERT_PADDING = .5f;
        cellWidth = Mathf.RoundToInt((bounds.max.x - bounds.min.x) / NUM_COLS);
        cellHeight = Mathf.RoundToInt((bounds.max.z - bounds.min.z) / NUM_ROWS);
        vertices = PointGeneration.generatePointsGrid(bounds, NUM_ROWS, NUM_COLS, HORIZ_PADDING, VERT_PADDING);

        /* 
        int NUM_POINTS_BASELINE = Mathf.RoundToInt((size / 4) * (size / 4)); 
        int NUM_POINTS = Mathf.RoundToInt(Random.Range(.9f * NUM_POINTS_BASELINE, 1.1f * NUM_POINTS_BASELINE));
        const float RADIUS = 6; 
        vertices = PointGeneration.generatePointsPoissonDiscSampling(NUM_POINTS, bounds, RADIUS);
        */

        // Turn points into a Triangle.NET polygon which we can use for triangulation
        Polygon polygon = new Polygon();

        /* 
        // TODO: Probably cleaner to turn this into a function\
        GameObject above = TerrainManager.GetChunkAtCoords(new Vector2Int(coords.x, coords.y + 1));
        if (above != null)
        {
            print("New chunk is generating connective tissue above itself!");
            List<Vertex> borderVertices = above.GetComponent<ChunkGen>().GetBorderVertices(Direction.DOWN);
            //foreach (Vertex v in borderVertices) vertices.Add(new Vertex(v.x, v.y + size)); // Need to adjust coords because the above chunk's vertices will still be local; we have to convert from local of the above chunk to local of current chunk
            vertices.Add(new Vertex(bounds.min.x, bounds.max.y));
            vertices.Add(new Vertex(bounds.max.x, bounds.max.y));
        }

        GameObject left = TerrainManager.GetChunkAtCoords(new Vector2Int(coords.x - 1, coords.y));
        if (left != null)
        {
            print("New chunk is generating connective tissue to its left!");
            List<Vertex> borderVertices = left.GetComponent<ChunkGen>().GetBorderVertices(Direction.RIGHT);
            //foreach (Vertex v in borderVertices) vertices.Add(new Vertex(v.x - size, v.y));
            vertices.Add(new Vertex(bounds.min.x, bounds.max.y));
            vertices.Add(new Vertex(bounds.min.x, bounds.min.y));
        }

        GameObject right = TerrainManager.GetChunkAtCoords(new Vector2Int(coords.x + 1, coords.y));
        if (right != null)
        {
            print("New chunk is generating connective tissue to its right!");
            List<Vertex> borderVertices = right.GetComponent<ChunkGen>().GetBorderVertices(Direction.LEFT);
            // foreach (Vertex v in borderVertices) vertices.Add(new Vertex(v.x + size, v.y));
            vertices.Add(new Vertex(bounds.max.x, bounds.max.y));
            vertices.Add(new Vertex(bounds.max.x, bounds.min.y));  
        }

        GameObject down = TerrainManager.GetChunkAtCoords(new Vector2Int(coords.x, coords.y - 1));
        if (down != null)
        {
            print("New chunk is generating connective tissue below itself!");
            List<Vertex> borderVertices = down.GetComponent<ChunkGen>().GetBorderVertices(Direction.UP);
            // foreach (Vertex v in borderVertices) vertices.Add(new Vertex(v.x, v.y - size));
            vertices.Add(new Vertex(bounds.min.x, bounds.min.y));
            vertices.Add(new Vertex(bounds.max.x, bounds.min.y));
        }
        */

        // Generate vertices along each side

        // Let Triangle.NET do the hard work of actually generating the triangles to connect them
        foreach (Vertex v in vertices) polygon.Add(v);

        // Add the corners 
        polygon.Add(new Vertex(bounds.min.x, bounds.min.z));
        polygon.Add(new Vertex(bounds.min.x, bounds.max.z));
        polygon.Add(new Vertex(bounds.max.x, bounds.min.z));
        polygon.Add(new Vertex(bounds.max.x, bounds.max.z));

        // Add points at random, semi-bounded intervals along the edges, which produces harder to notice artifacts
        for (float i = bounds.min.x; i < bounds.max.x; i += cellWidth) polygon.Add(new Vertex(i, bounds.min.z));
        for (float i = bounds.min.x; i < bounds.max.x; i += cellWidth) polygon.Add(new Vertex(i, bounds.max.z));
        for (float i = bounds.min.z; i < bounds.max.z; i += cellHeight) polygon.Add(new Vertex(bounds.min.x, i));
        for (float i = bounds.min.z; i < bounds.max.z; i += cellHeight) polygon.Add(new Vertex(bounds.max.x, i));

        TriangleNet.Meshing.ConstraintOptions constraintOptions = new TriangleNet.Meshing.ConstraintOptions()
        {
            ConformingDelaunay = false, 
            // Convex = true 
            // SegmentSplitting = 1
        };

        TriangleNet.Meshing.QualityOptions qualityOptions = new TriangleNet.Meshing.QualityOptions()
        { 
            // MinimumAngle = 30,
        };

        mesh = (TriangleNet.Mesh)polygon.Triangulate(constraintOptions, qualityOptions);

        Mesh actualMesh = GenerateMesh(mesh);
        gameObject.GetComponent<MeshFilter>().mesh = actualMesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = actualMesh;
        
        List<Vector2> tempVertices = Vector3ToVector2(this.meshFilter.mesh.vertices);
        float offsetX = transform.position.x, offsetZ = transform.position.z;

        this.heightNoise = gameObject.AddComponent<Noise>();
        List<float> noiseValues = this.heightNoise.GenerateNoiseMap(tempVertices, offsetX, offsetZ);
        UpdateVertexHeightsAndColors(this.meshFilter.mesh, noiseValues);
        gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;
    }

    // Generates a Mesh object from the provided TriangleNet.Mesh object
    // Somewhere in here, the mesh is getting way bigger than its intended to be
    // Lets do some nitty gritty troubleshooting!
    public Mesh GenerateMesh(TriangleNet.Mesh srcMesh)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        IEnumerator<TriangleNet.Topology.Triangle> triangleEnum =
        srcMesh.triangles.GetEnumerator();

        for (int i = 0; i < srcMesh.Triangles.Count; i++)
        {
            if (!triangleEnum.MoveNext())
            {
                break;
            }

            TriangleNet.Topology.Triangle currentTriangle = triangleEnum.Current;

            //print("Current Triangle: " + currentTriangle);
            //print("v1: " + currentTriangle.vertices[0]);
            //print("v2: " + currentTriangle.vertices[1]);
            //print("v3: " + currentTriangle.vertices[2]);

            Vector3 v0 = new Vector3((float)currentTriangle.vertices[2].x, 0, (float)currentTriangle.vertices[2].y);
            Vector3 v1 = new Vector3((float)currentTriangle.vertices[1].x, 0, (float)currentTriangle.vertices[1].y);
            Vector3 v2 = new Vector3((float)currentTriangle.vertices[0].x, 0, (float)currentTriangle.vertices[0].y);

            triangles.Add(vertices.Count);
            triangles.Add(vertices.Count + 1);
            triangles.Add(vertices.Count + 2);  

            vertices.Add(v0);
            vertices.Add(v1);
            vertices.Add(v2);

            var normal = Vector3.Cross(v1 - v0, v2 - v0);            
            for (int x = 0; x < 3; x++)
            {
                normals.Add(normal);
                uvs.Add(Vector3.zero);
            }
        }

        Mesh chunkMesh = new Mesh();
        chunkMesh.vertices = vertices.ToArray();
        chunkMesh.uv = uvs.ToArray();
        chunkMesh.triangles = triangles.ToArray();
        chunkMesh.normals = normals.ToArray();
        return chunkMesh;        
    }

    // Takes the provided mesh, sets heights, and applies colors 
    // Modifies the input mesh in-place to avoid some costly array reassignments
    private void UpdateVertexHeightsAndColors(Mesh mesh, List<float> heightmap)
    {
        this.colorRandomizationNoise = gameObject.AddComponent<Noise>();
        this.colorRandomizationNoise.scale = 30; // We want a pretty minor shift

        Vector3[] meshVertices = mesh.vertices;        

        // Because vertices are duplicated across triangles rather than shared, we need to store what the first instance of each vertex's color was and then
        // use that cached color instead of randomly generating a new one the next time
        for (int i = 0; i < meshVertices.Length; i++)
        {
            // This gets a little confusing to keep track of, so here are the things happening here:
            // 1. The heightMap passed in is all [0, 1] Perlin map values.
            // 2. We evaluate that value on our height curve (which lets us further control the
            //      Perlin distribution on top of things like octaves and lacunarity that happen
            //      during Perlin noise sampling) then multiply by a constant scaling factor that
            //      makes it so this all doesn't range by like a meter.
            Vector3 vertex = meshVertices[i];
            float height = Mathf.Clamp(heightmap[i], 0, 1); // Clamp necessary because the heightCurve can't handle the *very* occasional points Perlin noise generates outside of [0, 1]
            meshVertices[i] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);

            // Slightly randomize a given color so that a given height level doesn't look super monotone 
            /*
            Color tweakColor(float randomizationFactor, Color color)
            {
                return new Color(
                    Mathf.Clamp(color.r + Random.Range(-1 * randomizationFactor, randomizationFactor), 0, 1),
                    Mathf.Clamp(color.g + Random.Range(-1 * randomizationFactor, randomizationFactor), 0, 1),
                    Mathf.Clamp(color.b + Random.Range(-1 * randomizationFactor, randomizationFactor), 0, 1),
                    color.a);
            }
            */

            // Colors need treated in triplets, as we assign the color to be the average of the three vertices in each given triangle
            // Note that {0, 1, 2}, {3, 4, 5}, {6, 7, 8} represents the grouping of each triangle vertex; it's easy because we don't do shared
        }

        // Given coordinates of the center point of a triangle, returns the color that triangle should be
        Color ChooseColor(Vector3 point)
        {
            // 1. Determine base color
            TerrainType type = ChooseTerrainType(point.y);
            Color baseColor = type.color;
            float randomizationFactor = type.randomizationFactor;

            // 2. Tweak 80% by Perlin noise
            float noise = colorRandomizationNoise.GetNoiseAtPoint(point.x, point.z);
            const float perlin_weight = .75f; 
            baseColor = new Color(
                Mathf.Clamp(baseColor.r + perlin_weight * noise, 0, 1), 
                Mathf.Clamp(baseColor.g + perlin_weight * noise, 0, 1), 
                Mathf.Clamp(baseColor.b + perlin_weight * noise, 0, 1));

            // 3. Tweak 20% by pure randomness
            const float random_weight = .1f;
            baseColor = new Color(
                Mathf.Clamp(baseColor.r + random_weight * Random.Range(-1, 1) * randomizationFactor, 0, 1), 
                Mathf.Clamp(baseColor.g + random_weight * Random.Range(-1, 1) * randomizationFactor, 0, 1), 
                Mathf.Clamp(baseColor.b + random_weight * Random.Range(-1, 1) * randomizationFactor, 0, 1));

            return baseColor;
        }

        Color[] colors = new Color[meshVertices.Length];
        for (int i = 0; i < meshVertices.Length; i += 3)
        {
            Vector3 averagePoint = (meshVertices[i] + meshVertices[i + 1] + meshVertices[i + 2]) / 3;
            averagePoint.y /= this.heightMultiplier;    
            Color color = ChooseColor(averagePoint);
            colors[i] = colors[i + 1] = colors[i + 2] = color;
        }

        // Update actual mesh properties; basically "apply" the heights to the mesh 
        mesh.vertices = meshVertices;
        mesh.colors = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    // How "vertical" we want our map to be. Lower values will result in less extreme highs and lows and will generally make slopes smoother.
    [SerializeField]
    private float heightMultiplier; 

    // A useful thing Unity adds that lets us essentially do a height distribution rather than relying entirely on noise ourselves
    [SerializeField]
    private AnimationCurve heightCurve;

    // Helper method that returns which color should be used for a given vertex height (technically a noise value, but it's basically the same thing)
    TerrainType ChooseTerrainType(float height)
    {
        foreach (TerrainType terrainType in terrainTypes)
        {
            // Triggers on the first one where we qualify the condition
            // For instance, we have water below .3, then lowlands below like .5, so if I feed in .4, it won't get in
            // here for water but it will get in here for lowlands 
            if (height <= terrainType.height)
            {
                return terrainType;
            }
        }

        // If we didn't hit one, choose the highest one 
        return terrainTypes[terrainTypes.Length - 1]; 
    }

    // TODO: More straightforward, connotative to make this a Set instead of a List (which implies order means something) 
    public HashSet<Vertex> GetBoundaryVertices()
    {
        List<Osub> boundarySubsegments = GetBoundaryEdges();
        HashSet<Vertex> boundaryVertices = new HashSet<Vertex>();
        foreach (Osub subsegment in boundarySubsegments)
        {
            // First vertex of segment 
            if (!boundaryVertices.Contains(subsegment.Segment.vertices[0]))
            {
                boundaryVertices.Add(subsegment.Segment.vertices[0]);
            }

            // Second vertex of segment
            if (!boundaryVertices.Contains(subsegment.Segment.vertices[1]))
            {
                boundaryVertices.Add(subsegment.Segment.vertices[1]);
            }
        }
        return boundaryVertices;
    }

    public List<Osub> GetBoundaryEdges()
    {
        Dictionary<Osub, int> edges = new Dictionary<Osub, int>();

        // Get a count of each subsegment 
        foreach (Triangle triangle in mesh.Triangles)
        {
            foreach (Osub subseg in triangle.subsegs)
            {
                if (!edges.ContainsKey(subseg))
                {
                    edges.Add(subseg, 1);
                }

                else
                {
                    edges[subseg] += 1;
                }
            }
        }

        // Eliminate any subsegment that appears in more than one triangle 
        List<Osub> uniqueSegments = new List<Osub>(); // Can't modify a dictionary during iteration
        foreach (Osub subseg in edges.Keys)
        {
            if (edges[subseg] == 1)
            {
                uniqueSegments.Add(subseg);
            }
        }

        return uniqueSegments;
    }

    // Renders Triangulations on game pause
    public void OnDrawGizmos()
    {
        if (mesh == null)
        {
            // We're probably in the editor
            return;
        }
        
        float offsetX = coords.x * size, offsetZ = coords.y * size;

        /* 
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(offsetX + -1 * size, 0, offsetZ + size), new Vector3(offsetX + size, 0, offsetZ + size)); // Top 
        Gizmos.DrawLine(new Vector3(offsetX + -1 * size, 0, offsetZ + -1 * size), new Vector3(offsetX + -1 * size, 0, offsetZ + size)); // Left
        Gizmos.DrawLine(new Vector3(offsetX + size, 0, offsetZ + -1 * size), new Vector3(offsetX + size, 0, offsetZ + size)); // Right
        Gizmos.DrawLine(new Vector3(offsetX + -1 * size, 0, offsetZ + -1 * size), new Vector3(offsetX + size, 0, offsetZ + -1 * size)); // Down
        */

        Gizmos.color = gizmoColor;
        Visualization.DrawTriangulations(mesh, offsetX, offsetZ);
        
        float x, z;
        foreach (Vertex v in vertices)
        {
            x = offsetX + (float)v.x;
            z = offsetZ + (float)v.y;
            Gizmos.DrawCube(new Vector3(offsetX + (float)v.x, 0, offsetZ + (float)v.y), new Vector3(1f, 1f, 1f));
        }

        // Visualization.DrawChunkBoundaryTriangles(mesh);
        // Visualization.DrawChunkBoundaryEdges(this);
    }
}