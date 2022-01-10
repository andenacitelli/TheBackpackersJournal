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

        // Performs a Raycast to get terrain height and normal vector at a given (x, z) point. 
        // Returns a TerrainPointData object. The `isHit` property stores whether the raycast actually
        // collided with the terrain layer; `height` and `normal` are gibberish if `isHit` is false.
        public static TerrainPointData GetTerrainPointData(Vector2 point)
        {
            // Cast a ray from really high up to straight down, hitting the ground
            Ray ray = new Ray(new Vector3(point.x, 1000, point.y), Vector3.down);

            // Terrain is its own layer so that we don't raycast into animals, trees, etc. 
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain")))
            {
                return new TerrainPointData(hit.point.y, hit.normal, true);
            }

            // Just means it didn't hit terrain layer;
            // Caller is expected to check isHit parameter before doing assuming return isn't garbage
            return new TerrainPointData(0, Vector3.zero, false);
        }
    }
}