using System.Collections;
using UnityEngine;

namespace Assets.WorldGen
{
    /* Contains high-level, useful functions for interfacing with the terrain system. */
    public class Terrain
    {
        // Performs a Raycast to get terrain height at a given (x, z) represented as a Vector2.
        static float GetHeightAtPoint(Vector2 point)
        {
            // Cast a ray from really high up to straight down, hitting the ground
            Ray ray = new Ray(new Vector3(point.x, Mathf.Infinity, point.y), Vector3.down);

            // Terrain is its own layer so that we don't raycast into animals, trees, etc. 
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.NameToLayer("Terrain")))
            {
                return hit.point.y;
            }

            // Should only get here if we call this function somewhere that doesn't currently have a chunk loaded
            return -1;
        }
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}