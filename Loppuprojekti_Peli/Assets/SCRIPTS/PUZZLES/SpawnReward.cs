using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefabToSpawn; // Vedä prefabi tähän
    [SerializeField] private Transform spawnPoint;     // Vedä tyhjä GameObject tähän Inspectorissa

    public void SpawnReward()
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
