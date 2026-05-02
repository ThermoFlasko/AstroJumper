using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public PickupSound PickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Inventory inventory = collision.GetComponent<Inventory>();
            if (inventory != null)
            {
                // Assuming this GameObject has an Item component attached to it
                if (item != null)
                {
                    inventory.AddItem(item);
                    PickupSound.PlayPickupSound();
                    Destroy(gameObject); // Destroy the pickup after adding it to the inventory
                }
            }
        }

        if (collision.CompareTag("Terrain"))
        {
            FloatingEffect floatingEffect = gameObject.GetComponent<FloatingEffect>();

            floatingEffect.enabled = true;
        }
    }
}
