using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public float lookSpeed;

    float moveHoriz, moveVert;
    float lookHoriz, lookVert;
    Transform camera;

    private void Awake()
    {
        camera = transform.Find("PlayerCamera");
    }
    private void Update()
    {
        // move player
        Vector3 moveDirection = Vector3.forward * moveVert + Vector3.right * moveHoriz;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
    public void OnMoveInput(float horizontal, float vertical)
    {
        moveHoriz = horizontal;
        moveVert = vertical;
        Debug.Log($"Move Input: {moveHoriz}, {moveVert}");
    }

    public void OnCameraInput(float horizontal, float vertical)
    {
        lookHoriz = horizontal;
        lookVert = vertical;
        Debug.Log($"Camera Input: {lookHoriz}, {lookVert}");
    }
}
