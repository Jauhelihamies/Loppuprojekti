using System.Collections;
using UnityEngine;

public class AnimateDoor : MonoBehaviour
{
    [Header("Oven auki-asento (Kopioi tähän arvot kun ovi on auki)")]
    public Vector3 openRotationEuler = new Vector3(15f, -90f, 90f);

    [Header("Oven asetukset")]
    public float openSpeed = 2f;
    [Range(0f, 1f)] public float bounceIntensity = 0.1f; // Tärähdysaste lopussa
    public float bounceSpeed = 12f;

    [Header("Ääniefektit")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip slamSound;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpened = false;

    private void Awake()
    {
        // Otetaan talteen tarkka aloitusasento (kiinni) sellaisena kuin se on alussa
        closedRotation = transform.localRotation;

        // Luodaan tavoiteasento (auki) Inspectorin arvojen perusteella
        openRotation = Quaternion.Euler(openRotationEuler);

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void OpenDoor()
    {
        if (isOpened) return;
        isOpened = true;

        StartCoroutine(OpenDoorRotationRoutine());
    }

    private IEnumerator OpenDoorRotationRoutine()
    {
        if (audioSource != null && openSound != null)
        {
            audioSource.clip = openSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        // Vaihe 1: Sulautetaan ovi kiinni-asennosta auki-asentoon (kaikki akselit muuttuvat oikein)
        float progress = 0f;
        while (progress < 1.0f)
        {
            progress += Time.deltaTime * openSpeed;
            transform.localRotation = Quaternion.Lerp(closedRotation, openRotation, progress);
            yield return null;
        }

        if (audioSource != null && audioSource.clip == openSound)
        {
            audioSource.Stop();
        }

        if (audioSource != null && slamSound != null)
        {
            audioSource.PlayOneShot(slamSound);
        }

        // Vaihe 2: Pieni lopputärähdys auki-asennossa
        float time = 0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime * bounceSpeed;
            // Vaimeneva jousiefekti
            float bounceProgress = Mathf.Sin(time * Mathf.PI * 2f) * bounceIntensity * (1f - time);

            // Täräytetään ovea hieman takaisin kohti suljettua asentoa ja takaisin
            transform.localRotation = Quaternion.Lerp(openRotation, closedRotation, bounceProgress * 0.1f);
            yield return null;
        }

        // Varmistetaan tasan tarkka lopullinen auki-asento
        transform.localRotation = openRotation;
        Debug.Log("Ovi on täysin auki kaikilla akseleilla!");
    }
}
