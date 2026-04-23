using UnityEngine;

public class Boss_Attack_Script : MonoBehaviour
{
   public int attackDamage = 1;


   public Vector3 attackOffset;
   public float attackRange = 1f;
   public LayerMask attackMask;

   public CapsuleCollider2D playerCollider;
   public GameObject player;


   public BoxCollider2D attack1, attack2;

   public void StationaryAttack1()
   {
      Vector3 pos = transform.position;
      pos += transform.right * attackOffset.x;
      pos += transform.up * attackOffset.y;

      toggleCollider(attack1);
      
      if (attack1.bounds.Intersects(playerCollider.bounds))
      {
         Vector2 temp = new Vector2(transform.position.y, transform.position.x);
         player.GetComponent<Player>().TakeDamage(attackDamage, 5, 0, temp);
         Debug.Log("Attacked");
      }

      
      
   }

   public void stopStationaryAttack1()
   {
      toggleCollider(attack1);
   }

   public void StationaryAttack2()
   {
      toggleCollider(attack2);

      if (attack2.bounds.Intersects(playerCollider.bounds))
      {
         Vector2 temp = new Vector2(transform.position.y, transform.position.x);
         player.GetComponent<Player>().TakeDamage(attackDamage, 5, 0, temp);
      }

     
   }

   public void stopStationaryAttack2()
   {
      toggleCollider(attack2);
   }


   public void toggleCollider(Collider2D col)
   {
      if (col.enabled == true) { 
      col.enabled = false;
      }
      else
      {
         col.enabled = true;
      }
   }

   void OnTriggerEnter(Collider other)
   {
      if (other.CompareTag("Player"))
      {
         Vector2 temp = new Vector2(transform.position.y, transform.position.x);
         other.GetComponent<Player>().TakeDamage(attackDamage, 5, 0, temp);
      }
   }


}
