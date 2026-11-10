using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BowlingSystemNew : MonoBehaviour
{
    public enum BowlingState { Aiming, Charging, Rolling }
    private BowlingState currentState;

    
    public BowlingState CurrentState => currentState;

    [Header("Aiming Settings")]
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private float maxAimAngle = 30f;
    [SerializeField] private GameObject aimArrow;

    [Header("Power Settings")]
    [SerializeField] private float minForce = 10f;
    [SerializeField] private float maxForce = 35f;
    [SerializeField] private float chargeSpeed = 1.5f;
    [SerializeField] private Slider powerBarSlider;

    [Header("Timing Settings")]
    [Tooltip("How long to wait after hitting the pit trigger before the game resets. Lower this value for faster resets!")]
    [SerializeField] private float resetDelayTime = 1.2f;

    private Rigidbody rb;
    private float currentForce;
    private bool chargingUp = true;
    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private float currentAimAngle = 0f;
    private BowlingInteraction interactionController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialRotation = transform.rotation;
        initialPosition = transform.position;

        if (powerBarSlider != null) powerBarSlider.gameObject.SetActive(false);
        if (aimArrow != null) aimArrow.SetActive(false);
    }

    public void InitializeBowling(BowlingInteraction controller)
    {
        interactionController = controller;

        
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        currentState = BowlingState.Aiming;
        currentAimAngle = 0f;

        if (aimArrow != null) aimArrow.SetActive(true);
        if (powerBarSlider != null)
        {
            powerBarSlider.gameObject.SetActive(true);
            powerBarSlider.value = 0f;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        switch (currentState)
        {
            case BowlingState.Aiming:
                HandleAiming(keyboard);
                break;

            case BowlingState.Charging:
                HandlePowerCharge(keyboard);
                break;

            case BowlingState.Rolling:
                break;
        }
    }

    private void HandleAiming(Keyboard keyboard)
    {
        float rotationInput = 0f;
        if (keyboard.aKey.isPressed) rotationInput = -1f;
        if (keyboard.dKey.isPressed) rotationInput = 1f;

        currentAimAngle += rotationInput * rotationSpeed * Time.deltaTime;
        currentAimAngle = Mathf.Clamp(currentAimAngle, -maxAimAngle, maxAimAngle);

        transform.rotation = initialRotation * Quaternion.Euler(0f, currentAimAngle, 0f);

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            currentState = BowlingState.Charging;
            currentForce = minForce;
            chargingUp = true;
            if (aimArrow != null) aimArrow.SetActive(false);
        }
    }

    private void HandlePowerCharge(Keyboard keyboard)
    {
        if (chargingUp)
        {
            currentForce += (maxForce - minForce) * chargeSpeed * Time.deltaTime;
            if (currentForce >= maxForce)
            {
                currentForce = maxForce;
                chargingUp = false;
            }
        }
        else
        {
            currentForce -= (maxForce - minForce) * chargeSpeed * Time.deltaTime;
            if (currentForce <= minForce)
            {
                currentForce = minForce;
                chargingUp = true;
            }
        }

        if (powerBarSlider != null)
        {
            powerBarSlider.value = Mathf.InverseLerp(minForce, maxForce, currentForce);
        }

        if (keyboard.spaceKey.wasReleasedThisFrame)
        {
            ThrowBall();
        }
    }

    private void ThrowBall()
    {
        currentState = BowlingState.Rolling;
        rb.isKinematic = false;

        if (powerBarSlider != null) powerBarSlider.gameObject.SetActive(false);

        rb.AddForce(transform.forward * currentForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PitTrigger" || other.CompareTag("PitTrigger"))
        {
            CancelInvoke(nameof(CallReset));
            Invoke(nameof(CallReset), resetDelayTime);
        }
    }

    private void CallReset()
    {
        if (interactionController != null)
        {
            interactionController.EndTurnAndReset();
        }
    }
}