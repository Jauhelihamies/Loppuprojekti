using UnityEngine;

public class Spawner : MonoBehaviour
{
    public static Spawner Instance { get; private set; }

    [SerializeField] private GameObject prefabToSpawn; // Vedä prefabi tähän
    [SerializeField] private Transform spawnPoint;     // Vedä tyhjä GameObject tähän Inspectorissa

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    // Tätä kutsutaan muualta ilman parametreja
    public void SpawnAtEmptyObject()
    {
        if (prefabToSpawn != null && spawnPoint != null)
        {
            // Luodaan prefabi tyhjän objektin sijaintiin ja rotaatioon
            Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);
        }
        else
        {
            Debug.LogWarning("Prefab tai Spawn Point puuttuu Spawnerista!");
        }
    }
}
