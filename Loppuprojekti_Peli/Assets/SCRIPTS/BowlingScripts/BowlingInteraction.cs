using UnityEngine;
using System.Collections.Generic;

public class BowlingInteraction : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private MonoBehaviour playerMovementScript; // Drag your player movement script here
    [SerializeField] private Camera mainPlayerCamera;            // Your regular player camera
    [SerializeField] private Camera bowlingCamera;               // The camera facing the lane
    [SerializeField] private BowlingSystemNew bowlingSystemScript;   // Drag the bowling ball script here

    [Header("Pin Reset Configuration")]
    [SerializeField] private List<GameObject> pinObjects;        // Drag all 10 pin GameObjects here

    private List<Vector3> initialPinPositions = new List<Vector3>();
    private List<Quaternion> initialPinRotations = new List<Quaternion>();

    private bool isPlayerNear = false;
    private bool isBowling = false;

    private void Start()
    {
        // Ensure bowling system starts turned off
        if (bowlingSystemScript != null) bowlingSystemScript.enabled = false;
        if (bowlingCamera != null) bowlingCamera.enabled = false;

        // Save the starting positions and rotations of all pins so we can reset them perfectly
        foreach (GameObject pin in pinObjects)
        {
            if (pin != null)
            {
                initialPinPositions.Add(pin.transform.position);
                initialPinRotations.Add(pin.transform.rotation);
            }
        }
    }

    private void Update()
    {
        // Using the New Input System keyboard check for the 'E' key
        if (isPlayerNear && !isBowling && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartBowling();
        }
    }

    public void StartBowling()
    {
        isBowling = true;

        // 1. Disable player movement so they can't walk away
        if (playerMovementScript != null) playerMovementScript.enabled = false;

        // 2. Swap cameras to look at the lane
        if (mainPlayerCamera != null) mainPlayerCamera.enabled = false;
        if (bowlingCamera != null) bowlingCamera.enabled = true;

        // 3. Enable the throwing/aiming system
        if (bowlingSystemScript != null)
        {
            bowlingSystemScript.enabled = true;
            bowlingSystemScript.InitializeBowling(this);
        }
    }

    public void StopBowling()
    {
        isBowling = false;

        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (mainPlayerCamera != null) mainPlayerCamera.enabled = true;
        if (bowlingCamera != null) bowlingCamera.enabled = false;
        if (bowlingSystemScript != null) bowlingSystemScript.enabled = false;
    }

    // Call this to count the standing pins and reset the lane for a new turn
    public void EndTurnAndReset()
    {
        int knockedPins = 0;

        for (int i = 0; i < pinObjects.Count; i++)
        {
            if (pinObjects[i] == null) continue;

            // Check if the pin is knocked over by measuring its tilt (angle relative to upright Vector3.up)
            float angle = Quaternion.Angle(pinObjects[i].transform.rotation, initialPinRotations[i]);

            // If it's tilted more than 10 degrees, it counts as knocked down
            if (angle > 10f)
            {
                knockedPins++;
            }

            // Reset the pin's position, rotation, and physics states
            Rigidbody pinRb = pinObjects[i].GetComponent<Rigidbody>();
            if (pinRb != null)
            {
                pinRb.linearVelocity = Vector3.zero;
                pinRb.angularVelocity = Vector3.zero;
            }
            pinObjects[i].transform.position = initialPinPositions[i];
            pinObjects[i].transform.rotation = initialPinRotations[i];
        }

        Debug.Log($"🎳 Turn complete! You knocked down {knockedPins} out of {pinObjects.Count} pins!");

        // Put the player right back into the aiming screen for the next throw
        if (isBowling && bowlingSystemScript != null)
        {
            bowlingSystemScript.InitializeBowling(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNear = false;
    }
}