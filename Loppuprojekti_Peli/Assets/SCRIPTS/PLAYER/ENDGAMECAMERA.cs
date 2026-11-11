using UnityEngine;
using System.Collections;
public class ENDGAMECAMERA : MonoBehaviour
{
    [Header("Kameran asetukset")]
    [Tooltip("Voittokamera, johon siirryt‰‰n")]
    public Camera voittoKamera;

    [Tooltip("Haluamasi lopullinen zoom-taso (pienempi arvo = l‰hemp‰n‰)")]
    public float tavoiteFOV = 25f;

    [Tooltip("Kuinka monta sekuntia zoomaus kest‰‰")]
    public float zoomKesto = 3.0f;

    private Camera alkuperainenKamera;

    void Start()
    {
        // Varmistetaan, ett‰ voittokamera on alussa pois p‰‰lt‰
        if (voittoKamera != null)
        {
            voittoKamera.gameObject.SetActive(false);
        }
    }

    // Kutsu t‰t‰ funktiota, kun pelaaja voittaa pelin
    public void AktivoiVoittoKamera()
    {
        if (voittoKamera == null)
        {
            Debug.LogWarning("Voittokameraa ei ole m‰‰ritetty skriptiin!");
            return;
        }

        // Haetaan nykyinen Main Camera ja sammutetaan se
        alkuperainenKamera = Camera.main;
        if (alkuperainenKamera != null)
        {
            alkuperainenKamera.gameObject.SetActive(false);
        }

        // K‰ynnistet‰‰n voittokamera ja aloitetaan hidas zoomaus
        voittoKamera.gameObject.SetActive(true);
        StartCoroutine(ZoomaaHitaasti());
    }

    private IEnumerator ZoomaaHitaasti()
    {
        float aloitusFOV = voittoKamera.fieldOfView;
        float kulunutAika = 0f;

        while (kulunutAika < zoomKesto)
        {
            kulunutAika += Time.deltaTime;
            float edistyminen = kulunutAika / zoomKesto;

            // SmoothStep tekee zoomauksen alusta ja lopusta pehme‰mm‰n
            float pehmennettyEdistyminen = Mathf.SmoothStep(0f, 1f, edistyminen);
            voittoKamera.fieldOfView = Mathf.Lerp(aloitusFOV, tavoiteFOV, pehmennettyEdistyminen);

            yield return null;
        }

        // Varmistetaan, ett‰ lopullinen arvo on tarkka
        voittoKamera.fieldOfView = tavoiteFOV;
    }
}