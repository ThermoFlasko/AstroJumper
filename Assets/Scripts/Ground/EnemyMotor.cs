using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMotor : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeedModifier = 1.5f;
    [SerializeField] private float limpSpeedModifier = 0.6f;

    private float originalSpeed;

    private Rigidbody2D rb;
    public int FacingDir { get; private set; } = -1; // -1 left, +1 right

    private void Awake()
    {
        originalSpeed = moveSpeed;
        rb = GetComponent<Rigidbody2D>();
    }
    

    // these are constantly being called by the EnemyAI script
    // need to register once only any updates to enemy speed so it doesnt keep making calls

    // default call is Move() in most cases
    // if the enemy gets low then enemy should now move with the limp modifier

    public void Move()
    {
        rb.linearVelocity = new Vector2(FacingDir * moveSpeed, rb.linearVelocity.y);
    }

    public void Chase()
    {
        moveSpeed = CalculateNewMoveSpeed(originalSpeed, chaseSpeedModifier);
    }

    public void LimpChase()
    {
        moveSpeed = CalculateNewMoveSpeed(originalSpeed, chaseSpeedModifier, limpSpeedModifier);
    }

    public void Limp()
    {
        moveSpeed = CalculateNewMoveSpeed(originalSpeed, limpSpeedModifier);
    }

    public void RestoreSpeedToOriginal()
    {
        moveSpeed = originalSpeed;
    }

    public void StopHorizontal()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    public void Flip()
    {
        FacingDir *= -1;
        transform.Rotate(0f, 180f, 0f);
    }

    private float CalculateNewMoveSpeed(float speed, float modifier, float modifier2 = 1f)
    {
        return speed * modifier * modifier2;
    }

    public void SetFacingToward(float targetX)
    {
        int desired = (targetX >= transform.position.x) ? 1 : -1;
        if (desired != FacingDir) Flip();
    }

    public void ApplyKnockback(Vector2 force)
    {
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
    }
}
