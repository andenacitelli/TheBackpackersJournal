using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class MoveInputEvent : UnityEvent<float, float> { }
[Serializable]
public class CameraInputEvent: UnityEvent<float, float> { }
public class InputController : MonoBehaviour
{
    Controls controls;
    public MoveInputEvent moveInputEvent;
    public CameraInputEvent cameraInputEvent;

    private void Awake()
    {
        controls = new Controls();
    }

    private void OnEnable()
    {
        // enable the Gameplay action map
        controls.Gameplay.Enable();

        controls.Gameplay.Move.performed += OnMovePerformed;
        controls.Gameplay.CameraMove.performed += OnCameraMovePerformed;
    }

    private void OnCameraMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 cameraInput = context.ReadValue<Vector2>();
        cameraInputEvent.Invoke(cameraInput.x, cameraInput.y);
        Debug.Log($"Camera Input: {cameraInput}");
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        moveInputEvent.Invoke(moveInput.x, moveInput.y);
        Debug.Log($"Move Input: {moveInput}");
    }
}
