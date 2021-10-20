using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [Header("GUI")]
    public GameObject storagePopUp;
    public void PlayInteractAnim()
    {
        var pos = transform.position;
        Vector3 pressedPos = new Vector3(pos.x, pos.y, pos.z - .09f);
        transform.Translate(pressedPos);
    }
}
