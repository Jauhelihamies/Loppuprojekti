using UnityEngine;

[CreateAssetMenu(fileName = "Uusi Spawn Esine", menuName = "Inventory/Spawn Prefab Esine Data")]
public class SpawnPrefabItemData : InventoryItemData
{
    [Header("Spawn Asetukset")]
    public GameObject prefabToSpawn; 
    public Vector3 spawnOffset = new Vector3(0f, 0.5f, 1f);

    public override void UseItem(Player player)
    {
        // Kutsutaan emoluokan koodia (‰‰net ja logit)
        base.UseItem(player);

        if (prefabToSpawn != null)
        {
            // Lasketaan paikka pelaajan asennon ja offsetin mukaan
            Vector3 spawnPosition = player.transform.position + player.transform.TransformDirection(spawnOffset);

            // Lasketaan rotaatio (esim. pelaajan rintamasuunnan mukaan)
            Quaternion spawnRotation = player.transform.rotation;

            // Luodaan objekti peliin
            Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        }
        else
        {
            Debug.LogWarning($"Esineess‰ {itemName} ei ole asetettu spawnattavaa Prefabia!");
        }
    }
}
