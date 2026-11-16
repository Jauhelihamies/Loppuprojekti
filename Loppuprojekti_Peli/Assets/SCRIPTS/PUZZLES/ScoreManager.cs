using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ScoreCollector : MonoBehaviour
{
    [Header("Asetukset")]
    [SerializeField] private int tarvittavatPallot = 5;
    [SerializeField] private string palloTag = "SCORE";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource; // Ved‰ AudioSource-komponentti t‰h‰n
    [SerializeField] private AudioClip scoreSound;    // ƒ‰ni, kun pallo ker‰t‰‰n
    [SerializeField] private AudioClip winSound;      // ƒ‰ni, kun voitto toteutuu
    [Tooltip("J‰t‰ 0, jos haluat odottaa automaattisesti koko piste‰‰nen pituuden ajan.")]
    [SerializeField] private float viiveEnnenVoittoaanea = 0f;

    private int keratytPallot = 0;

    // T‰m‰ julkinen muuttuja sallii oven tarkistaa tilanteen
    public bool OnVoitettu { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        if (OnVoitettu) return;

        if (other.CompareTag(palloTag))
        {
            keratytPallot++;
            Destroy(other.gameObject);

            if (keratytPallot >= tarvittavatPallot)
            {
                OnVoitettu = true;
                // K‰ynnistet‰‰n ajastettu ‰‰nisekvenssi voitolle
                StartCoroutine(SoitaVoittoSekvenssi());
            }
            else
            {
                // Soitetaan tavallinen piste‰‰ni
                if (audioSource != null && scoreSound != null)
                {
                    audioSource.PlayOneShot(scoreSound);
                }
            }
        }
    }

    private IEnumerator SoitaVoittoSekvenssi()
    {
        if (audioSource != null)
        {
            // 1. Soitetaan viimeisen pallon piste‰‰ni
            if (scoreSound != null)
            {
                audioSource.PlayOneShot(scoreSound);

                // Lasketaan odotusaika: k‰ytet‰‰n joko omaa viivett‰ tai ‰‰niklipin pituutta
                float odotusAika = viiveEnnenVoittoaanea > 0 ? viiveEnnenVoittoaanea : scoreSound.length;
                yield return new WaitForSeconds(odotusAika);
            }

            // 2. Soitetaan voitto‰‰ni heti per‰‰n
            if (winSound != null)
            {
                audioSource.PlayOneShot(winSound);
            }
        }
    }
}
