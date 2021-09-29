using UnityEngine;
/// <summary>
/// Spawns a number of predators, prey, and stationary targets onto the scene
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Populate with whatever needs spawned")]
    public Bounds spawnArea;
    public GameObject predator;
    public int numPredators;
    public GameObject prey;
    public int numPrey;
    public GameObject target;
    public int numTargets;
    void Start()
    {
        SpawnObjects(numPredators, predator);
        SpawnObjects(numPrey, prey);
        SpawnObjects(numTargets, target);
    }

    // spawn quantity number of prefab objects at random locations within the spawnArea
    private void SpawnObjects(int quantity, GameObject prefab)
    {
        for (var i = 0; i < quantity; i++)
        {
            float x = Random.Range(spawnArea.min.x, spawnArea.max.x), z = Random.Range(spawnArea.min.z, spawnArea.max.z);
            float y = spawnArea.center.y;
            Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
        }
    }
}
