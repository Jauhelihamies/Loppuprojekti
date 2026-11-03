using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [Header("Wheel Colliders")]
    [SerializeField] private WheelCollider frontLeftWheel;
    [SerializeField] private WheelCollider frontRightWheel;
    [SerializeField] private WheelCollider rearLeftWheel;
    [SerializeField] private WheelCollider rearRightWheel;

    [Header("Wheel Transforms (Visuaaliset renkaat)")]
    [SerializeField] private Transform frontLeftTransform;
    [SerializeField] private Transform frontRightTransform;
    [SerializeField] private Transform rearLeftTransform;
    [SerializeField] private Transform rearRightTransform;

    [Header("Auton asetukset")]
    [SerializeField] private float motorForce = 3000f;
    [SerializeField] private float boostForce = 10000f;
    [SerializeField] private float brakeForce = 2500f; // Huijauksessa tämä saa olla esim. 1000-5000, koska se on suoraa työntövoimaa!
    [SerializeField] private float maxSteerAngle = 30f;

    // Input-muuttujat
    private Vector2 moveInput;
    private bool isBoosting;
    private bool isBraking;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Varmistetaan, että WheelColliderien omat fysiikkajarrut eivät ikinä sotke autoa
        frontLeftWheel.brakeTorque = 0f;
        frontRightWheel.brakeTorque = 0f;
        rearLeftWheel.brakeTorque = 0f;
        rearRightWheel.brakeTorque = 0f;
    }

    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
    public void OnBoost(InputValue value) { isBoosting = value.isPressed; }
    public void OnBrake(InputValue value) { isBraking = value.isPressed; }

    private void FixedUpdate()
    {
        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    private void HandleMotor()
    {
        // 1. JARRUTUS (Jarru painettuna)
        if (isBraking)
        {
            // Katkaistaan moottorin veto kokonaan jarrutuksen ajaksi
            rearLeftWheel.motorTorque = 0f;
            rearRightWheel.motorTorque = 0f;

            if (rb != null)
            {
                // Katsotaan auton paikallinen nopeus eteen/taakse suunnassa
                Vector3 localVelocity = transform.InverseTransformDirection(rb.linearVelocity);

                // Jos auto liikkuu vielä eteen tai taakse yli pienen kynnyksen
                if (Mathf.Abs(localVelocity.z) > 0.1f)
                {
                    // Työnnetään autoa fysiikalla tasan vastakkaiseen suuntaan
                    float reverseDirection = -Mathf.Sign(localVelocity.z);
                    rb.AddForce(transform.forward * reverseDirection * brakeForce, ForceMode.Force);
                }
                else
                {
                    // Jos auto on melkein pysähtynyt, nollataan nopeus kokonaan ettei se ala täristä paikoillaan
                    localVelocity.z = 0f;
                    rb.linearVelocity = transform.TransformDirection(localVelocity);
                }
            }
            return; // Hypätään ulos jarrutuksen aikana
        }

        // 2. NORMAALI KAASU JA BOOST (Kun jarru EI ole pohjassa)

        // Pakotetaan Rigidbody hereille heti kun jarru vapautetaan, jotta fysiikkamoottori ei jää jumiin
        if (rb != null && rb.IsSleeping())
        {
            rb.WakeUp();
        }

        float torque = moveInput.y * motorForce;
        if (isBoosting && moveInput.y > 0)
        {
            torque += boostForce;
        }

        // KORJAUS JUMIUTUMISEEN: Jos kaasua ei paineta, syötetään renkaile olematon "herätysvoima" (0.001f)
        // sen sijaan että syötettäisiin puhdas nolla. Tämä estää fysiikkamoottorin renkaiden jäätymisen.
        if (moveInput.y == 0)
        {
            // Hyvin pieni arvo riittää pitämään WheelColliderit aktiivisina taustalla ilman että auto liikkuu
            rearLeftWheel.motorTorque = 0.001f;
            rearRightWheel.motorTorque = 0.001f;
        }
        else
        {
            // Syötetään normaali voima takarenkaille
            rearLeftWheel.motorTorque = torque;
            rearRightWheel.motorTorque = torque;
        }
    }

    private void HandleSteering()
    {
        float currentSteerAngle = moveInput.x * maxSteerAngle;
        frontLeftWheel.steerAngle = currentSteerAngle;
        frontRightWheel.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheel, frontLeftTransform);
        UpdateSingleWheel(frontRightWheel, frontRightTransform);
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
