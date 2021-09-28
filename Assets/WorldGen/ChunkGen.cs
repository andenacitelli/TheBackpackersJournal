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

    // Set by the class Instanting the gameObject linked to this script
    public Vector2Int coords;

    void Start()
    {
        // Calculate things we'll use all throughout the class 
        Vector3 tileSize = this.GetComponent<MeshRenderer>().bounds.size;
        tileWidth = Mathf.RoundToInt(tileSize.x); // Should theoretically already be 10, but can't hurt to round to be sure
        tileDepth = Mathf.RoundToInt(tileSize.z);

        GenerateChunk(); 
    }

    void GenerateChunk()
    {
        // Seed this random generation off chunk coords so each chunk generates the same every time. May be off-by-one, but doesn't really matter.
        Random.InitState(coords.GetHashCode());

        // Generate set of vertices
        int NUM_POINTS = Random.Range(120, 180);
        Bounds chunkBounds = gameObject.GetComponent<MeshRenderer>().bounds;
        float radius = (chunkBounds.max.x - chunkBounds.min.x) * .1f;
        Polygon polygon = PointGeneration.generatePointsPoissonDiscSampling(NUM_POINTS, chunkBounds, radius);

        // Add corners of the chunk, which gets us close to continuity
        // TODO: Better, actually non-glitchy way of doing continuity is to do Triangulation with the border areas
        // polygon.Add(new Vertex(chunkBounds.min.x, chunkBounds.min.y));
        // polygon.Add(new Vertex(chunkBounds.min.x, chunkBounds.max.y));
        // polygon.Add(new Vertex(chunkBounds.max.x, chunkBounds.min.y));
        // polygon.Add(new Vertex(chunkBounds.max.x, chunkBounds.max.y));

        // Let Triangle.NET do the hard work of actually generating the triangles to connect them
        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true } ;
        mesh = (TriangleNet.Mesh) polygon.Triangulate(options);

        // Actually convert Delaunay to a mesh and redo the triangles to give us flat shading
        GenerateMesh();

        // ZERO clue why this is necessary, but THANK GOD it fixes the offset issue
        // This fixes the issue where there's weird space between them, but the chunks are still generating away from the player
        transform.position = Vector3.zero;

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

        Mesh chunkMesh = new Mesh();
        chunkMesh.vertices = vertices.ToArray();
        chunkMesh.uv = uvs.ToArray();
        chunkMesh.triangles = triangles.ToArray();
        chunkMesh.normals = normals.ToArray();
        gameObject.GetComponent<MeshFilter>().mesh = chunkMesh;
        gameObject.GetComponent<MeshCollider>().sharedMesh = chunkMesh;
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
            int colorForVertex = Random.Range(0, 1);
            //colors[i] = new Color(colorForVertex, colorForVertex, colorForVertex);
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