using System.Collections;
using UnityEngine;
using TriangleNet.Topology;
using TriangleNet.Geometry;

namespace Assets.WorldGen
{
    public class Visualization : MonoBehaviour
    {
        private void DrawTriangulations(TriangleNet.Mesh mesh)
        {
            Gizmos.color = Color.white;
            foreach (Edge edge in mesh.Edges)
            {
                Vertex v0 = mesh.vertices[edge.P0];
                Vertex v1 = mesh.vertices[edge.P1];
                Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
                Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);
                Gizmos.DrawLine(p0, p1);
            }
        }

        private void DrawChunkBoundaryTriangles(TriangleNet.Mesh mesh)
        {
            Gizmos.color = Color.cyan;
            foreach (Triangle triangle in mesh.Triangles)
            {
                if (triangle.neighbors[0].tri.hash == TriangleNet.Mesh.DUMMY ||
                    triangle.neighbors[1].tri.hash == TriangleNet.Mesh.DUMMY ||
                    triangle.neighbors[2].tri.hash == TriangleNet.Mesh.DUMMY)
                {
                    Vector3 p0 = new Vector3((float)triangle.vertices[0].x, 0, (float)triangle.vertices[0].y);
                    Vector3 p1 = new Vector3((float)triangle.vertices[1].x, 0, (float)triangle.vertices[1].y);
                    Vector3 p2 = new Vector3((float)triangle.vertices[2].x, 0, (float)triangle.vertices[2].y);
                    Gizmos.DrawLine(p0, p1);
                    Gizmos.DrawLine(p1, p2);
                    Gizmos.DrawLine(p2, p0);
                }
            }
        }

        // Gets a list of boundary edges by iterating through the triangles array and finding the edges
        // ("subsegments" as Triangle.NET calls them) that are only referenced once 
        private void DrawChunkBoundaryEdges(ChunkGen genComponent)
        {
            Gizmos.color = Color.yellow;
            foreach (Osub subseg in genComponent.GetBoundaryEdges())
            {
                Vector3 p0 = new Vector3((float)subseg.Segment.vertices[0].x, 0, (float)subseg.Segment.vertices[0].y);
                Vector3 p1 = new Vector3((float)subseg.Segment.vertices[1].x, 0, (float)subseg.Segment.vertices[1].y);
                Gizmos.DrawLine(p0, p1);
            }
        }
    }
}