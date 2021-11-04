using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildlifeEvent : MonoBehaviour
{
    public enum EventRarity
    {
        COMMON,
        UNCOMMON,
        RARE,
    }

    [SerializeField] private EventRarity rarity = EventRarity.COMMON; // Currently only really used in unity editor but could be used for photo ID/rating purposes
    [SerializeField] private float durationSeconds = 120.0f;
    [SerializeField] private GameObject[] involvedAnimals;

    // Rarity of the event.
    public EventRarity Rarity { get => rarity;}

    // How long (in seconds) the event lasts
    public float DurationSeconds { get => durationSeconds;}
    
    // Array of what animals to use for the event
    public GameObject[] InvolvedAnimals { get => involvedAnimals;}

    public void Spawn(Vector3 center)
    {

    }
}
