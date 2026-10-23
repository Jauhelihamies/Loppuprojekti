using UnityEngine;
using UnityEngine.Events;

public class InteractableItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemName = "Aktivoitava kohde";
    public AudioClip interactSound;
    public bool destroyOnInteract = true;  

    public InventoryItemData itemData;  

    [Header("Activation Events")]
    public UnityEvent onInteractAction;

    private bool isAlreadyInteracted = false;

    public void Interact(Player player)
    {
        if (isAlreadyInteracted) return;
        isAlreadyInteracted = true;

        if (interactSound != null)
        {
            AudioSource.PlayClipAtPoint(interactSound, transform.position, 1f);
        }


        if (itemData != null && player != null)
        {
            player.AddItemToInventory(itemData);
        }

        if (onInteractAction != null)
        {
            onInteractAction.Invoke();
        }

        if (destroyOnInteract)
        {
            Destroy(gameObject);
        }
        else
        {
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
        }
    }
}
