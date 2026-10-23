using UnityEngine;

[CreateAssetMenu(fileName = "Uusi Esine", menuName = "Inventory/Esine Data")]
public class InventoryItemData : ScriptableObject
{
    public string itemName = "Esineen Nimi";
    public Sprite itemIcon;                
    public AudioClip useSound;          


    public void UseItem(Player player)
    {
        Debug.Log($"{player.name} käytti esinettä: {itemName}");

        if (useSound != null)
        {
            AudioSource.PlayClipAtPoint(useSound, player.transform.position, 1f);
        }


    }
}
