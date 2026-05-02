using UnityEngine;

public class Boss_Move : StateMachineBehaviour
{

   public float speed = 2.5f;
   public float attackRange = 3f;

   //Find player
   Transform player;
   //for boss hitbox
   Rigidbody2D rb;
   Boss_Transform boss;



    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      player = GameObject.FindGameObjectWithTag("Player").transform;
      rb = animator.GetComponent<Rigidbody2D>();

      boss = animator.GetComponent<Boss_Transform>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      boss.LookAtPlayer();
      Vector2 target = new Vector2(player.position.x, player.position.y);
      Vector2 newpos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
      rb.MovePosition(newpos);

      if (Vector2.Distance(player.position, rb.position) <= attackRange)
      {
         animator.SetTrigger("Attack");
      }

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      //prevent from attacking right after if not in range
      animator.ResetTrigger("Attack");   
    }

    
}
