using UnityEngine;

public class PickupSound : MonoBehaviour
{
   private AudioSource audioSource;
   public AudioClip pickupAudio;
   private GameObject player;

   void Awake()
   {
      player = GameObject.FindWithTag("Player");
      audioSource = player.GetComponent<AudioSource>();
   }

   // Called by animation event
   public void PlayPickupSound()
   {
      if (pickupAudio != null)
      {
         Debug.Log(pickupAudio);
         Debug.Log("Played sound");
         audioSource.PlayOneShot(pickupAudio);
         return;
      }

      OSCHandler.Instance.SendMessageToClient("pd", "/unity/health", 100);
      
   }
}
