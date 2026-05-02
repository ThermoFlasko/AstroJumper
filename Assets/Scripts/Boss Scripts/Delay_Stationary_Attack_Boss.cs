using Unity.VisualScripting;
using UnityEngine;

public class Delay_Stationary_Attack_Boss : StateMachineBehaviour
{

   Rigidbody2D rb;
   public float decend_Speed = 1f;

   Transform boss_transform;

   //Taken from player for boss attack
   [Header("Ground Check")]
   private Transform groundCheck;
   [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.6f);
   [SerializeField] private LayerMask groundMask;   // Ground + OneWayPlatform for jumping 
   [SerializeField] private LayerMask oneWayMask;   // OneWayPlatform only for dropping through select platforms

   private bool isGrounded;


   // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
   override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
   {
     boss_transform = animator.transform;
     rb = animator.GetComponent<Rigidbody2D>();
     groundCheck = animator.GetComponentInChildren<Transform>();
   }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundMask);
      if (isGrounded) 
      {
         animator.SetTrigger("Decend");
      }
      else
      {
         Vector2 newpos = new Vector2(boss_transform.position.x, boss_transform.position.y - decend_Speed);
         rb.MovePosition(newpos);
      }



   }

   // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
   override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      animator.ResetTrigger("Decend");

   }


}
