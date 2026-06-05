using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    HitBox hitBoxInfo;
    private float speed = 1f;
    private int direction = 1; // 1 for right, -1 for left
    private float yValue = 0f;
    private Vector3 desiredTransform;
    private bool isDead = false; // prevent LateUpdate from moving after death
    private bool useLobbedMovement = false;
    private float lobInitialVerticalVelocity = 0f;
    private float lobGravity = 12f;
    private float lobMaxFallSpeed = 20f;
    private float verticalVelocity = 0f;

   public float horizontalMove { get; set; }
   

   [SerializeField] private LayerMask wallLayers;

    void OnEnable()
    {
        isDead = false;
        verticalVelocity = lobInitialVerticalVelocity;
        desiredTransform = transform.position; // initialize to current position
    }

    void Update()
    {
        if (isDead) return;

        horizontalMove = speed * direction * Time.deltaTime;
        float verticalMove = 0f;

        if (useLobbedMovement)
        {
            verticalVelocity -= lobGravity * Time.deltaTime;
            if (lobMaxFallSpeed > 0f)
                verticalVelocity = Mathf.Max(verticalVelocity, -lobMaxFallSpeed);
            verticalMove = verticalVelocity * Time.deltaTime;
        }

        Vector3 movement = new Vector3(horizontalMove, verticalMove, 0f);
        float moveDistance = movement.magnitude;
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y);
        Vector2 rayDir = moveDistance > 0f ? (Vector2)movement.normalized : (direction == 1 ? Vector2.right : Vector2.left);
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDir, moveDistance + 0.1f, wallLayers);

        if (hit.collider != null)
        {
            Debug.Log("Projectile raycast hit: " + hit.collider.name);
            isDead = true;
            HitBox hitBox = GetComponentInChildren<HitBox>();
            if (hitBox != null)
            {
                Vector3 impactPosition = hit.point;
                impactPosition.z = transform.position.z;
                hitBox.SpawnImpactEffect(impactPosition);
                hitBox.ForceDestroy();
            }
            return;
        }

        float nextY = useLobbedMovement ? transform.position.y + verticalMove : yValue;
        desiredTransform = new Vector3(transform.position.x + horizontalMove, nextY, transform.position.z);
    }

    void LateUpdate()
    {
        if (isDead) return;
        transform.position = desiredTransform;
    }

    public void SetWallLayers(LayerMask layers)
    {
        wallLayers = layers;
    }

    public void SetDirection(int dir) { direction = dir; }
    public void SetYValue(float y) { yValue = y; }
    public void SetSpeed(float newSpeed) { speed = newSpeed; }
    public void SetLobbedMovement(bool enabled, float initialVerticalVelocity, float gravity, float maxFallSpeed)
    {
        useLobbedMovement = enabled;
        lobInitialVerticalVelocity = initialVerticalVelocity;
        lobGravity = gravity;
        lobMaxFallSpeed = maxFallSpeed;
        verticalVelocity = lobInitialVerticalVelocity;
    }

   public float GetHorizontalVelocity()
   {
      return horizontalMove;
   }

  
}
