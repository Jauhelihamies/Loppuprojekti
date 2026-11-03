using UnityEngine;

public class ObjectShaker : MonoBehaviour
{
    [Header("Shake Settings")]
    [Tooltip("How fast the object vibrates.")]
    public float shakeSpeed = 15f;

    [Tooltip("How far the object moves from its original position.")]
    public Vector3 shakeAmount = new Vector3(0.05f, 0.05f, 0.05f);

    [Tooltip("Should the object also rotate dynamically?")]
    public bool shakeRotation = true;

    [Tooltip("How far the object rotates.")]
    public Vector3 rotationAmount = new Vector3(1f, 1f, 1f);

    // Internal tracking variables
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float seedX;
    private float seedY;
    private float seedZ;

    void Start()
    {
        // Save the starting positions so the object doesn't drift away
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

        // Generate random seeds so multiple parts don't shake identically
        seedX = Random.Range(0f, 100f);
        seedY = Random.Range(0f, 100f);
        seedZ = Random.Range(0f, 100f);
    }

    void Update()
    {
        // Progress time based on speed
        float time = Time.time * shakeSpeed;

        // Calculate smooth Perlin Noise offsets (-1 to 1 range)
        float noiseX = Mathf.PerlinNoise(seedX + time, 0f) * 2f - 1f;
        float noiseY = Mathf.PerlinNoise(0f, seedY + time) * 2f - 1f;
        float noiseZ = Mathf.PerlinNoise(seedZ, seedZ + time) * 2f - 1f;

        // Apply positional shake relative to starting position
        Vector3 posOffset = new Vector3(
            noiseX * shakeAmount.x,
            noiseY * shakeAmount.y,
            noiseZ * shakeAmount.z
        );
        transform.localPosition = originalPosition + posOffset;

        // Apply rotational shake if enabled
        if (shakeRotation)
        {
            Vector3 rotOffset = new Vector3(
                noiseY * rotationAmount.x, // Mixed seeds for organic variation
                noiseZ * rotationAmount.y,
                noiseX * rotationAmount.z
            );
            transform.localRotation = originalRotation * Quaternion.Euler(rotOffset);
        }
    }
}