using UnityEngine;
using UnityEngine.UIElements;

public class BackgroundScript : MonoBehaviour
{
   // Start is called once before the first execution of Update after the MonoBehaviour is created
   private float startPos, background_Length;
   public GameObject cam;
   public float parallaxEffect;
    void Start()
    {
      startPos = transform.position.x;   
      background_Length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

   // Update is called once per frame
   void FixedUpdate()
   {
      float distance = cam.transform.position.x * parallaxEffect;
      float movement = cam.transform.position.x * (1 - parallaxEffect);

      transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);
      if (movement > startPos + background_Length)
      {
         startPos += background_Length;

      }
      else if (movement < startPos - background_Length)
      {
         startPos -= background_Length;
      }

   }  
}
