using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSettings
{
    [Header("General Settings")]
    public int maxSpawns = 20;
    public int totalSpawns = 0;
    public float minSpawnDelaySeconds = 5.0f;
    public float maxSpawnDelaySeconds = 60.0f;

    [Header("Rates")]
    [Range(0.0f, 1.0f)] public float preySpawnChance = 0.65f;
    [Range(0.0f, 1.0f)] public float predSpawnChance = 0.35f;
    [Range(0.0f, 1.0f)] public float singleSpawnChance = 0.25f;
    [Range(0.0f, 1.0f)] public float doubleSpawnChance = 0.25f;
    [Range(0.0f, 1.0f)] public float tripleSpawnChance = 0.40f;
    [Range(0.0f, 1.0f)] public float multiSpawnChance = 0.10f;
    [Range(0.0f, 1.0f)] public float sameSpeciesChance = 0.40f;

    [Header("Displacements")]
    public float minPlayerDistance = 20.0f;
    public float maxPlayerDistance = 200.0f;
    public float aboveGroundDistance = 1.0f;
    public float maxSpawnCenterDistance = 10.0f;
    public float minAnimalDistance = 5.0f;


    [Header("Despawning")]
    public float despawnRange = 60.0f;
    public float despawnDelaySeconds = 120.0f;

}
