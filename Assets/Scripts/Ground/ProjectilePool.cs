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
      PlayAtLocation(projectile.transform.position, projectile.transform.rotation);
      
      projectile.SetActive(false);
      projectile.GetComponent<Projectile>().enabled = false;
      projectile.transform.GetChild(0).gameObject.SetActive(false);
      
      projectilePool.Enqueue(projectile);
   }
      //probably dont need quaternion
   public void PlayAtLocation(Vector3 targetPosition, Quaternion targetRotation)
   {
      JuiceSFXBulletImpact.transform.GetChild(0).gameObject.SetActive(true);
      //Call and destroy impact juice from bullet in 3 seconds
      Destroy(Instantiate(JuiceSFXBulletImpact, targetPosition, targetRotation), 3f);

   }


}
