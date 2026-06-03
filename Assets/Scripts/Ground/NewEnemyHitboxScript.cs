using UnityEngine;

public class NewEnemyHitboxScript : MonoBehaviour
{
    [SerializeField] private int sweetSpotDamage = 40;
    [SerializeField] private float sweetSpotHorizontalKnockback = 30f;
    [SerializeField] private float sweetSpotVerticalKnockback = 20f;


     private void OnTriggerEnter2D(Collider2D other) {
        Unit unit = GetComponent<Unit>();
       
       //If only sweetspot is hit use sweetspot
        
        if (other.CompareTag("SweetSpot") && !other.CompareTag("SourSpot") && unit != null)
        {
            unit.TakeDamage(sweetSpotDamage, sweetSpotHorizontalKnockback, sweetSpotVerticalKnockback, transform.position);
            
            
            Debug.Log("Sweet Spot Hit!");
        }

        //if both colliders hit priortize Sourspot
        else if (gameObject.CompareTag("SourSpot") && other.CompareTag("SweetSpot") && unit != null)
        {
            unit.TakeDamage(10, 10, 5, transform.position);
            Debug.Log("Sour Spot Hit!");
        }
        EnemyAI enemyAI = GetComponentInParent<EnemyAI>();
        if (enemyAI.canBeDamagedByWall && other.CompareTag("Terrain"))
        {
            unit.TakeDamage(10, 0, 0, transform.position);
            enemyAI.canBeDamagedByWall = false;
            Debug.Log("Enemy hit by wall!");
        }
    }
}
