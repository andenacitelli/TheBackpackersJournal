using UnityEngine;

public class Creature : MonoBehaviour
{
    // Types of creatures in the game, can add more if expanding to different categories
    public enum CreatureTypes
    {
        PLAYER,
        PREY,
        PREDATOR,
    }

    public CreatureTypes creatureType;
}
