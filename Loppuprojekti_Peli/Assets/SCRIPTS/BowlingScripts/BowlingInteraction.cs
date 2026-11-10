using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class BowlingInteraction : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private Camera mainPlayerCamera;
    [SerializeField] private Camera bowlingCamera;
    [SerializeField] private BowlingSystemNew bowlingSystemScript;

    [Header("Pin Configuration")]
    [SerializeField] private List<GameObject> pinObjects;

    [Header("UI Image Feedback")]
    [SerializeField] private Image displayImageHolder;
    [SerializeField] private float imageDisplayDuration = 2.5f;

    [Header("Animation Settings")]
    [SerializeField] private float popInSpeed = 10f;
    [SerializeField] private float fadeOutSpeed = 2f;

    [Header("Sprite Assets (Pictures)")]
    [SerializeField] private Sprite gutterballSprite;
    [SerializeField] private Sprite strikeSprite;
    [SerializeField] private Sprite clearSprite;

    [Tooltip("Add 10 elements here. Element 0 = 1-Count, Element 9 = 10-Count")]
    [SerializeField] private List<Sprite> countSprites;

    private List<Vector3> initialPinPositions = new List<Vector3>();
    private List<Quaternion> initialPinRotations = new List<Quaternion>();

    private bool isPlayerNear = false;
    private bool isBowling = false;
    private int currentTurnCount = 0;
    private const int maxTurnsPerFrame = 3;

    private Coroutine activeAnimationCoroutine;

    private void Start()
    {
        if (bowlingSystemScript != null) bowlingSystemScript.enabled = false;
        if (bowlingCamera != null) bowlingCamera.enabled = false;

        if (displayImageHolder != null) displayImageHolder.gameObject.SetActive(false);

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
        if (isPlayerNear && !isBowling && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartBowling();
        }

        if (isBowling && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StopBowling();
        }
    }

    public void StartBowling()
    {
        isBowling = true;
        currentTurnCount = 0;

        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (mainPlayerCamera != null) mainPlayerCamera.enabled = false;
        if (bowlingCamera != null) bowlingCamera.enabled = true;

        ResetAllTenPins();

        if (bowlingSystemScript != null)
        {
            bowlingSystemScript.enabled = true;
            bowlingSystemScript.InitializeBowling(this);
        }
    }

    public void StopBowling()
    {
        isBowling = false;

        if (activeAnimationCoroutine != null) StopCoroutine(activeAnimationCoroutine);
        if (displayImageHolder != null) displayImageHolder.gameObject.SetActive(false);

        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (mainPlayerCamera != null) mainPlayerCamera.enabled = true;
        if (bowlingCamera != null) bowlingCamera.enabled = false;
        if (bowlingSystemScript != null) bowlingSystemScript.enabled = false;
    }

    public void EndTurnAndReset()
    {
        currentTurnCount++;

        int totalKnockedThisThrow = 0;
        int totalRemainingStandingOnDeck = 0;

        // One clean pass to check pin states
        for (int i = 0; i < pinObjects.Count; i++)
        {
            if (pinObjects[i] == null) continue;
            if (!pinObjects[i].activeSelf) continue;

            float angle = Quaternion.Angle(pinObjects[i].transform.rotation, initialPinRotations[i]);

            if (angle > 10f)
            {
                totalKnockedThisThrow++;
                pinObjects[i].SetActive(false);
            }
            else
            {
                totalRemainingStandingOnDeck++;

                Rigidbody pinRb = pinObjects[i].GetComponent<Rigidbody>();
                if (pinRb != null)
                {
                    pinRb.linearVelocity = Vector3.zero;
                    pinRb.angularVelocity = Vector3.zero;
                    pinRb.Sleep();
                }

                pinObjects[i].transform.position = initialPinPositions[i];
                pinObjects[i].transform.rotation = initialPinRotations[i];
            }
        }

        DisplayImageReaction(totalKnockedThisThrow, totalRemainingStandingOnDeck == 0);

        if (totalRemainingStandingOnDeck == 0)
        {
            ResetAllTenPins();
            currentTurnCount = 0;
        }
        else if (currentTurnCount >= maxTurnsPerFrame)
        {
            ResetAllTenPins();
            currentTurnCount = 0;
        }

        if (isBowling && bowlingSystemScript != null)
        {
            bowlingSystemScript.InitializeBowling(this);
        }
    }

    private void DisplayImageReaction(int knockedCount, bool allCleared)
    {
        if (displayImageHolder == null) return;

        if (activeAnimationCoroutine != null)
        {
            StopCoroutine(activeAnimationCoroutine);
        }

        Sprite selectedSprite = null;

        if (knockedCount == 0)
        {
            selectedSprite = gutterballSprite;
        }
        else if (allCleared && currentTurnCount == 1)
        {
            selectedSprite = strikeSprite;
        }
        else if (allCleared)
        {
            selectedSprite = clearSprite;
        }
        else
        {
            int spriteIndex = knockedCount - 1;
            if (countSprites != null && spriteIndex >= 0 && spriteIndex < countSprites.Count)
            {
                selectedSprite = countSprites[spriteIndex];
            }
        }

        if (selectedSprite != null)
        {
            displayImageHolder.sprite = selectedSprite;
            activeAnimationCoroutine = StartCoroutine(JuicyImageAnimationRoutine());
        }
        else
        {
            displayImageHolder.gameObject.SetActive(false);
        }
    }

    private IEnumerator JuicyImageAnimationRoutine()
    {
        displayImageHolder.gameObject.SetActive(true);

        Color imgColor = displayImageHolder.color;
        imgColor.a = 1f;
        displayImageHolder.color = imgColor;
        displayImageHolder.transform.localScale = Vector3.zero;

        float timer = 0f;
        Vector3 finalNaturalScale = new Vector3(4f, 4f, 4f);
        Vector3 targetPopScale = new Vector3(5.2f, 5.2f, 5.2f);

        while (timer < 1f)
        {
            timer += Time.deltaTime * popInSpeed;
            displayImageHolder.transform.localScale = Vector3.Lerp(Vector3.zero, targetPopScale, timer);
            yield return null;
        }

        timer = 0f;
        while (timer < 1f)
        {
            timer += Time.deltaTime * (popInSpeed * 1.5f);
            displayImageHolder.transform.localScale = Vector3.Lerp(targetPopScale, finalNaturalScale, timer);
            yield return null;
        }
        displayImageHolder.transform.localScale = finalNaturalScale;

        yield return new WaitForSeconds(imageDisplayDuration);

        timer = 0f;
        Vector3 startingScale = displayImageHolder.transform.localScale;
        while (timer < 1f)
        {
            timer += Time.deltaTime * fadeOutSpeed;
            imgColor.a = Mathf.Lerp(1f, 0f, timer);
            displayImageHolder.color = imgColor;
            displayImageHolder.transform.localScale = Vector3.Lerp(startingScale, finalNaturalScale * 0.8f, timer);
            yield return null;
        }

        displayImageHolder.gameObject.SetActive(false);
    }

    private void ResetAllTenPins()
    {
        for (int i = 0; i < pinObjects.Count; i++)
        {
            if (pinObjects[i] == null) continue;

            pinObjects[i].SetActive(true);

            Rigidbody pinRb = pinObjects[i].GetComponent<Rigidbody>();
            if (pinRb != null)
            {
                pinRb.linearVelocity = Vector3.zero;
                pinRb.angularVelocity = Vector3.zero;
                pinRb.Sleep();
            }

            pinObjects[i].transform.position = initialPinPositions[i];
            pinObjects[i].transform.rotation = initialPinRotations[i];
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
    }
}