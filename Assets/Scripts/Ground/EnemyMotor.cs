using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMotor : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseSpeedModifier = 2.0f;
    [SerializeField] private float limpSpeedModifier = -1.5f;

    private float originalSpeed;

    private Rigidbody2D rb;
    public int FacingDir { get; private set; } = -1; // -1 left, +1 right

    private void Awake()
    {
        originalSpeed = moveSpeed;
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move()
    {
        moveSpeed = originalSpeed;
        rb.linearVelocity = new Vector2(FacingDir * moveSpeed, rb.linearVelocity.y);
        //Debug.Log($"FacingDir={FacingDir}, velX={rb.linearVelocity.x}");
    }

    public void Chase()
    {
        moveSpeed += chaseSpeedModifier;
        rb.linearVelocity = new Vector2(FacingDir * moveSpeed, rb.linearVelocity.y);
    }

    public void Limp()
    {
        moveSpeed += limpSpeedModifier;
        rb.linearVelocity = new Vector2(FacingDir * moveSpeed, rb.linearVelocity.y);
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
