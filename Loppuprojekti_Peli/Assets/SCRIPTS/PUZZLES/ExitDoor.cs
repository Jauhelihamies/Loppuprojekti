using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))] // Varmistaa, että objektissa on AudioSource-komponentti
public class ExitDoorController : MonoBehaviour
{
    [Header("Viittaukset")]
    [SerializeField] private ScoreCollector scoreCollector; // Raahaa tähän pallokerääjä
    [SerializeField] private Transform venttiili;         // Raahaa tähän Valve-objekti
    [SerializeField] private Transform oviMetalli;        // Raahaa tähän EXITDOOR-objekti

    [Header("Ääniefektit")]
    [SerializeField] private AudioClip epaonnistuiAani;    // Ääni, kun ovi ei aukea (esim. kolahdus/lukossa)
    [SerializeField] private AudioClip onnistuiAani;       // Ääni, kun ovi aukeaa (esim. narina/mekanismi)

    [Header("Venttiilin Asetukset")]
    [SerializeField] private Vector3 venttiiliPyorahdysAkseli = new Vector3(0, 0, 1);
    [SerializeField] private float pieniPyorahdysAsteet = 30f;
    [SerializeField] private float venttiiliPyorimisNopeus = 360f;

    [Header("Oven Rotaatioasetukset")]
    [SerializeField] private Vector3 oviAvausAkseli = new Vector3(0, 1, 0);
    [SerializeField] private float oviAvausKulma = 90f;
    [SerializeField] private float animaatioKesto = 2f;

    private AudioSource audioSource;
    private bool onkoKaynnissa = false;
    private bool oviOnAuki = false;

    private void Awake()
    {
        // Haetaan AudioSource-komponentti talteen
        audioSource = GetComponent<AudioSource>();
    }

    // Tämä metodi kutsutaan Interactable Item -skriptistä
    public void YritaAvataOvi()
    {
        if (onkoKaynnissa || oviOnAuki) return;

        if (scoreCollector != null && scoreCollector.OnVoitettu)
        {
            // Soitetaan onnistumisääni
            ToistaAani(onnistuiAani);
            StartCoroutine(AvaaOviAnimaatio());
        }
        else
        {
            // Soitetaan epäonnistumisääni
            ToistaAani(epaonnistuiAani);
            StartCoroutine(OvenPyorahdysAnimaatio());
        }
    }
    private void ToistaAani(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    private IEnumerator OvenPyorahdysAnimaatio()
    {
        onkoKaynnissa = true;
        Quaternion aloitusRotaatio = venttiili.localRotation;
        Quaternion kohdeRotaatio = aloitusRotaatio * Quaternion.Euler(venttiiliPyorahdysAkseli * pieniPyorahdysAsteet);

        float aika = 0;
        while (aika < 0.2f)
        {
            venttiili.localRotation = Quaternion.Slerp(aloitusRotaatio, kohdeRotaatio, aika / 0.2f);
            aika += Time.deltaTime;
            yield return null;
        }

        aika = 0;
        while (aika < 0.2f)
        {
            venttiili.localRotation = Quaternion.Slerp(kohdeRotaatio, aloitusRotaatio, aika / 0.2f);
            aika += Time.deltaTime;
            yield return null;
        }

        venttiili.localRotation = aloitusRotaatio;
        onkoKaynnissa = false;
    }

    // Jos peli ON voitettu: venttiili pyörii jatkuvasti ja ovi kääntyy (rotate) auki
    private IEnumerator AvaaOviAnimaatio()
    {
        onkoKaynnissa = true;
        oviOnAuki = true;

        Quaternion oviAloitusRotaatio = oviMetalli.localRotation;
        Quaternion oviKohdeRotaatio = oviAloitusRotaatio * Quaternion.Euler(oviAvausAkseli * oviAvausKulma);

        float aika = 0;
        while (aika < animaatioKesto)
        {
            float t = aika / animaatioKesto;

            venttiili.Rotate(venttiiliPyorahdysAkseli * venttiiliPyorimisNopeus * Time.deltaTime);
            oviMetalli.localRotation = Quaternion.Slerp(oviAloitusRotaatio, oviKohdeRotaatio, t);

            aika += Time.deltaTime;
            yield return null;
        }

        oviMetalli.localRotation = oviKohdeRotaatio;
        onkoKaynnissa = false;

    }
}
