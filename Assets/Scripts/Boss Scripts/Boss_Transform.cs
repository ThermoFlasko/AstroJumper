using UnityEngine;

public class Boss_Transform : MonoBehaviour
{
   public Transform player;

   public bool isFlipped = false;

   public void LookAtPlayer()
   {
      Vector3 flipped = transform.localScale;
      flipped.z *= -1f;

      if (transform.position.x > player.position.x && isFlipped)
      {
         transform.localScale = flipped;
         transform.Rotate(0f, 180f, 0);
         isFlipped = false;
      }
      else if (transform.position.x < player.position.x && !isFlipped)
      {
         transform.localScale = flipped;
         transform.Rotate(0f, 180f, 0f);
         isFlipped = true;
      }
   }
}
