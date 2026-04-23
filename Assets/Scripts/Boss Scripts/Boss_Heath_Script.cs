using UnityEngine;

public class Boss_Heath_Script : MonoBehaviour
{
   public int health = 500;

   

   public void TakeDamage(int damage)
   {
      health -= damage;

      if (health <= 0)
      {
         Die();
      }
   }

   void Die()
   {
      GetComponent<Animator>().SetBool("IsDead", true);
   }

}
