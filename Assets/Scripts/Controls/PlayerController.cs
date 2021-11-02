using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Speed")]
    [Range (0.0f, 10.0f)]
    public float moveSpeed = 5.0f;
    [Header("Look Sensitivity")]
    [Range (0.0f, 1.0f)]
    public float horizontalSensitivity = 0.5f;
    [Range(0.0f, 1.0f)]
    public float verticalSensitivity = 0.25f;
    [Header("Vertical Look Restriction")]
    [Range (0.0f, 90.0f)]
    [SerializeField] float verticalClamp = 90.0f;

    CharacterController controller;
    float moveHoriz, moveVert;
    float lookHoriz, lookVert;
    float vertRotation;
    Transform playerCamera;
    public static PlayerController instance;
    private void Awake()
    {
        /*has one and only one PlayerController throughout the game*/
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        controller = GetComponent<CharacterController>();
        playerCamera = transform.Find("PlayerCamera");
    }
    public void UpdateMove()
    {
        // Move Player
        Vector3 moveDirection = Vector3.forward * moveVert + Vector3.right * moveHoriz + Vector3.up * -9.81f;
        moveDirection = transform.TransformDirection(moveDirection);
        if (!controller.isGrounded)
        {
            Debug.Log("Player isn't in contact with the ground");
        }
        controller.Move(moveDirection * moveSpeed * Time.deltaTime); 
    }

    public void UpdateLook()
    {
        // Look Around
        // player
        transform.Rotate(Vector3.up, lookHoriz * horizontalSensitivity);
        // camera
        vertRotation -= lookVert;
        vertRotation = Mathf.Clamp(vertRotation, -verticalClamp, verticalClamp);
        Vector3 newRotation = transform.eulerAngles;
        newRotation.x = vertRotation;
        playerCamera.eulerAngles = newRotation;
    }

    public void OnMoveInput(float horizontal, float vertical)
    {
        moveHoriz = horizontal;
        moveVert = vertical;
        //Debug.Log($"Move Input: {moveHoriz}, {moveVert}");
    }

    public void OnLookInput(float horizontal, float vertical)
    {
        lookHoriz = horizontal;
        lookVert = vertical;
        //Debug.Log($"Camera Input: {lookHoriz}, {lookVert}");
    }

}
