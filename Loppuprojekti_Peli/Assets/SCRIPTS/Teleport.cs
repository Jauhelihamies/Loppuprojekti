using System.Collections;
using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    [Header("Asetukset")]
    [Tooltip("Ved‰ t‰h‰n toinen portaali, johon t‰m‰ portaali vie.")]
    public PortalTeleport destinationPortal;

    [Tooltip("Aika sekunteina, jonka j‰lkeen portaali toimii uudelleen.")]
    public float teleportCooldown = 1.0f;

    [Tooltip("Pelaajan Tag-tunniste.")]
    public string playerTag = "Player";

    // Est‰‰ loputtoman edestakaisin teleporttaamisen
    private bool canTeleport = true;

    private void OnTriggerEnter(Collider other)
    {
        // Tarkistetaan, ett‰ osuva kohde on pelaaja ja teleportti on valmis
        if (canTeleport && destinationPortal != null && other.CompareTag(playerTag))
        {
            TeleportPlayer(other.gameObject);
        }
    }

    private void TeleportPlayer(GameObject player)
    {
        // K‰ynnistet‰‰n j‰‰hyaika molemmissa portaaleissa
        StartCoroutine(CooldownRoutine());
        destinationPortal.StartCooldown();

        // Jos pelaaja k‰ytt‰‰ CharacterControlleria, se pit‰‰ poistaa hetkeksi k‰ytˆst‰ sijainnin muuttamiseksi
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
        }

        // Siirret‰‰n pelaaja kohdeportaalin sijaintiin
        player.transform.position = destinationPortal.transform.position;

        // K‰‰nnet‰‰n pelaaja katsomaan kohdeportaalin suuntaan (valinnainen, poista jos ei tarvita)
        player.transform.rotation = destinationPortal.transform.rotation;

        if (cc != null)
        {
            cc.enabled = true;
        }
    }

    public void StartCooldown()
    {
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        canTeleport = false;
        yield return new WaitForSeconds(teleportCooldown);
        canTeleport = true;
    }
}
