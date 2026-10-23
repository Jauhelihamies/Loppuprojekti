using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMachine : MonoBehaviour
{
    [Header("Kammen ja jousen asetukset")]
    public Transform crankTransform;
    public Vector3 crankRotationAxis = new Vector3(0, 0, 1);
    public float crankRotationDegrees = 180f;
    public Transform springTransform;
    public Vector3 springRotationAxis = new Vector3(0, 1, 0);
    public float springRotationDegrees = 180f;

    [Header("Spawner & Hissi asetukset")]
    [Tooltip("Lista pallojen Prefabeista. Kone käy nämä läpi järjestyksessä kerran ja lopettaa toiminnan.")]
    public List<GameObject> ballPrefabs = new List<GameObject>();

    [Tooltip("Piste, josta pallo lähtee liikkeelle (esim. hissin aloitussijainti)")]
    public Transform spawnPoint;
    [Tooltip("Piste, johon pallo nousee ennen laukaisua (esim. yläluukku)")]
    public Transform targetPoint;

    [Header("Koon muutos (Skaalaus)")]
    [Tooltip("Mistä koosta pallo lähtee liikkeelle matkaan (0 = näkymätön alussa)")]
    public Vector3 startingScale = Vector3.zero;
    [Tooltip("Mikä on pallon lopullinen koko kun se saavuttaa yläpisteen")]
    public Vector3 targetScale = new Vector3(1f, 1f, 1f);

    [Header("Laukaisun asetukset")]
    [Tooltip("Mihin suuntaan pallo laukaistaan ulos koneesta?")]
    public Vector3 launchDirection = new Vector3(0, 0, 1);
    [Tooltip("Kuinka suurella voimalla pallo tönäistään ulos luukusta")]
    public float launchForce = 5f;

    [Header("Yleiset asetukset")]
    public float duration = 0.8f;

    private bool isOperating = false;
    private bool isFinished = false; // Pitää kirjaa, onko lista jo suoritettu loppuun
    private int currentBallIndex = 0;

    public void TriggerMachine()
    {
        // Jos kone on jo käynnissä tai lista on käyty kerran läpi, ei tehdä mitään
        if (isOperating || isFinished) return;

        if (ballPrefabs == null || ballPrefabs.Count == 0 || spawnPoint == null || targetPoint == null)
        {
            Debug.LogError("Muista lisätä vähintään yksi pallo Ball Prefabs -listaan sekä asettaa Spawn ja Target Pointit Inspectorissa!");
            return;
        }

        StartCoroutine(AnimateMachineAndSpawnRoutine());
    }

    private IEnumerator AnimateMachineAndSpawnRoutine()
    {
        isOperating = true;

        // Varmistetaan vielä varmuuden vuoksi, ettei indeksi ole yli listan koon
        if (currentBallIndex >= ballPrefabs.Count)
        {
            isFinished = true;
            isOperating = false;
            yield break;
        }

        // --- VALITAAN PALLO JÄRJESTYKSESSÄ ---
        GameObject selectedPrefab = ballPrefabs[currentBallIndex];

        // Kasvatetaan indeksiä
        currentBallIndex++;

        // Jos tämä oli listan viimeinen pallo, asetetaan kone valmiiksi (isFinished)
        if (currentBallIndex >= ballPrefabs.Count)
        {
            isFinished = true;
            Debug.Log("BallMachine: Kaikki listan pallot on nyt laukaistu kerran. Kone sammutettu.");
        }

        // Varmistetaan vielä, ettei listan kyseinen alkio ole tyhjä (null)
        if (selectedPrefab == null)
        {
            Debug.LogError("Valittu Prefab listassa oli null! Tarkista Ball Prefabs lista.");
            isOperating = false;
            yield break;
        }

        // 1. SYNNYTETÄÄN VALITTU PALLO ALOITUSPISTEESEEN
        GameObject spawnedBall = Instantiate(selectedPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody ballRb = spawnedBall.GetComponent<Rigidbody>();
        Collider ballCol = spawnedBall.GetComponent<Collider>();

        if (ballRb != null)
        {
            ballRb.isKinematic = true;
            ballRb.useGravity = false;
        }
        if (ballCol != null)
        {
            ballCol.enabled = false;
        }

        spawnedBall.transform.localScale = startingScale;

        // --- Tallennetaan kammen ja jousen lähtötilanteet ---
        Quaternion startCrankRot = crankTransform != null ? crankTransform.localRotation : Quaternion.identity;
        Quaternion startSpringRot = springTransform != null ? springTransform.localRotation : Quaternion.identity;

        // --- Lasketaan pyöritysten tavoitteet ---
        Quaternion targetCrankRot = startCrankRot * Quaternion.Euler(crankRotationAxis * crankRotationDegrees);
        Quaternion targetSpringRot = startSpringRot * Quaternion.Euler(springRotationAxis * springRotationDegrees);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            if (crankTransform != null) crankTransform.localRotation = Quaternion.Slerp(startCrankRot, targetCrankRot, t);
            if (springTransform != null) springTransform.localRotation = Quaternion.Slerp(startSpringRot, targetSpringRot, t);

            if (spawnedBall != null)
            {
                spawnedBall.transform.position = Vector3.Lerp(spawnPoint.position, targetPoint.position, t);
                spawnedBall.transform.localScale = Vector3.Lerp(startingScale, targetScale, t);
            }

            yield return null;
        }

        if (crankTransform != null) crankTransform.localRotation = targetCrankRot;
        if (springTransform != null) springTransform.localRotation = targetSpringRot;

        if (spawnedBall != null)
        {
            spawnedBall.transform.position = targetPoint.position;
            spawnedBall.transform.localScale = targetScale;

            // 2. PALAUTETAAN FYSIIKAT JA LAUKAISTAAN PALLO ULOS
            if (ballCol != null) ballCol.enabled = true;
            if (ballRb != null)
            {
                ballRb.isKinematic = false;
                ballRb.useGravity = true;

                Vector3 worldLaunchDirection = transform.TransformDirection(launchDirection.normalized);
                ballRb.AddForce(worldLaunchDirection * launchForce, ForceMode.Impulse);
            }
        }

        isOperating = false;
    }

    // Julkinen funktio, jos haluat myöhemmin nollata koneen koodin kautta uudelleenkäynnistystä varten
    public void ResetMachine()
    {
        currentBallIndex = 0;
        isFinished = false;
        isOperating = false;
        Debug.Log("BallMachine nollattu ja valmis uuteen kierrokseen.");
    }
}
