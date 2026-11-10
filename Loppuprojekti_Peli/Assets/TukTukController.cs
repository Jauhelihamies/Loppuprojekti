using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class TukTukController : MonoBehaviour
{
    [Header("Wheel Colliders (3 kpl)")]
    [SerializeField] private WheelCollider frontWheel;
    [SerializeField] private WheelCollider rearLeftWheel;
    [SerializeField] private WheelCollider rearRightWheel;

    [Header("Wheel Transforms (Visuaaliset renkaat)")]
    [SerializeField] private Transform frontTransform;
    [SerializeField] private Transform rearLeftTransform;
    [SerializeField] private Transform rearRightTransform;

    [Header("Ratin kääntäminen")]
    [SerializeField] private Transform steeringHandlebar;
    [SerializeField] private float maxSteeringHandlebarAngle = 40f;

    [Header("Tuk-tukin fysiikka-asetukset")]
    [SerializeField] private float motorForce = 2000f;
    [SerializeField] private float boostForce = 6000f;
    [SerializeField] private float brakeForce = 2000f;
    [SerializeField] private float maxSteerAngle = 40f;
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0, -0.6f, 0);

    [Header("Ohjauksen pehmennys (Smoothing)")]
    [SerializeField] private float steeringSmoothSpeed = 5f; // Mitä suurempi arvo, sitä nopeammin ratti kääntyy (kokeile arvoja 3-10)

    [Header("Äänentoisto (Audio Sources)")]
    [SerializeField] private AudioSource effectsAudioSource;
    [SerializeField] private AudioSource engineLoopSource;

    [Header("Äänitiedostot (Audio Clips)")]
    [SerializeField] private AudioClip startClip1;
    [SerializeField] private AudioClip startClip2;
    [SerializeField] private AudioClip engineLoopClip;
    [SerializeField] private AudioClip boostClip;

    [Header("Äänen häivytyksen asetukset")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private float fadeTimeBeforeEnd = 0.8f;
    [SerializeField] private float maxEngineVolume = 0.7f;

    // Input-muuttujat
    private Vector2 moveInput;
    private bool isBoosting;
    private bool isBraking;
    private Rigidbody rb;

    // Sulavuuden seurantaan tarkoitettu muuttuja
    private float currentSteeringInterpolation = 0f;

    // Ääänilogiikan tilamuuttujat
    private bool isInputTriggered = false;
    private bool isStartingUp = false;
    private bool isEngineRunning = false;
    private bool isBoostSoundPlaying = false;

    // Ratin alkuperäinen asento
    private Quaternion steeringHandlebarStartRotation;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.centerOfMass += centerOfMassOffset;
        }

        frontWheel.brakeTorque = 0f;
        rearLeftWheel.brakeTorque = 0f;
        rearRightWheel.brakeTorque = 0f;

        if (steeringHandlebar != null)
        {
            steeringHandlebarStartRotation = steeringHandlebar.localRotation;
        }

        if (engineLoopSource != null)
        {
            engineLoopSource.volume = 0f;
        }
    }

    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }

    public void OnBoost(InputValue value)
    {
        isBoosting = value.isPressed;

        if (isBoosting && isEngineRunning && !isBoostSoundPlaying && moveInput.y > 0)
        {
            StartCoroutine(PlayBoostSound());
        }
    }

    public void OnBrake(InputValue value) { isBraking = value.isPressed; }

    private void FixedUpdate()
    {
        if (!isInputTriggered && (moveInput.x != 0 || moveInput.y != 0))
        {
            isInputTriggered = true;
            StartCoroutine(StartEngineSequence());
        }

        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void HandleSteering()
    {
        // Interpoloidaan (pehmennetään) ohjausarvoa nykyisestä kohti pelaajan syötettä ajan kuluessa
        currentSteeringInterpolation = Mathf.MoveTowards(
            currentSteeringInterpolation,
            moveInput.x,
            steeringSmoothSpeed * Time.fixedDeltaTime
        );

        // Fyysinen renkaan kääntäminen pehmennetyllä arvolla
        float targetSteerAngle = currentSteeringInterpolation * maxSteerAngle;
        frontWheel.steerAngle = targetSteerAngle;

        // VISUAALINEN RATIN KÄÄNTÄMINEN pehmennetyllä arvolla
        if (steeringHandlebar != null)
        {
            float handlebarRotationZ = currentSteeringInterpolation * maxSteeringHandlebarAngle;
            Quaternion turnRotation = Quaternion.Euler(0f, 0f, handlebarRotationZ);
            steeringHandlebar.localRotation = steeringHandlebarStartRotation * turnRotation;
        }
    }

    private IEnumerator StartEngineSequence()
    {
        isStartingUp = true;
        AudioClip chosenStartClip = Random.value > 0.5f ? startClip1 : startClip2;

        if (chosenStartClip != null && effectsAudioSource != null)
        {
            effectsAudioSource.clip = chosenStartClip;
            effectsAudioSource.Play();

            float waitTimeBeforeFade = Mathf.Max(0f, chosenStartClip.length - fadeTimeBeforeEnd);
            yield return new WaitForSeconds(waitTimeBeforeFade);
        }

        if (engineLoopClip != null && engineLoopSource != null)
        {
            engineLoopSource.clip = engineLoopClip;
            engineLoopSource.volume = 0f;
            engineLoopSource.Play();

            float currentTime = 0f;
            while (currentTime < fadeDuration)
            {
                currentTime += Time.deltaTime;
                engineLoopSource.volume = Mathf.Lerp(0f, maxEngineVolume, currentTime / fadeDuration);
                yield return null;
            }
            engineLoopSource.volume = maxEngineVolume;
        }

        isStartingUp = false;
        isEngineRunning = true;
    }

    private IEnumerator PlayBoostSound()
    {
        isBoostSoundPlaying = true;

        if (boostClip != null && effectsAudioSource != null)
        {
            effectsAudioSource.PlayOneShot(boostClip);
            yield return new WaitForSeconds(boostClip.length);
        }

        isBoostSoundPlaying = false;
    }

    private void HandleMotor()
    {
        if (!isInputTriggered || isStartingUp)
        {
            rearLeftWheel.motorTorque = 0.001f;
            rearRightWheel.motorTorque = 0.001f;
            return;
        }

        if (isBraking)
        {
            rearLeftWheel.motorTorque = 0f;
            rearRightWheel.motorTorque = 0f;

            if (rb != null)
            {
                Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);

                if (Mathf.Abs(localVelocity.z) > 0.1f)
                {
                    float reverseDirection = -Mathf.Sign(localVelocity.z);
                    rb.AddForce(transform.forward * reverseDirection * brakeForce, ForceMode.Force);
                }
                else
                {
                    localVelocity.z = 0f;
                    rb.linearVelocity = transform.TransformDirection(localVelocity);
                }
            }
            return;
        }

        if (rb != null && rb.IsSleeping())
        {
            rb.WakeUp();
        }

        float torque = moveInput.y * motorForce;
        if (isBoosting && moveInput.y > 0)
        {
            torque += boostForce;
        }

        if (moveInput.y == 0)
        {
            rearLeftWheel.motorTorque = 0.001f;
            rearRightWheel.motorTorque = 0.001f;
        }
        else
        {
            rearLeftWheel.motorTorque = torque;
            rearRightWheel.motorTorque = torque;
        }
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontWheel, frontTransform);
        UpdateSingleWheel(rearLeftWheel, rearLeftTransform);
        UpdateSingleWheel(rearRightWheel, rearRightTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        if (wheelTransform == null) return;
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.position = pos;
        wheelTransform.rotation = rot;
    }
}
