using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roam : MonoBehaviour
{
    Vector3 destination;
    [Header("Movement and Turn Speeds")]
    [SerializeField]
    [Range(0.0f, 10.0f)]
    float moveSpeed = 5.0f;
    [SerializeField]
    [Range(0.0f, 10.0f)]
    float turnSpeed = 2.0f;
    [Header("Distance to consider target reached")]
    [SerializeField]
    float targetTolerance = 3.0f;

    // restrictions on where the npc can go
    public float maxX;
    public float maxZ;

    CharacterController controller;

    void Start()
    {
        
        controller = GetComponent<CharacterController>();

        // Get intial move position
        GetNextPosition();
    }

    void Update()
    {
        if(Vector3.Distance(destination, transform.position) <= targetTolerance)
        {
            GetNextPosition();
        }

        // turn toward target
        Quaternion targetRotation = Quaternion.LookRotation(destination - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // move toward target
        transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed);
    }

    // generate a new position to navigate to
    void GetNextPosition()
    {
        destination = new Vector3(Random.Range(-maxX, maxX), transform.position.y, Random.Range(-maxZ, maxZ));
    }

}
