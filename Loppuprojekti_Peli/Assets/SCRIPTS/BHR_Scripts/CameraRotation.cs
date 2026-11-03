using UnityEngine;

public class CinematicGarageCamera : MonoBehaviour
{
    [Header("Target settings")]
    public Transform target;            // The car or object to focus on

    [Header("Shot Duration")]
    public float minShotDuration = 4.0f; // Minimum time before changing angles
    public float maxShotDuration = 7.0f; // Maximum time before changing angles

    [Header("Orbit Limits")]
    public float minDistance = 4.0f;    // Closest the camera can get
    public float maxDistance = 7.0f;    // Farthest the camera can get
    public float minHeight = 0.5f;      // Lowest camera height (ground look-up)
    public float maxHeight = 2.5f;      // Highest camera height (top-down look)
    public float panSpeed = 8.0f;       // Speed of the cinematic slow pan

    private float shotTimer;
    private float currentShotDuration;
    private float currentAngle;
    private float currentDistance;
    private float currentHeight;
    private int panDirection = 1;       // 1 for clockwise, -1 for counter-clockwise

    void Start()
    {
        if (target == null) return;
        GenerateNewShot();
    }

    void Update()
    {
        if (target == null) return;

        // Count down until the next camera cut
        shotTimer += Time.deltaTime;
        if (shotTimer >= currentShotDuration)
        {
            GenerateNewShot();
        }

        // Apply the slow, cinematic orbital rotation
        currentAngle += panSpeed * panDirection * Time.deltaTime;

        // Calculate the position based on the active shot's parameters
        float rad = currentAngle * Mathf.Deg2Rad;
        float x = target.position.x + Mathf.Cos(rad) * currentDistance;
        float z = target.position.z + Mathf.Sin(rad) * currentDistance;
        float y = target.position.y + currentHeight;

        // Apply position and force camera to look at the center of the object
        transform.position = new Vector3(x, y, z);
        transform.LookAt(target.position + Vector3.up * 0.5f); // Looks slightly up from pivot point
    }

    void GenerateNewShot()
    {
        shotTimer = 0f;
        // Randomize how long this specific shot lasts
        currentShotDuration = Random.Range(minShotDuration, maxShotDuration);

        // Instant camera cut: choose a completely new random starting angle around the car
        currentAngle = Random.Range(0f, 360f);

        // Randomize distance and height for a distinct camera perspective
        currentDistance = Random.Range(minDistance, maxDistance);
        currentHeight = Random.Range(minHeight, maxHeight);

        // Randomize pan direction so it's not always orbiting the same way
        panDirection = Random.value > 0.5f ? 1 : -1;
    }
}
