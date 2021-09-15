using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aspect : MonoBehaviour
{
    public enum AspectTypes
    {
        PLAYER,
        PREY,
        PREDATOR,
    }
    public AspectTypes aspectType;
}
