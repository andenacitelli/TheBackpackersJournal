using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class MoveInputEvent : UnityEvent<float, float> { }
[Serializable]
public class LookInputEvent: UnityEvent<float, float> { }

public class InputController : MonoBehaviour
{
    Controls controls;
    public MoveInputEvent moveInputEvent;
    public LookInputEvent lookInputEvent;

    private void Awake()
    {
        controls = new Controls();
    }

    private void OnEnable()
    {
        // enable the Gameplay action map
        controls.Gameplay.Enable();


        controls.Gameplay.Move.performed += OnMovePerformed;
        // stop moving when key/stick released
        controls.Gameplay.Move.canceled += OnMovePerformed;

        controls.Gameplay.Look.performed += OnLookPerformed;
        // stop moving camera when key/stick released
        controls.Gameplay.Look.canceled += OnLookPerformed;
    }

    private void OnLookPerformed(InputAction.CallbackContext context)
    {
        Vector2 lookInput = context.ReadValue<Vector2>();
        // invoke event associated with camera movement with input values
        lookInputEvent.Invoke(lookInput.x, lookInput.y);
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        Vector2 moveInput = context.ReadValue<Vector2>();
        // invoke event associated with movement with input values
        moveInputEvent.Invoke(moveInput.x, moveInput.y);
    }
}
