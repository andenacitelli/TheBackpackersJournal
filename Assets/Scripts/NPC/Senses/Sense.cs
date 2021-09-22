using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sense : MonoBehaviour
{
    public Aspect.AspectTypes aspectName = Aspect.AspectTypes.PREY;
    [HideInInspector]
    public List<GameObject> detectedTargets = new List<GameObject>();

    [SerializeField]
    [Range(0.0f, 1.0f)]
    float detectRate = 1.0f;

    protected virtual void Initialize() { }
    protected virtual void DetectAspect() { }

    void Start()
    {
        Initialize();
        StartCoroutine(FindTargetsWithDelay());
    }

    IEnumerator FindTargetsWithDelay()
    {
        while (true)
        {
            UpdateSense();
            yield return new WaitForSeconds(detectRate);
        }
    }
    public void UpdateSense()
    {
            detectedTargets.Clear();
            DetectAspect();
    }
}
