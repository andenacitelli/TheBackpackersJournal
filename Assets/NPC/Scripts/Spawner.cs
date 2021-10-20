using UnityEngine;
/// <summary>
/// Spawns a number of predators, prey, and stationary targets onto the scene
/// </summary>
public class Spawner : MonoBehaviour
{
    [Header("Populate with whatever needs spawned")]
    public int numPredators;
    public GameObject[] predators;
    public int numPrey;
    public GameObject[] prey;
    public int numTargets;
    public GameObject target;
    public Bounds spawnArea;
    void Start()
    {
        SpawnObjects(numPredators, predators);
        SpawnObjects(numPrey, prey);
        SpawnObjects(numTargets, target);
    }

    // spawn quantity number of prefab objects at random locations within the spawnArea
    private void SpawnObjects(int quantity, GameObject[] prefab)
    {
        int animalIndex = Random.Range(0, prefab.Length);
        AnimalController animal = prefab[animalIndex].GetComponent<AnimalController>();
        for (var i = 0; i < quantity; i++)
        {
            animal.territory = new Bounds(spawnArea.center, spawnArea.size);
            float x = Random.Range(spawnArea.min.x, spawnArea.max.x), z = Random.Range(spawnArea.min.z, spawnArea.max.z);
            float y = spawnArea.center.y;

            Instantiate(prefab[animalIndex], new Vector3(x, y, z), Quaternion.identity);

            animalIndex = Random.Range(0, prefab.Length);
            animal = prefab[animalIndex].GetComponent<AnimalController>();
        }
    }
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
