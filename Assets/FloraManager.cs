using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloraManager : MonoBehaviour
{
    // List of all Flora prefabs
    // A lot more scalable to just define file paths and dynamically load these each time, rather than manually drag them all over in the editor
    private static Dictionary<string, Object[]> floraPrefabs = new Dictionary<string, Object[]>();

    // Start is called before the first frame update
    void Start()
    {
        LoadPrefabs();
    }

    const string basePath = "Low Poly Vegetation Pack/Vegetation Assets/Prefabs"; 
    void LoadPrefabs() {
        floraPrefabs.Add("Bush", Resources.LoadAll(basePath + "/Bushes/Bush", typeof(Object)));
        floraPrefabs.Add("DeadBush", Resources.LoadAll(basePath + "/Bushes/DeadBush", typeof(Object)));
        floraPrefabs.Add("FlowerBush", Resources.LoadAll(basePath + "/Bushes/FlowerBush", typeof(Object)));
        floraPrefabs.Add("CactusNoBottoms", Resources.LoadAll(basePath + "/Cactus/No_Bottoms", typeof(Object)));
        floraPrefabs.Add("CactusWithBottoms", Resources.LoadAll(basePath + "/Cactus/With_Bottoms", typeof(Object)));
        floraPrefabs.Add("FlowersOneSided", Resources.LoadAll(basePath + "/Flowers/OneSided", typeof(Object)));
        floraPrefabs.Add("FlowersTwoSided", Resources.LoadAll(basePath + "/Flowers/TwoSided", typeof(Object)));
        floraPrefabs.Add("Grass3D", Resources.LoadAll(basePath + "/Grass/Grass3D", typeof(Object)));
        floraPrefabs.Add("GrassPlane", Resources.LoadAll(basePath + "/Grass/GrassPlane", typeof(Object)));
        floraPrefabs.Add("MeshGrass", Resources.LoadAll(basePath + "/Grass/MeshGrass", typeof(Object)));
        floraPrefabs.Add("Mushrooms", Resources.LoadAll(basePath + "/Mushrooms", typeof(Object)));
        floraPrefabs.Add("OtherPlants", Resources.LoadAll(basePath + "/Plants/Other_Plants", typeof(Object)));
        floraPrefabs.Add("Reeds", Resources.LoadAll(basePath + "/Reeds", typeof(Object)));
    }

    public static Object GetRandomPrefabOfType(string floraType) {
        return floraPrefabs[floraType][Random.Range(0, floraPrefabs[floraType].Length)];
    }
}
