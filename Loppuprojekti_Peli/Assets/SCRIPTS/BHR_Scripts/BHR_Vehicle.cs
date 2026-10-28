using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BHR_Vehicle : MonoBehaviour
{
    private float horizontalInput;
    private float verticalInput;
    private float currentSteerAngle;
    private float currentBrakeForce; // Fixed typo 'break' -> 'brake'
    private bool isBraking;          // Fixed typo 'isBreaking' -> 'isBraking'
    private Rigidbody rb;

    [Header("Driving Settings")]
    [SerializeField] private float motorForce = 1500f; // Boosted for WheelCollider realistic defaults
    [SerializeField] private float brakeForce = 5000f; // Boosted for heavy physics clamping
    [SerializeField] private float maxSteerAngle = 30f;

    [Header("Anti-Flip Settings")]
    [Tooltip("How aggressively the car forces itself to stand upright. Recommended: 50 to 200")]
    [SerializeField] private float antiFlipStabilitySpeed = 100f;
    [Tooltip("Damping to prevent the car from oscillating or spinning out of control. Recommended: 10 to 30")]
    [SerializeField] private float antiFlipDamping = 20f;
    [Tooltip("If the car flips completely upside down on its head, how fast should it force snap back up?")]
    [SerializeField] private float completelyUpsideDownResetSpeed = 30f;

    [Header("Camera Settings")]
    [SerializeField] private Transform mainCameraTransform;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 3.5f, -7.5f);
    [SerializeField] private float cameraMoveSpeed = 5f;

    [Header("Assign Wheel Colliders Here")]
    [SerializeField] private WheelCollider flCollider;
    [SerializeField] private WheelCollider frCollider;
    [SerializeField] private WheelCollider rlCollider;
    [SerializeField] private WheelCollider rrCollider;

    [Header("Assign Visual Wheel Meshes Here")]
    [SerializeField] private Transform flVisualTransform;
    [SerializeField] private Transform frVisualTransform;
    [SerializeField] private Transform rlVisualTransform;
    [SerializeField] private Transform rrVisualTransform;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Low center of mass prevents instant flipping on tight turns
            rb.centerOfMass = new Vector3(0f, -0.8f, 0.5f);
        }
        else
        {
            Debug.LogError("BHR_Vehicle: No Rigidbody component found on this GameObject root!");
        }

        if (flCollider == null || frCollider == null || rlCollider == null || rrCollider == null)
        {
            Debug.LogError("BHR_Vehicle: One or more Wheel Colliders are not assigned in the inspector!");
        }

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
        ApplyAntiFlipStabilizer();
    }

    private void LateUpdate()
    {
        // Moved to LateUpdate to completely eliminate camera tracking jitter
        HandleCamera();
    }

    private void GetInput()
    {
        if (Keyboard.current != null)
        {
            // Horizontal Input
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontalInput = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontalInput = 1f;
            else
                horizontalInput = 0f;

            // Vertical Input
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                verticalInput = 1f;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                verticalInput = -1f;
            else
                verticalInput = 0f;

            // Fixed input sticking bug: captures state explicitly every frame
            isBraking = Keyboard.current.spaceKey.isPressed;
        }
    }

    private void HandleMotor()
    {
        if (flCollider == null || frCollider == null) return;

        flCollider.motorTorque = verticalInput * motorForce;
        frCollider.motorTorque = verticalInput * motorForce;

        currentBrakeForce = isBraking ? brakeForce : 0f;
        ApplyBraking();
    }

    private void ApplyBraking()
    {
        if (flCollider == null || frCollider == null || rlCollider == null || rrCollider == null) return;

        flCollider.brakeTorque = currentBrakeForce;
        frCollider.brakeTorque = currentBrakeForce;
        rlCollider.brakeTorque = currentBrakeForce;
        rrCollider.brakeTorque = currentBrakeForce;
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
        UpdateSingleWheel(flCollider, flVisualTransform);
        UpdateSingleWheel(frCollider, frVisualTransform);
        UpdateSingleWheel(rlCollider, rlVisualTransform);
        UpdateSingleWheel(rrCollider, rrVisualTransform);
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

    private void ApplyAntiFlipStabilizer()
    {
        if (rb == null) return;

        // 1. Hard reset if upside down
        if (Vector3.Dot(transform.up, Vector3.down) > 0.4f)
        {
            Quaternion uprightTarget = Quaternion.LookRotation(transform.forward, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, uprightTarget, completelyUpsideDownResetSpeed * Time.fixedDeltaTime));
            return;
        }

        // 2. Stable stabilization torque
        Vector3 predictedUp = transform.up;
        Vector3 torqueCorrectionVector = Vector3.Cross(predictedUp, Vector3.up);
        Vector3 finalTorque = (torqueCorrectionVector * antiFlipStabilitySpeed) - (rb.angularVelocity * antiFlipDamping);

        rb.AddTorque(finalTorque, ForceMode.Acceleration);
    }

    private void HandleCamera()
    {
        if (mainCameraTransform == null) return;

        Vector3 targetCameraPosition = transform.TransformPoint(cameraOffset);
        mainCameraTransform.position = Vector3.Lerp(mainCameraTransform.position, targetCameraPosition, cameraMoveSpeed * Time.deltaTime);

        Vector3 forwardHeading = transform.forward;
        forwardHeading.y = 0f;

        if (forwardHeading != Vector3.zero)
        {
            mainCameraTransform.rotation = Quaternion.LookRotation(forwardHeading.normalized, Vector3.up);
        }
    }
}