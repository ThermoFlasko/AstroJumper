using UnityEngine;

[CreateAssetMenu(fileName = "HealthDrop", menuName = "Scriptable Objects/HealthDrop")]
public class HealthDrop : Item
{
    public float healthAmount = 20;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnItemPickUp()
    {
        Player player = FindAnyObjectByType<Player>();

        player.Health += 20;

    }
}
