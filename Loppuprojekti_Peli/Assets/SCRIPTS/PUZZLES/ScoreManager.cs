using UnityEngine;
using UnityEngine.Events;

public class ScoreCollector : MonoBehaviour
{
    [Header("Asetukset")]
    [SerializeField] private int tarvittavatPallot = 5;
    [SerializeField] private string palloTag = "SCORE";

    private int keratytPallot = 0;

    // Tämä julkinen muuttuja sallii oven tarkistaa tilanteen
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


            }
        }
    }
}
