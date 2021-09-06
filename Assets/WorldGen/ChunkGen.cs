using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    // Represents how much to spread the Perlin noise out; for a flatter, smoother map, make this value high
    [SerializeField]
    private float mapScale;

    void Start()
    {
        GenerateChunk(); 
    }

    // Holds the Vector2 local coordinate of every vertex in the mesh.vertices array 
    void GenerateChunk()
    {
        // Get a list of the square vertices 
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        int chunkDepth = (int)Mathf.Sqrt(meshVertices.Length);
        int chunkWidth = chunkDepth;

        // Convert from shared vertices to non-shared vertices; 
        // If we use shared vertices, there's just no way to 
        UpdateTriangles();

        // Offset from overall world position based on coordinates
        float offsetX = -this.gameObject.transform.position.x;
        float offsetZ = -this.gameObject.transform.position.z;
        Dictionary<Vector2, float> heightMap = this.noise.GenerateNoiseMap(chunkDepth, chunkWidth, this.mapScale, offsetX, offsetZ, waves);

        // Update vertex heights and colors 
        UpdateMeshVertices(heightMap);
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

        // Three vertices for each end vertex
        int width = (int) Mathf.Sqrt(this.meshFilter.mesh.vertices.Length);
        int newVerticesPos = 0;
        Vector3[] newVertices = new Vector3[(int) Mathf.Pow(width, 2) * 6];

        // We recalculate these 
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
    private void UpdateMeshVertices(Dictionary<Vector2, float> heightMap)
    {
        int tileDepth = (int) Mathf.Sqrt(heightMap.Count);
        int tileWidth = tileDepth;
        Vector3[] meshVertices = this.meshFilter.mesh.vertices;
        Color[] colors = new Color[meshVertices.Length];

        // Need a way to map the vertices to their subdivision values 
        Bounds chunkBounds = this.meshFilter.mesh.bounds;
        int vertexIndex = 0;
        for (int i = 0; i < meshVertices.Length; i++)
        {
            // Update mesh height 
            Vector3 vertex = meshVertices[i];
            float height = Mathf.Clamp(heightMap[localVertexCoordinates[i]], .4f, 1);
            // print("height: " + height);
            meshVertices[vertexIndex] = new Vector3(vertex.x, this.heightCurve.Evaluate(height) * this.heightMultiplier, vertex.z);
            // print("World height: " + this.heightCurve.Evaluate(height) * this.heightMultiplier);
            //  print("Setting y-height to " + this.heightCurve.Evaluate(height) * this.heightMultiplier);
            vertexIndex++;

            // Slightly randomize a given color so that a given height level doesn't look super monotone 
            Color tweakColor(float randomizationFactor, Color color)
            {
                return new Color(
                    color.r + Random.Range(-1 * randomizationFactor, randomizationFactor),
                    color.g + Random.Range(-1 * randomizationFactor, randomizationFactor),
                    color.b + Random.Range(-1 * randomizationFactor, randomizationFactor), 
                    color.a);
            }

            // Update mesh colors; I have a shader that draws from the vertex colors 
            TerrainType terrainType = ChooseTerrainType(height);
            colors[i] = tweakColor(terrainType.randomizationFactor, terrainType.color);
        }

        // Update actual mesh properties; basically "apply" the heights to the mesh 
        this.meshFilter.mesh.colors = colors; // Apply color
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
}