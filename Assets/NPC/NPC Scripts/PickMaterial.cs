using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickMaterial : MonoBehaviour
{
    // Materials and mesh
    [SerializeField] protected Material[] skins;
    [SerializeField] protected SkinnedMeshRenderer animalMeshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        int skinIndex = Random.Range(0, skins.Length);
        animalMeshRenderer.materials[0] = skins[skinIndex];
    }
}
