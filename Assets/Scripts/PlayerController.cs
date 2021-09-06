using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Speed")]
    public float moveSpeed;
    [Header("Look Sensitivity")]
    [Range (0.0f, 1.0f)]
    public float xSensitivity;
    [Range(0.0f, 1.0f)]
    public float ySensitivity;

    float moveHoriz, moveVert;
    float lookHoriz, lookVert;
    Transform playerCamera;

    private void Awake()
    {
        playerCamera = transform.Find("PlayerCamera");
    }
    private void Update()
    {
        // move player
        Vector3 moveDirection = Vector3.forward * moveVert + Vector3.right * moveHoriz;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // look around
        float yaw = lookHoriz * xSensitivity;
        float pitch = lookVert * ySensitivity ;
        transform.Rotate(Vector3.up * yaw );
        playerCamera.Rotate(Vector3.left * pitch);
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
