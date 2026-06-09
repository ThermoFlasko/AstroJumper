using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{

    public GameObject projectilePrefab;
    public int poolSize = 10;
    public Queue<GameObject> projectilePool = new Queue<GameObject>();
    public ProjectileAudio playSound;
   //attach parent prefab in the unity inspector
   public GameObject JuiceSFXBulletImpact;
   public PlayerAnimator SetAttackBool;

   public float xOffset;

   void Awake()
    {
      //Update: not needed if attatched from inspector screen
        //projectilePrefab = GetComponentInParent<Unit>().GetProjectilePrefab();
        if(projectilePrefab == null)
        {
            print("Add a attack thats a projectile");
        }
   }

   public GameObject GetProjectile()
    {
        if (projectilePool.Count > 0)
        {
            GameObject projectile = projectilePool.Dequeue();
            projectile.SetActive(true);
            projectile.GetComponent<Projectile>().enabled = true; 
            projectile.transform.GetChild(0).gameObject.SetActive(true);
            playSound.PlayRandomSound();
            SetAttackBool.MakePlayerShoot();

            return projectile;

        }
        else
        {
         Debug.Log("Testing script");
            return Instantiate(projectilePrefab);
        }
    }

    public void ReturnProjectile(GameObject projectile)
    {
      float moveDirectionX = projectile.GetComponent<Projectile>().GetHorizontalVelocity();

      float zRotation = (moveDirectionX > 0) ? 0f : 180f;
      float xImpactOffset = projectile.transform.position.x + ((moveDirectionX > 0) ? -xOffset : xOffset);

      Vector3 adjustTransform = new Vector3(xImpactOffset, projectile.transform.position.y, 0f);
      Quaternion rotation = Quaternion.Euler(0, 0, zRotation);

      PlayAtLocation(adjustTransform, rotation);
      
      projectile.SetActive(false);
      projectile.GetComponent<Projectile>().enabled = false;
      projectile.transform.GetChild(0).gameObject.SetActive(false);
      
      projectilePool.Enqueue(projectile);
   }
      //you need quaternion for flipping
   public void PlayAtLocation(Vector3 targetPosition, Quaternion targetRotation)
   {
     
      
      Destroy(Instantiate(JuiceSFXBulletImpact, targetPosition, targetRotation), 3f);

   }


}
