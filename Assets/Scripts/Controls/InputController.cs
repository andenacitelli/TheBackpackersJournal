using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class MoveInputEvent : UnityEvent<float, float> { }
[Serializable]
public class SprintInputEvent : UnityEvent<float> { }
[Serializable]
public class LookInputEvent: UnityEvent<float, float> { }
[Serializable]
public class RaiseCameraInputEvent : UnityEvent<float> { }
[Serializable]
public class UseCameraInputEvent : UnityEvent<float> { }
[Serializable]
public class InteractInputEvent : UnityEvent<float> {  }

public class InputController : MonoBehaviour
{
    Controls controls;
    [Header("Moving")]
    public MoveInputEvent moveInputEvent;
    [Header("Sprinting")]
    public SprintInputEvent sprintInputEvent;
    [Header("Looking")]
    public LookInputEvent lookInputEvent;
    [Header("RaiseCamera")]
    public RaiseCameraInputEvent raiseCameraInputEvent;
    [Header("UseCamera")]
    public UseCameraInputEvent useCameraInputEvent;
    [Header("Interact")]
    public InteractInputEvent interactInputEvent;

    private void Awake()
    {
        controls = new Controls();
    }

    private void OnEnable()
    {
        // enable the Gameplay action map
        controls.Gameplay.Enable();


        controls.Gameplay.Move.performed += OnMovePerformed;
        controls.Gameplay.Sprint.performed += OnSprintPerformed;

        // stop moving when key/stick released
        controls.Gameplay.Move.canceled += OnMovePerformed;


        controls.Gameplay.Look.performed += OnLookPerformed;
        // stop moving camera when key/stick released
        controls.Gameplay.Look.canceled += OnLookPerformed;

        controls.Gameplay.RaiseCamera.performed += OnRaiseCameraPerformed;
        controls.Gameplay.RaiseCamera.canceled += OnRaiseCameraPerformed;

        controls.Gameplay.UseCamera.performed += OnUseCameraPerformed;
        //controls.Gameplay.UseCamera.canceled += OnUseCameraPerformed;

        controls.Gameplay.Interact.performed += OnInteractPerformed;
    }
    private void OnDisable()
    {
        controls.Gameplay.Disable();
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

    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        float sprintInput = context.ReadValue<float>();

        // invoke event associated with sprint using input value
        sprintInputEvent.Invoke(sprintInput);
    }   

    private void OnRaiseCameraPerformed(InputAction.CallbackContext context)
    {
        float raiseCamInput = context.ReadValue<float>();
        raiseCameraInputEvent.Invoke(raiseCamInput);
    }

    private void OnUseCameraPerformed(InputAction.CallbackContext context)
    {
        float useCamInput = context.ReadValue<float>();
        useCameraInputEvent.Invoke(useCamInput);
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        float interactInput = context.ReadValue<float>();
        interactInputEvent.Invoke(interactInput);
    }

}
