using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TriangleNet.Geometry;

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

    TriangleNet.Mesh mesh;

    // Represents how much to spread the Perlin noise out; for a flatter, smoother map, make this value high
    [SerializeField]
    private float mapScale;

    void Start()
    {
        // Calculate things we'll use all throughout the class 
        Vector3 tileSize = this.GetComponent<MeshRenderer>().bounds.size;
        tileWidth = Mathf.RoundToInt(tileSize.x); // Should theoretically already be 10, but can't hurt to round to be sure
        tileDepth = Mathf.RoundToInt(tileSize.z);

        // Actually generate the chunk
        GenerateChunk(); 
    }

    void GenerateChunk()
    {
        // Generate random list of vertices to use as our Delaunay vertices
        // Seed this random generation off chunk coords so each chunk generates the same every time. May be off-by-one, but doesn't really matter.
        Random.InitState(chunkSeedFromCoords(new Vector2Int((int) this.gameObject.transform.position.x / tileWidth, (int) this.gameObject.transform.position.x / tileDepth)));
        const int NUM_POINTS = 50;
        Polygon polygon = new Polygon();
        float chunkX = this.gameObject.transform.position.x, chunkZ = this.gameObject.transform.position.z;
        for (int i = 0; i < NUM_POINTS; i++)
        {
            // Generate random vertices within chunk boundaries
            polygon.Add(new Vertex(
                Random.Range(chunkX - tileWidth / 2, chunkX + tileWidth / 2),
                Random.Range(chunkZ - tileDepth / 2, chunkZ + tileDepth / 2)));
        }

        // Add the corners of the chunk
        // TODO: Will introduce visual straight lines along chunk edges; use a hole-filling algorithm like the one detailed in this paper: https://kth.diva-portal.org/smash/get/diva2:1106042/FULLTEXT01.pdf
        polygon.Add(new Vertex(chunkX - tileWidth / 2, chunkZ + tileDepth / 2));
        polygon.Add(new Vertex(chunkX - tileWidth / 2, chunkZ - tileDepth / 2));
        polygon.Add(new Vertex(chunkX + tileWidth / 2, chunkZ + tileDepth / 2));
        polygon.Add(new Vertex(chunkX + tileWidth / 2, chunkZ - tileDepth / 2));

        // Let Triangle.NET do the hard work of actually generating the triangles to connect them
        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true } ;
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);

        // Actually convert Delaunay to a mesh and redo the triangles to give us flat shading
        GenerateMesh();

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
        List<Vector2> vertices = Vector3ToVector2(this.meshFilter.mesh.vertices);
        float offsetX = this.gameObject.transform.position.x;
        float offsetZ = this.gameObject.transform.position.z;
        List<float> noiseValues = this.noise.GenerateNoiseMap(vertices, this.mapScale, offsetX, offsetZ, waves);
        UpdateVertexHeightsAndColors(noiseValues);
    }

    public void GenerateMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();

        IEnumerator<TriangleNet.Topology.Triangle> triangleEnum =
        mesh.triangles.GetEnumerator();

        for (int i = 0; i < mesh.Triangles.Count; i++)
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

        this.meshFilter.mesh.vertices = vertices.ToArray();
        this.meshFilter.mesh.uv = uvs.ToArray();
        this.meshFilter.mesh.triangles = triangles.ToArray();
        this.meshFilter.mesh.normals = normals.ToArray();
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();
        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }

    // Low-collision function that gives us as unique an integer as possible, given chunk coordinates
    int chunkSeedFromCoords(Vector2Int coords)
    {
        // Parse will fail if the second number's ToString() puts a negative at the beginning, so we just 
        // use the sign of the first one as the sign of whatever the overall method's output is
        coords.y = coords.y < 0 ? coords.y * -1 : coords.y;

        // This is ugly, but it's infrequently called and is about as unique as you can get
        // TODO: This will fail when you go very far due to chunk coord string concatenation parsing giving you something above the integer limit. *Highly* doubt this will come up later, so not worth finding a workaround right now.
        return int.Parse(coords.x.ToString() + coords.y.ToString());
    }

    // Converts the vertex representation of the mesh to non-shared vertices 
    private Vector2[] localVertexCoordinates; // Same length as mesh.vertices, but stores their chunk-specific subdivide ("local") coordinates 
    void UpdateTriangles()
    {
        // Essentially a Vector3 deep copy 
        Vector3 CopyVector3(Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        Vector3 tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;
        int width = (int)tileSize.x;
        int newVerticesPos = 0;
        Vector3[] newVertices = new Vector3[(int) Mathf.Pow(width, 2) * 6];

        // List of same order and length as normal vertex array that allows us to map a vertex index to a local chunk grid coordinate
        localVertexCoordinates = new Vector2[newVertices.Length];

        // Iterate through each vertex
        for (int row = 0; row < width - 1; row++)
        {
            for (int col = 0; col < width - 1; col++)
            {
                // Grab references to the vertices that will matter for us here 
                // Note that for loop conditions will stop us from hitting a bounds check 
                Vector3 currentVertex = this.meshFilter.mesh.vertices[row * width + col];
                Vector3 rightVertex = this.meshFilter.mesh.vertices[row * width + (col + 1)];
                Vector3 downVertex = this.meshFilter.mesh.vertices[(row + 1) * width + col];
                Vector3 downRightVertex = this.meshFilter.mesh.vertices[(row + 1) * width + (col + 1)];

                // We have to make the order a little weird on these; normals are calculated based on vertices being specified clockwise, 
                //     or something like that, which was the cause of half of my triangles being hidden (technically, shown from under the landscape)
                // First triangle: this vertex, vertex one to the right, vertex one to the right and one down
                newVertices[newVerticesPos] = CopyVector3(downRightVertex);
                localVertexCoordinates[newVerticesPos] = new Vector2(col + 1, row + 1);
                newVertices[newVerticesPos + 1] = CopyVector3(rightVertex);
                localVertexCoordinates[newVerticesPos + 1] = new Vector2(col + 1, row);
                newVertices[newVerticesPos + 2] = CopyVector3(currentVertex);
                localVertexCoordinates[newVerticesPos + 2] = new Vector2(col, row);

                // Second triangle: this vertex, vertex one to the bottom, vertex one to the right and one down 
                newVertices[newVerticesPos + 3] = CopyVector3(currentVertex); 
                localVertexCoordinates[newVerticesPos + 3] = new Vector2(col, row);
                newVertices[newVerticesPos + 4] = CopyVector3(downVertex);
                localVertexCoordinates[newVerticesPos + 4] = new Vector2(col, row + 1);
                newVertices[newVerticesPos + 5] = CopyVector3(downRightVertex);
                localVertexCoordinates[newVerticesPos + 5] = new Vector2(col + 1, row + 1);

                // Switch to calculating for next box subdivision inside the chunk 
                newVerticesPos += 6;
            }
        }

        // Triangle vertices are easy; we know each triplet of vertices represents a triangle now
        int[] newTriangles = new int[newVertices.Length];
        for (int i = 0; i < newTriangles.Length; i++)
        {
            newTriangles[i] = i;
        }

        this.meshFilter.mesh.vertices = newVertices;
        this.meshFilter.mesh.triangles = newTriangles;

        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
        }
        this.meshFilter.mesh.uv = uvs;
    }

    // How "vertical" we want our map to be. Lower values will result in less extreme highs and lows and will generally make slopes smoother.
    [SerializeField]
    private float heightMultiplier; 

    // A useful thing Unity adds that lets us essentially do a height distribution rather than relying entirely on noise ourselves
    [SerializeField]
    private AnimationCurve heightCurve;

    // Iterates through all vertices in a given Chunk and sets their heights based on their noise values 
    // I'd normally split this into two functions for better cohesion, but color assignment uses the *perlin noise value*, not the height value.
    //      We do it this way so we don't have to keep changing biome cutoffs if we change the map height scale constant.
    private void UpdateVertexHeightsAndColors(List<float> heightmap)
    {
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        Color[] colors = new Color[meshVertices.Length];

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
        this.meshFilter.mesh.colors = colors;
        this.meshFilter.mesh.vertices = meshVertices;
        this.meshFilter.mesh.RecalculateBounds();
        this.meshFilter.mesh.RecalculateNormals();
        this.meshCollider.sharedMesh = this.meshFilter.mesh;
    }

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

    // Renders Triangulations on game pause
    public void OnDrawGizmos()
    {
        if (mesh == null)
        {
            // We're probably in the editor
            return;
        }

        Gizmos.color = Color.red;
        foreach (Edge edge in mesh.Edges)
        {
            Vertex v0 = mesh.vertices[edge.P0];
            Vertex v1 = mesh.vertices[edge.P1];
            Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
            Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);
            Gizmos.DrawLine(p0, p1);
        }
    }
}