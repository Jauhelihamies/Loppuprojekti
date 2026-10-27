using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// The class name now matches your file name perfectly!
public class BHR_Vehicle : MonoBehaviour
{
    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentbreakForce;
    private bool isBreaking;

    [Header("Driving Settings")]
    [SerializeField] private float motorForce = 1500f;
    [SerializeField] private float brakeForce = 3000f;
    [SerializeField] private float maxSteerAngle = 30f;

    [Header("Camera Settings")]
    [Tooltip("The script will try to find Camera.main automatically, but you can drag your Main Camera here if needed.")]
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 3.5f, -7.5f); // Distance behind and above the car
    [SerializeField] private float cameraMoveSpeed = 5f; // How smoothly the camera catches up
    [SerializeField] private float cameraRotationSpeed = 5f; // How fast the camera turns to look at the car

    [Header("Assign Your 4 Wheel Objects Here")]
    [Tooltip("Drag the actual wheel game objects that have the Wheel Colliders on them.")]
    [SerializeField] private Transform frontLeftWheel;
    [SerializeField] private Transform frontRightWheel;
    [SerializeField] private Transform rearLeftWheel;
    [SerializeField] private Transform rearRightWheel;

    // Private references automatically found at startup
    private WheelCollider flCollider;
    private WheelCollider frCollider;
    private WheelCollider rlCollider;
    private WheelCollider rrCollider;

    private void Start()
    {
        // Automatically grab the colliders from the assigned wheel objects
        if (frontLeftWheel != null) flCollider = frontLeftWheel.GetComponent<WheelCollider>();
        if (frontRightWheel != null) frCollider = frontRightWheel.GetComponent<WheelCollider>();
        if (rearLeftWheel != null) rlCollider = rearLeftWheel.GetComponent<WheelCollider>();
        if (rearRightWheel != null) rrCollider = rearRightWheel.GetComponent<WheelCollider>();

        // Quick warning check to help you debug if a slot was missed
        if (flCollider == null || frCollider == null || rlCollider == null || rrCollider == null)
        {
            Debug.LogError("BHR_Vehicle: One or more assigned wheel objects are missing a Wheel Collider component!");
        }

        // Automatically find the Main Camera in your scene if the slot is left empty
        if (mainCameraTransform == null && Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        HandleCamera(); // Processes the camera movement right after the physics updates
    }

    private void GetInput()
    {
        if (Keyboard.current != null)
        {
            // Steering (A/D or Arrows)
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput = 1f;
            else horizontalInput = 0f;

            // Acceleration (W/S or Arrows)
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) verticalInput = 1f;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) verticalInput = -1f;
            else verticalInput = 0f;

            // Handbrake (Spacebar)
            isBreaking = Keyboard.current.spaceKey.isPressed;
        }
    }

    private void HandleMotor()
    {
        if (flCollider == null || frCollider == null) return;

        flCollider.motorTorque = verticalInput * motorForce;
        frCollider.motorTorque = verticalInput * motorForce;

        currentbreakForce = isBreaking ? brakeForce : 0f;
        ApplyBraking();
    }

    private void ApplyBraking()
    {
        if (flCollider == null || frCollider == null || rlCollider == null || rrCollider == null) return;

        flCollider.brakeTorque = currentbreakForce;
        frCollider.brakeTorque = currentbreakForce;
        rlCollider.brakeTorque = currentbreakForce;
        rrCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        if (flCollider == null || frCollider == null) return;

        currentSteerAngle = horizontalInput * maxSteerAngle;
        flCollider.steerAngle = currentSteerAngle;
        frCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(flCollider, frontLeftWheel);
        UpdateSingleWheel(frCollider, frontRightWheel);
        UpdateSingleWheel(rlCollider, rearLeftWheel);
        UpdateSingleWheel(rrCollider, rearRightWheel);
    }

    private void UpdateSingleWheel(WheelCollider collider, Transform visualTransform)
    {
        if (collider == null || visualTransform == null) return;

        Vector3 pos;
        Quaternion rot;

        collider.GetWorldPose(out pos, out rot);

        visualTransform.position = pos;
        visualTransform.rotation = rot;
    }

    private void HandleCamera()
    {
        if (mainCameraTransform == null) return;

        // 1. Calculate where the camera should want to step behind the car
        Vector3 targetCameraPosition = transform.TransformPoint(cameraOffset);

        // 2. Smoothly slide the camera position over time
        mainCameraTransform.position = Vector3.Lerp(mainCameraTransform.position, targetCameraPosition, cameraMoveSpeed * Time.fixedDeltaTime);

        // 3. Determine the direct line of sight to look at the center of the car
        Vector3 directionToCar = transform.position - mainCameraTransform.position;
        Quaternion targetCameraRotation = Quaternion.LookRotation(directionToCar, Vector3.up);

        // 4. Smoothly rotate the camera lens to face the car
        mainCameraTransform.rotation = Quaternion.Slerp(mainCameraTransform.rotation, targetCameraRotation, cameraRotationSpeed * Time.fixedDeltaTime);
    }
}