using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadBossScene : MonoBehaviour
{



   void Update()
   {
      // Example: Press Control + Shift + B to load "Boss Scene by Alfredo"
      if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Y) && Input.GetKeyDown(KeyCode.B))
      {
         SceneManager.LoadScene("Boss Scene");
      }
      // Example: Press Control + Shift + M to load "Boss Scene by Alfredo"

      if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.Y) && Input.GetKeyDown(KeyCode.M))
      {
         SceneManager.LoadScene("Menus");
      }
   }

}
