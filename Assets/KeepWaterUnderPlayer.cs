using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepWaterUnderPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Water should be at the same height (a hardcoded value - don't judge me)
        // but be a large plane that stays centered on the player
        // Note that this sets it in absolute world space; the relative coords will be  (0, <whatever y coord relative to player is necessary to keep it at 25 world space, 0)
        transform.position = new Vector3(transform.parent.position.x, 25, transform.parent.position.z);
    }
}
