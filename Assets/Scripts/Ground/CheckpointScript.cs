using UnityEngine;

public class CheckpointScript : MonoBehaviour
{
   private bool part1,part2,part3,part4;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
   {
      part1 = false; part2 = false; part3 = false; part4 = false;
      

   }

   private void OnTriggerEnter2D(Collider2D other)
   {
      if (other.CompareTag("Checkpoint1") && !part1)
      {
         part1 = true;
         OSCHandler.Instance.SendMessageToClient("pd","/unity/checkpoint1", 0.5f);
         //OSCHandler.Instance.SendMessageToClient("pd", "/unity/melee", 100);
         Debug.Log("Am I working?");

      }
      if (other.CompareTag("Checkpoint2") && !part2)
      {
         part2 = true;
         OSCHandler.Instance.SendMessageToClient("pd", "/unity/checkpoint2", 0.5f);

      }
      if (other.CompareTag("Checkpoint3") && !part3)
      {
         part3 = true;
         OSCHandler.Instance.SendMessageToClient("pd", "/unity/checkpoint3", 0.5f);

      }
      if (other.CompareTag("Checkpoint4") && !part4)
      {
         part4 = true;
         OSCHandler.Instance.SendMessageToClient("pd", "/unity/checkpoint4", 0.5f);

      }
   }
}
