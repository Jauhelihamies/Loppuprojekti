using System.Collections.Generic;
using UnityEngine;
// We import the new input system namespace here so Unity recognizes the keyboard classes
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class AdvancedCarController : MonoBehaviour
{
    [System.Serializable]
    public class CustomWheel
    {
        public Transform wheelTransform; // The visual wheel mesh container
        public bool isFrontWheel;        // Is this wheel used for steering?
        public bool isDriveWheel;        // Does this wheel receive engine torque?

        [HideInInspector] public float rotationAngle;
        [HideInInspector] public TrailRenderer skidTrail;
    }

    [Header("Drivetrain & Steering")]
    public List<CustomWheel> wheels = new List<CustomWheel>();
    public float engineTorque = 1500f;
    public float maxSteerAngle = 35f;
    public float brakeForce = 3000f;

    [Header("Suspension Physics")]
    public float suspensionRestLength = 0.5f;
    public float springStrength = 35000f;
    public float springDamper = 4000f;
    public float wheelRadius = 0.33f;

    [Header("Traction & Friction")]
    public float tireGripFactor = 0.8f;
    public float tireMass = 1.0f;

    [Header("Effects")]
    public GameObject skidMarkPrefab;

    private Rigidbody rb;
    private float throttleInput;
    private float steerInput;
    private bool isBraking;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Optional: Lower Center of Mass to prevent flipping over easily
        rb.centerOfMass += new Vector3(0, -0.5f, 0);

        // Setup skidmark trails dynamically per wheel if prefab exists
        if (skidMarkPrefab != null)
        {
            foreach (var wheel in wheels)
            {
                GameObject trailObj = Instantiate(skidMarkPrefab, wheel.wheelTransform);
                trailObj.transform.localPosition = Vector3.down * wheelRadius;
                wheel.skidTrail = trailObj.GetComponent<TrailRenderer>();
                if (wheel.skidTrail != null) wheel.skidTrail.emitting = false;
            }
        }
    }

    void Update()
    {
        // Modernized Input Handling: Safely reads the keyboard package inputs natively
        if (Keyboard.current != null)
        {
            var keyboard = Keyboard.current;

            // Calculate throttle input (W / UpArrow = 1, S / DownArrow = -1)
            float forward = keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f;
            float backward = keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f;
            throttleInput = forward - backward;

            // Calculate steering input (D / RightArrow = 1, A / LeftArrow = -1)
            float right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f;
            float left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f;
            steerInput = right - left;

            // Handbrake check (Spacebar)
            isBraking = keyboard.spaceKey.isPressed;
        }
    }

    void FixedUpdate()
    {
        foreach (var wheel in wheels)
        {
            // 1. Raycast Suspension Physics
            Vector3 tierWorldPos = wheel.wheelTransform.position;
            RaycastHit hit;

            // Cast down along the wheel's local down axis
            if (Physics.Raycast(tierWorldPos, -wheel.wheelTransform.up, out hit, suspensionRestLength + wheelRadius))
            {
                // Calculate spring compression force
                Vector3 springDir = wheel.wheelTransform.up;
                Vector3 tireVelocity = rb.GetPointVelocity(hit.point);

                float offset = suspensionRestLength - (hit.distance - wheelRadius);
                float vel = Vector3.Dot(springDir, tireVelocity);
                float force = (offset * springStrength) - (vel * springDamper);

                // Anti-Explosion Safeguard: Prevents extreme negative/positive spikes if spawning inside a collider
                force = Mathf.Clamp(force, 0f, springStrength * 2f);

                // Apply suspension forces up to the vehicle
                rb.AddForceAtPosition(springDir * force, hit.point);

                // Update visual position of wheel to match terrain displacement
                wheel.wheelTransform.position = tierWorldPos + (-wheel.wheelTransform.up * (hit.distance - wheelRadius));

                // 2. Track & Forward Forces (Driving / Braking)
                if (wheel.isDriveWheel && !isBraking)
                {
                    Vector3 forwardDir = wheel.wheelTransform.forward;
                    rb.AddForceAtPosition(forwardDir * throttleInput * (engineTorque / wheels.Count), hit.point);
                }
                else if (isBraking)
                {
                    Vector3 forwardDir = wheel.wheelTransform.forward;
                    float currentForwardVel = Vector3.Dot(forwardDir, tireVelocity);
                    rb.AddForceAtPosition(-forwardDir * Mathf.Sign(currentForwardVel) * brakeForce, hit.point);
                }

                // 3. Sideways Grip Friction (Prevents infinite sliding)
                Vector3 steeringDir = wheel.wheelTransform.right;
                float steeringVel = Vector3.Dot(steeringDir, tireVelocity);
                float desiredVelChange = -steeringVel * tireGripFactor;
                float desiredAccel = desiredVelChange / Time.fixedDeltaTime;

                rb.AddForceAtPosition(steeringDir * tireMass * desiredAccel, hit.point);

                // 4. Handle Skid Mark Logic based on lateral velocity thresholds
                if (wheel.skidTrail != null)
                {
                    wheel.skidTrail.emitting = isBraking || Mathf.Abs(steeringVel) > 4.5f;
                }
            }
            else
            {
                // If in mid-air, reset wheel position downward fully
                wheel.wheelTransform.position = tierWorldPos + (-wheel.wheelTransform.up * suspensionRestLength);
                if (wheel.skidTrail != null) wheel.skidTrail.emitting = false;
            }

            // 5. Turn Visuals & Steer Logic
            if (wheel.isFrontWheel)
            {
                wheel.wheelTransform.localRotation = Quaternion.Euler(wheel.rotationAngle, steerInput * maxSteerAngle, 0);
            }
            else
            {
                wheel.wheelTransform.localRotation = Quaternion.Euler(wheel.rotationAngle, 0, 0);
            }

            // Spin the tire visuals forward relative to speed
            float forwardVel = Vector3.Dot(wheel.wheelTransform.forward, rb.GetPointVelocity(wheel.wheelTransform.position));
            wheel.rotationAngle += (forwardVel / wheelRadius) * Time.fixedDeltaTime * Mathf.Rad2Deg;
        }
    }
}