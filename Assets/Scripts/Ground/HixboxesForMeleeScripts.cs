using UnityEngine;

public class HixboxesForMeleeScripts : MonoBehaviour
{
    //melee hitboxes
    public BoxCollider2D sweetSpot;
    public BoxCollider2D sourSpot;

    void Awake()
    {
        sweetSpot.enabled = false;
        sourSpot.enabled = false;
    }

    public void enableHitboxes()
    {
        sweetSpot.enabled = true;
        sourSpot.enabled = true;
    }
    public void disableHitboxes()
    {
        sweetSpot.enabled = false;
        sourSpot.enabled = false;
    }

   public void MakeSound()
   {
      OSCHandler.Instance.SendMessageToClient("pd", "/unity/melee", 45);
      print("sounds");
   }

   

}
