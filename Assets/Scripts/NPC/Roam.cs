using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roam : MonoBehaviour
{
    public Vector3 target;
    [Header("Movement and Turn Speeds")]
    [SerializeField]
    float moveSpeed = 5.0f;
    [SerializeField]
    float turnSpeed = 2.0f;
    [Header("Distance to consider target reached")]
    [SerializeField]
    float targetTolerance = 3.0f;

    // restrictions on where the npc can go
    float minX, maxX;
    float minZ, maxZ;

    CharacterController controller;

    void Start()
    {
        // initialize movement restrictions
        minX = -15.0f;
        maxX = 15.0f;
        minZ = -15.0f;
        maxZ = 15.0f;

        controller = GetComponent<CharacterController>();

        // Get intial move position
        GetNextPosition();
    }

    void Update()
    {
        if(Vector3.Distance(target, transform.position) <= targetTolerance)
        {
            GetNextPosition();
        }

        // turn toward target
        Quaternion targetRotation = Quaternion.LookRotation(target - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // move toward target
        Vector3 moveDirection = target - transform.position;
        moveDirection.Normalize();
        moveDirection = transform.TransformDirection(moveDirection);
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }

    // generate a new position to navigate to
    void GetNextPosition()
    {
        target = new Vector3(Random.Range(minX, maxX), .5f, Random.Range(minZ, maxZ));
    }

}
