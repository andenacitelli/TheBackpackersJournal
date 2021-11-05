using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface ICanPhoto{
    GameObject ReportIsVisible();

}
public class CanPhoto : MonoBehaviour, ICanPhoto
{
    // This class can hold any attribute values we want
    // to attach to the attributes
    [SerializeField] CameraRoll cRoll;

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    public GameObject ReportIsVisible()
    {
        GameObject hold = new GameObject();
        if (rend.isVisible)
        {
            hold = gameObject;
        }
        
        return hold;
    }

}
