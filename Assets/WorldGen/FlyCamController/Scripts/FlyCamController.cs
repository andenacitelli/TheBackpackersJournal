using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class FlyCamController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float moveSmoothing = 0.2f;
    private Vector3 smoothingVelocity;
    public float mouseSensitivity = 2.0f;
    public float mouseSmoothing = 2.0f;

    private float hLookRotation;
    private float vLookRotation;

    private float xRotation;
    private float yRotation;

    private Vector3 lookRotation;

    private CharacterController cc;
    private Transform cameraTransform;

    private Vector3 moveAmount;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //Looking
        hLookRotation += Input.GetAxis("Mouse X") * mouseSensitivity;

        vLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivity;
        vLookRotation = Mathf.Clamp(vLookRotation, -90, 90);

        xRotation = Mathf.Lerp(xRotation, vLookRotation, mouseSmoothing * Time.deltaTime);
        yRotation = Mathf.Lerp(yRotation, hLookRotation, mouseSmoothing * Time.deltaTime);


        lookRotation = new Vector3(-xRotation, yRotation, 0);

        cameraTransform.localRotation = Quaternion.Euler(lookRotation);


        //Movement
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Lateral"));
        moveDir = cameraTransform.TransformDirection(moveDir);
        moveDir += new Vector3(0, Input.GetAxisRaw("Vertical"), 0); // Don't want this to be relative to camera 
        moveDir.Normalize();
        Vector3 targetMoveAmount = moveDir * moveSpeed;

        moveAmount = Vector3.SmoothDamp(moveAmount, targetMoveAmount, ref smoothingVelocity, moveSmoothing);



        cc.Move(moveAmount * Time.deltaTime);
    }

}
