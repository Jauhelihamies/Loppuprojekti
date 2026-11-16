using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn; // Ved‰ prefabi t‰h‰n
    [SerializeField] private Transform spawnPoint;   // Ved‰ tyhj‰ GameObject t‰h‰n Inspectorissa

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;  // Ved‰ AudioSource-komponentti t‰h‰n
    [SerializeField] private AudioClip spawnSound;     // Ved‰ ‰‰nitiedosto t‰h‰n

    public void SpawnReward()
    {
        if (prefabToSpawn != null && spawnPoint != null)
        {
            // Luodaan prefabi tyhj‰n objektin sijaintiin ja rotaatioon
            Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

            // Soitetaan ‰‰ni kerran (OneShot), jos ‰‰ni ja l‰hde on asetettu
            if (audioSource != null && spawnSound != null)
            {
                audioSource.PlayOneShot(spawnSound);
            }
        }
        else
        {
            Debug.LogWarning("Prefab tai Spawn Point puuttuu Spawnerista!");
        }
    }
}
