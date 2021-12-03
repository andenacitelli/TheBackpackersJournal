using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSettings : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] private int maxSpawns = 20;
    [SerializeField] private int totalSpawns = 0;
    [SerializeField] private float minSpawnDelaySeconds = 5.0f;
    [SerializeField] private float maxSpawnDelaySeconds = 60.0f;
    public SpawnManager spawnManager;

    [Header("Rates")]
    [SerializeField] [Range(0.0f, 1.0f)] private float preySpawnChance = 0.65f;
    [SerializeField] [Range(0.0f, 1.0f)] private float predSpawnChance = 0.35f;
    [SerializeField] [Range(0.0f, 1.0f)] public float singleSpawnChance = 0.25f;
    [SerializeField] [Range(0.0f, 1.0f)] private float doubleSpawnChance = 0.25f;
    [SerializeField] [Range(0.0f, 1.0f)] private float tripleSpawnChance = 0.40f;
    [SerializeField] [Range(0.0f, 1.0f)] private float multiSpawnChance = 0.10f;
    [SerializeField] [Range(0.0f, 1.0f)] private float sameSpeciesChance = 0.40f;

    [Header("Displacements")]
    [SerializeField] private float minPlayerDistance = 20.0f;
    [SerializeField] private float maxPlayerDistance = 200.0f;
    [SerializeField] private float aboveGroundDistance = 1.0f;
    [SerializeField] private float maxSpawnCenterDistance = 10.0f;
    [SerializeField] private float minAnimalDistance = 5.0f;


    [Header("Despawning")]
    [SerializeField] private float despawnDelaySeconds = 120.0f;

    public int TotalSpawns { get => totalSpawns;}
    public int MaxSpawns { get => maxSpawns;}
    public float PreySpawnChance { get => preySpawnChance; }
    public float PredSpawnChance { get => predSpawnChance; }
    public float DoubleSpawnChance { get => doubleSpawnChance; }
    public float TripleSpawnChance { get => tripleSpawnChance; }
    public float MultiSpawnChance { get => multiSpawnChance; }
    public float SameSpeciesChance { get => sameSpeciesChance; }
    public float MinPlayerDistance { get => minPlayerDistance; }
    public float MaxPlayerDistance { get => maxPlayerDistance; }
    public float AboveGroundDistance { get => aboveGroundDistance; }
    public float MaxSpawnCenterDistance { get => maxSpawnCenterDistance; }
    public float MinAnimalDistance { get => minAnimalDistance; }
    public float MinSpawnDelaySeconds { get => minSpawnDelaySeconds; }
    public float MaxSpawnDelaySeconds { get => maxSpawnDelaySeconds; }
    public float DespawnDelaySeconds { get => despawnDelaySeconds; }

    private void Start()
    {
        StartCoroutine(UpdateSpawnTotal());
    }

    private bool TotalChanged() { return spawnManager.worldAnimals.Count != TotalSpawns; }

    /// <summary>
    /// Updates the total number of animals spawned in the game and waits for it to change.
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdateSpawnTotal()
    {
        while (true)
        {
            yield return new WaitUntil(TotalChanged);
            totalSpawns = spawnManager.worldAnimals.Count;
        }
    }
}
