using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.WorldGen
{
    /* Contains high-level, useful functions for interfacing with the terrain system. */
    public class TerrainFunctions
    {
        public struct TerrainPointData
        {
            // Stores the y-coordinate that the raycast hit the terrain layer
            public float height;

            // Stores the normal vector at the hit point; lets us do things like:
            // - Tilt trees according to terrain
            // - Don't generate trees and other flora on very steep hills
            public Vector3 normal;

            // Stores whether the raycast actually collided with the terrain layer or not
            public bool isHit;

            public TerrainPointData(float height, Vector3 normal, bool isHit)
            {
                this.height = height;
                this.normal = normal;
                this.isHit = isHit;
            }
        }

        // *Would* be much more performant to just query the heightmap, but we also want the normal vector in 90% of circumstances
        // TODO: Not every plant type probably needs the normal vector; maybe just trees? Can shift everything else over to a purely heightmap based variant
        public static TerrainPointData GetTerrainPointData(Vector2 point)
        {
            // Cast a ray from really high up to straight down, hitting the ground
            Ray ray = new Ray(new Vector3(point.x, 1000, point.y), Vector3.down);

            // For performance reasons, we want to use the chunk of interest's Collider.Raycast() function
            // instead of the general Physics.Raycast
            Vector2Int coords = new Vector2Int(Mathf.RoundToInt(point.x / ChunkGen.size), Mathf.RoundToInt(point.y / ChunkGen.size));
            GameObject chunk = TerrainManager.GetChunkAtCoords(coords);
            if (chunk == null) return new TerrainPointData();
            MeshCollider collider = chunk.GetComponent<MeshCollider>();

            // Terrain is its own layer so that we don't raycast into animals, trees, etc. 
            RaycastHit hit;
            if (collider.Raycast(ray, out hit, Mathf.Infinity))
            {
                return new TerrainPointData(hit.point.y, hit.normal, true);
            }

            // Just means it didn't hit terrain layer;
            // Caller is expected to check isHit parameter before doing assuming return isn't garbage
            return new TerrainPointData(0, Vector3.zero, false);
        }
    }
}