using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TriangleNet.Geometry;
using TriangleNet.Topology;

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

    // Booleans used to track whenever we generate the connecting triangulations
    // We set these IMMEDIATELY after we start generating one on both connected chunks, then check these every time we are about to start again
    public bool leftStarted = false, rightStarted = false, upStarted = false, downStarted = false;

    // Holds the inner triangulation of this chunk in Triangle.NET's format
    TriangleNet.Mesh mesh;

    // Represents how much to spread the Perlin noise out; for a flatter, smoother map, make this value high
    [SerializeField]
    private float mapScale;

    // Set by the class Instanting the gameObject linked to this script
    public Vector2Int coords;

    // NOTE: As we set `coords` AFTER initialization, we can't do chunk seeding in Awake. It HAS to be in Start() for it to work correctly.
    private void Start()
    {
        // Calculate things we'll use all throughout the class 
        Vector3 tileSize = this.GetComponent<MeshRenderer>().bounds.size;
        tileWidth = Mathf.RoundToInt(tileSize.x); // Should theoretically already be 10, but can't hurt to round to be sure
        tileDepth = Mathf.RoundToInt(tileSize.z);

        GenerateChunk(); 
    }

    // Update vertex heights and apply height-based coloration
    List<Vector2> Vector3ToVector2(Vector3[] vList)
    {
        List<Vector2> output = new List<Vector2>();
        foreach (Vector3 v in vList)
        {
            output.Add(new Vector2(v.x, v.z));
        }
        return output;
    }

    void GenerateChunk()
    {
        // Seed this random generation off chunk coords so each chunk generates the same every time. May be off-by-one, but doesn't really matter.
        Random.InitState(coords.GetHashCode());
        print("Coords Hashcode: " + coords.GetHashCode());

        // 1. Generate this chunk's Delaunay triangulation, ezpz
        int NUM_POINTS = Random.Range(70, 130);
        Bounds chunkBounds = gameObject.GetComponent<MeshRenderer>().bounds;
        chunkBounds.extents = new Vector3(chunkBounds.extents.x * .9f, 0, chunkBounds.extents.z * .9f);
        float radius = (chunkBounds.max.x - chunkBounds.min.x) * .1f;
        Polygon polygon = PointGeneration.generatePointsPoissonDiscSampling(NUM_POINTS, chunkBounds, radius);

        // TODO: Better, actually non-glitchy way of doing continuity is to do Triangulation with the border areas
        // We essentially set up a bunch of vertices along the edges

        // Add the corners 
        // polygon.Add(new Vertex(chunkBounds.min.x, chunkBounds.min.z));
        //polygon.Add(new Vertex(chunkBounds.min.x, chunkBounds.max.z));
        //polygon.Add(new Vertex(chunkBounds.max.x, chunkBounds.min.z));
        //polygon.Add(new Vertex(chunkBounds.max.x, chunkBounds.max.z));

        // Add points at random, semi-bounded intervals along the edges, which produces harder to notice artifacts
        //for (float i = chunkBounds.min.x; i < chunkBounds.max.x; i += 10) polygon.Add(new Vertex(i, chunkBounds.min.z));
        //for (float i = chunkBounds.min.x; i < chunkBounds.max.x; i += 10) polygon.Add(new Vertex(i, chunkBounds.max.z));
        //for (float i = chunkBounds.min.z; i < chunkBounds.max.z; i += 10) polygon.Add(new Vertex(chunkBounds.min.x, i));
        //for (float i = chunkBounds.min.z; i < chunkBounds.max.z; i += 10) polygon.Add(new Vertex(chunkBounds.max.x, i));

        // Let Triangle.NET do the hard work of actually generating the triangles to connect them
        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true } ;

        // Actually convert Delaunay to a mesh and redo the triangles to give us flat shading
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);
        // print("# of Vertices: " + polygon.Count);
        Mesh actualMesh = GenerateMesh(mesh);
        gameObject.GetComponent<MeshFilter>().mesh = actualMesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = actualMesh;

        // ZERO clue why this is necessary, but THANK GOD it fixes the offset issue
        // This fixes the issue where there's weird space between them, but the chunks are still generating away from the player
        transform.position = Vector3.zero;

        List<Vector2> vertices = Vector3ToVector2(this.meshFilter.mesh.vertices);
        float offsetX = this.gameObject.transform.position.x;
        float offsetZ = this.gameObject.transform.position.z;
        List<float> noiseValues = this.noise.GenerateNoiseMap(vertices, this.mapScale, offsetX, offsetZ, waves);
        UpdateVertexHeightsAndColors(this.meshFilter.mesh, noiseValues);
        gameObject.GetComponent<MeshCollider>().sharedMesh = gameObject.GetComponent<MeshFilter>().mesh;

        // 2. Check each of the four directly neighboring chunks. If they've been generated, use triangulation to generate a mesh to connect them.
        // TODO: Be able to access the other chunk's vertices and get which ones are neighbors.
        // TODO: 
        const int UP = 0, RIGHT = 1, DOWN = 2, LEFT = 3; 

        // ABOVE
        GameObject above = TerrainManager.GetChunkAtCoords(new Vector2Int(coords.x, coords.y + 1));
        if (above != null && !above.GetComponent<ChunkGen>().downStarted) // If the chunk is loaded in, meaning we should do triangulation on the shared region 
        {
            // Flag that we're getting this one so the chunk above this doesn't concurrently start loading it as well 
            upStarted = true;
            above.GetComponent<ChunkGen>().downStarted = true;

            // End polygon we will use to triangulate then produce a mesh from 
            Polygon interpPolygon = new Polygon();

            // Add vertices we want to use from our section 
            // TODO: Can optimize this by only getting border vertices once, THEN filtering up/down/left/right
            HashSet<Vertex> myNearVertices = GetBorderVerticesInQuadrant(UP);
            foreach (Vertex v in myNearVertices)
            {
                interpPolygon.Add(v);
            }

            // Add vertices we want to use from the other chunk 
            HashSet<Vertex> otherNearVertices = above.GetComponent<ChunkGen>().GetBorderVerticesInQuadrant(DOWN);
            foreach (Vertex v in otherNearVertices)
            {
                interpPolygon.Add(v);
            }

            // Add the two common corners
            interpPolygon.Add(new Vertex(chunkBounds.max.x, chunkBounds.max.y));
            interpPolygon.Add(new Vertex(chunkBounds.min.x, chunkBounds.max.y));

            

            // Do the actual triangulation 
            TriangleNet.Mesh interpMesh = (TriangleNet.Mesh) interpPolygon.Triangulate(options);

            // Convert into a mesh and render
            Mesh interpMesh2 = GenerateMesh(interpMesh);
            GameObject chunkBorder = new GameObject();
            chunkBorder.GetComponent<MeshFilter>().mesh = interpMesh2;
            chunkBorder.GetComponent<MeshCollider>().sharedMesh = interpMesh2;

            List<Vector2> vertices2 = Vector3ToVector2(chunkBorder.GetComponent<MeshFilter>().mesh.vertices);
            List<float> noiseValues2 = this.noise.GenerateNoiseMap(vertices2, this.mapScale, offsetX, offsetZ, waves);
            UpdateVertexHeightsAndColors(this.meshFilter.mesh, noiseValues2);
            chunkBorder.GetComponent<MeshCollider>().sharedMesh = chunkBorder.GetComponent<MeshFilter>().mesh;
        }

        // BELOW
        GameObject below = TerrainManager.GetChunkAtCoords(new Vector2Int(coords.x, coords.y + 1));
        if (below != null && !below.GetComponent<ChunkGen>().upStarted)
        {
            downStarted = true;
            below.GetComponent<ChunkGen>().upStarted = true;

            // End polygon we will use to triangulate then produce a mesh from 
            Polygon interpPolygon = new Polygon();

            // Add vertices we want to use from our section 
            // TODO: Can optimize this by only getting border vertices once, THEN filtering up/down/left/right
            HashSet<Vertex> myNearVertices = GetBorderVerticesInQuadrant(DOWN);
            foreach (Vertex v in myNearVertices)
            {
                interpPolygon.Add(v);
            }

            // Add vertices we want to use from the other chunk 
            HashSet<Vertex> otherNearVertices = above.GetComponent<ChunkGen>().GetBorderVerticesInQuadrant(UP);
            foreach (Vertex v in otherNearVertices)
            {
                interpPolygon.Add(v);
            }

            // Add the two common corners
            interpPolygon.Add(new Vertex(chunkBounds.max.x, chunkBounds.min.y));
            interpPolygon.Add(new Vertex(chunkBounds.min.x, chunkBounds.min.y));

            // Do the actual triangulation 
            TriangleNet.Mesh interpMesh = (TriangleNet.Mesh)interpPolygon.Triangulate(options);

            // Convert into a mesh and render
            Mesh interpMesh2 = GenerateMesh(interpMesh);
            GameObject chunkBorder = new GameObject();
            chunkBorder.GetComponent<MeshFilter>().mesh = interpMesh2;
            chunkBorder.GetComponent<MeshCollider>().sharedMesh = interpMesh2;

            List<Vector2> vertices2 = Vector3ToVector2(chunkBorder.GetComponent<MeshFilter>().mesh.vertices);
            List<float> noiseValues2 = this.noise.GenerateNoiseMap(vertices2, this.mapScale, offsetX, offsetZ, waves);
            UpdateVertexHeightsAndColors(this.meshFilter.mesh, noiseValues2);
            chunkBorder.GetComponent<MeshCollider>().sharedMesh = chunkBorder.GetComponent<MeshFilter>().mesh;
        }

        // LEFT 
        GameObject left = TerrainManager.GetChunkAtCoords(new Vector2Int(coords.x, coords.y + 1));
        if (left != null && !left.GetComponent<ChunkGen>().rightStarted)
        {
            leftStarted = true;
            left.GetComponent<ChunkGen>().rightStarted = true;

            // End polygon we will use to triangulate then produce a mesh from 
            Polygon interpPolygon = new Polygon();

            // Add vertices we want to use from our section 
            // TODO: Can optimize this by only getting border vertices once, THEN filtering up/down/left/right
            HashSet<Vertex> myNearVertices = GetBorderVerticesInQuadrant(LEFT);
            foreach (Vertex v in myNearVertices)
            {
                interpPolygon.Add(v);
            }

            // Add vertices we want to use from the other chunk 
            HashSet<Vertex> otherNearVertices = above.GetComponent<ChunkGen>().GetBorderVerticesInQuadrant(RIGHT);
            foreach (Vertex v in otherNearVertices)
            {
                interpPolygon.Add(v);
            }

            // Add the two common corners
            interpPolygon.Add(new Vertex(chunkBounds.min.x, chunkBounds.min.y));
            interpPolygon.Add(new Vertex(chunkBounds.min.x, chunkBounds.max.y));

            // Do the actual triangulation 
            TriangleNet.Mesh interpMesh = (TriangleNet.Mesh)interpPolygon.Triangulate(options);

            // Convert into a mesh and render
            Mesh interpMesh2 = GenerateMesh(interpMesh);
            GameObject chunkBorder = new GameObject();
            chunkBorder.GetComponent<MeshFilter>().mesh = interpMesh2;
            chunkBorder.GetComponent<MeshCollider>().sharedMesh = interpMesh2;

            List<Vector2> vertices2 = Vector3ToVector2(chunkBorder.GetComponent<MeshFilter>().mesh.vertices);
            List<float> noiseValues2 = this.noise.GenerateNoiseMap(vertices2, this.mapScale, offsetX, offsetZ, waves);
            UpdateVertexHeightsAndColors(this.meshFilter.mesh, noiseValues2);
            chunkBorder.GetComponent<MeshCollider>().sharedMesh = chunkBorder.GetComponent<MeshFilter>().mesh;
        }

        // RIGHT 
        GameObject right = TerrainManager.GetChunkAtCoords(new Vector2Int(coords.x, coords.y + 1));
        if (right != null && !right.GetComponent<ChunkGen>().leftStarted)
        {
            rightStarted = true;
            right.GetComponent<ChunkGen>().leftStarted = true;

            // End polygon we will use to triangulate then produce a mesh from 
            Polygon interpPolygon = new Polygon();

            // Add vertices we want to use from our section 
            // TODO: Can optimize this by only getting border vertices once, THEN filtering up/down/left/right
            HashSet<Vertex> myNearVertices = GetBorderVerticesInQuadrant(RIGHT);
            foreach (Vertex v in myNearVertices)
            {
                interpPolygon.Add(v);
            }

            // Add vertices we want to use from the other chunk 
            HashSet<Vertex> otherNearVertices = above.GetComponent<ChunkGen>().GetBorderVerticesInQuadrant(LEFT);
            foreach (Vertex v in otherNearVertices)
            {
                interpPolygon.Add(v);
            }

            // Add the two common corners
            interpPolygon.Add(new Vertex(chunkBounds.max.x, chunkBounds.min.y));
            interpPolygon.Add(new Vertex(chunkBounds.max.x, chunkBounds.max.y));    

            // Do the actual triangulation 
            TriangleNet.Mesh interpMesh = (TriangleNet.Mesh)interpPolygon.Triangulate(options);

            // Convert into a mesh and render
            Mesh interpMesh2 = GenerateMesh(interpMesh);
            GameObject chunkBorder = new GameObject();
            chunkBorder.GetComponent<MeshFilter>().mesh = interpMesh2;
            chunkBorder.GetComponent<MeshCollider>().sharedMesh = interpMesh2;

            List<Vector2> vertices2 = Vector3ToVector2(chunkBorder.GetComponent<MeshFilter>().mesh.vertices);
            List<float> noiseValues2 = this.noise.GenerateNoiseMap(vertices2, this.mapScale, offsetX, offsetZ, waves);
            UpdateVertexHeightsAndColors(this.meshFilter.mesh, noiseValues2);
            chunkBorder.GetComponent<MeshCollider>().sharedMesh = chunkBorder.GetComponent<MeshFilter>().mesh;
        }
    }

    // 0 = Up, 1 = Right, 2 = Down, 3 = Right
    // TODO: Probably cleaner to make this an enum, just looking for something quick and dirty right now 
    public HashSet<Vertex> GetBorderVerticesInQuadrant(int quadrant)
    {
        HashSet<Vertex> boundaryVertices = GetBoundaryVertices();
        HashSet<Vertex> verticesInQuadrant = new HashSet<Vertex>();

        // TODO: Lots of nesting and shared code; probably possible to improve that
        Vector3 chunkCenter = gameObject.GetComponent<MeshRenderer>().bounds.center; 
        foreach (Vertex v in boundaryVertices)
        {
            float dx = (float) v.x - chunkCenter.x, dy = (float) v.y - chunkCenter.z;
            switch (quadrant)
            {
                // Top = dy positive and abs(dy) > abs(dx)
                case 0:
                    {
                        if (dy > 0 && Mathf.Abs(dy) > Mathf.Abs(dx))
                        {
                            verticesInQuadrant.Add(v);
                        }
                        break;
                    }

                // Right = dx positive and abs(dx) > abs(dy)
                case 1:
                    {
                        if (dx > 0 && Mathf.Abs(dx) > Mathf.Abs(dy))
                        {
                            verticesInQuadrant.Add(v);
                        }
                        break;
                    }

                // Down = dy negative and abs(dy) > abs(dx) 
                case 2:
                    {
                        if (dy < 0 && Mathf.Abs(dy) > Mathf.Abs(dx))
                        {
                            verticesInQuadrant.Add(v);
                        }
                        break;
                    }

                // Left = dx negative and abs(dx) > abs(dy) 
                case 3:
                    {
                        if (dx < 0 && Mathf.Abs(dx) > Mathf.Abs(dy))
                        {
                            verticesInQuadrant.Add(v);
                        }
                        break;
                    }

                default:
                    {
                        print("ERROR: GetBorderVerticesInQuadrant called in segment not in [0, 4]!");
                        return null;
                    }
            }
        }
        return verticesInQuadrant;
    }

    // Generates a Mesh object from the provided TriangleNet.Mesh object
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
        Vector3[] meshVertices = mesh.vertices;
        Color[] colors = new Color[meshVertices.Length];

        // TODO: Seed color randomization
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
            Color tweakColor(float randomizationFactor, Color color)
            {
                return new Color(
                    Mathf.Clamp(color.r + Random.Range(-1 * randomizationFactor, randomizationFactor), 0, 1),
                    Mathf.Clamp(color.g + Random.Range(-1 * randomizationFactor, randomizationFactor), 0, 1),
                    Mathf.Clamp(color.b + Random.Range(-1 * randomizationFactor, randomizationFactor), 0, 1),
                    color.a);
            }

            // Update mesh colors; I have a shader that draws from the vertex colors 
            TerrainType terrainType = ChooseTerrainType(height);
            colors[i] = tweakColor(terrainType.randomizationFactor, terrainType.color);
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

    private List<Osub> GetBoundaryEdges()
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

        DrawTriangulations();
        DrawChunkBoundaryTriangles();
        DrawChunkBoundaryEdges();
    }
}