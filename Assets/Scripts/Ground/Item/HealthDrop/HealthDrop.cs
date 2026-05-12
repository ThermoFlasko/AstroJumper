using UnityEngine;

[CreateAssetMenu(fileName = "HealthDrop", menuName = "Scriptable Objects/HealthDrop")]
public class HealthDrop : Item
{
    public float healthAmount = 20;

    public override void OnItemPickUp()
    {
        Player player = FindAnyObjectByType<Player>();

        player.Health += 20;

    }
}
