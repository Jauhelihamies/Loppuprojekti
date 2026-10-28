using System.Collections;
using UnityEngine;

public class AnimateLever : MonoBehaviour
{
    [Header("Liikkeen korkeus (Paikallinen Y-akseli)")]
    public float upLocalY = 0.5f;
    public float downLocalY = -0.5f;

    [Header("Vivun asetukset")]
    public float animationSpeed = 5f;
    [Range(0f, 1f)] public float bounceIntensity = 0.3f;
    public float bounceSpeed = 15f;

    private bool isUp = false;
    private Coroutine movementCoroutine;
    private Vector3 startingLocalPosition;
    private bool isInitialized = false;

    private void Awake()
    {
        InitializePosition();
    }

    private void InitializePosition()
    {
        if (isInitialized) return;
        startingLocalPosition = transform.localPosition;
        isInitialized = true;
    }

    // Tätä kutsutaan managerista, kun pelaaja klikkaa vipua!
    public void ForceLeverVisualState(bool targetUpState)
    {
        isUp = targetUpState;
        StartMovement(isUp ? upLocalY : downLocalY);
    }

    // Tätä kutsutaan managerin Start-funktiossa pelin alussa (asettuu ilman animaatiota)
    public void SetInitialState(bool startUp)
    {
        InitializePosition();

        isUp = startUp;
        float targetY = isUp ? upLocalY : downLocalY;

        transform.localPosition = new Vector3(startingLocalPosition.x, targetY, startingLocalPosition.z);
    }

    private void StartMovement(float targetY)
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine = StartCoroutine(MoveLeverRoutine(targetY));
    }

    private IEnumerator MoveLeverRoutine(float targetY)
    {
        float currentY = transform.localPosition.y;

        while (Mathf.Abs(currentY - targetY) > 0.001f)
        {
            currentY = Mathf.MoveTowards(currentY, targetY, animationSpeed * Time.deltaTime);
            transform.localPosition = new Vector3(startingLocalPosition.x, currentY, startingLocalPosition.z);
            yield return null;
        }

        float time = 0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime * bounceSpeed;
            float bounceOffset = Mathf.Sin(time * Mathf.PI * 2f) * bounceIntensity * (1f - time) * 0.1f;

            float finalY = targetY + bounceOffset;
            transform.localPosition = new Vector3(startingLocalPosition.x, finalY, startingLocalPosition.z);
            yield return null;
        }

        transform.localPosition = new Vector3(startingLocalPosition.x, targetY, startingLocalPosition.z);
    }
}
