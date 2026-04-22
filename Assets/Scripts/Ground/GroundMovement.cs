using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class GroundMovement : MonoBehaviour
{
    //hello
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 70f;

    [Header("Jump")]
    [SerializeField] private float jumpVelocity = 18f; // tweak this with gravity( grav set to 3 so fall fast enough plus jumps feels decent)
    [SerializeField] private float coyoteTime = 0.08f; // allows for jumping shortly after leaving ground(floating jump for some frames) 
    [SerializeField] private float jumpBufferTime = 0.10f;

    [Header("Jump (Extras)")]
    [SerializeField] private bool enableDoubleJump = false;
    [SerializeField] private int maxAirJumps = 1; // 1 = double jump, 2 = triple . . .
    private int airJumpsRemaining;

    [Header("Variable Jump Height")]
    [SerializeField] private float max_jumpHoldTime = 0.2f; // how long the player can hold the jump button to reach max jump height


    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.6f);
    [SerializeField] private LayerMask groundMask;   // Ground + OneWayPlatform for jumping 
    [SerializeField] private LayerMask oneWayMask;   // OneWayPlatform only for dropping through select platforms

    [Header("Drop Through")]
    [SerializeField] private float dropDuration = 0.5f;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset actionsAsset;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string dropActionName = "Drop";
    [SerializeField] private string sprintActionName = "Sprint";

    [Header("Knockback")]
    [SerializeField] private float knockbackDuration = 0.3f;

    [Header("Slope Handling")]
    [SerializeField] private LayerMask slopeMask;
    [SerializeField] private bool IsOnSlope;
    
    private bool isKnockedBack;
    private Unit unit;

    private Rigidbody2D rb;
    private Collider2D playerCol;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dropAction;
    private InputAction sprintAction;

    private float xInput;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private float jumpHoldTimer;
    private bool isJumpPressed = false;
    private bool isJumpReleased = false;

    private bool isGrounded;
    private bool isDropping;
    public bool isFacingRight = true;



    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCol = GetComponent<Collider2D>();
        unit = GetComponent<Unit>();

        if (groundCheck == null)
            Debug.LogError("GroundMovement: groundCheck is not assigned.");

        if (actionsAsset == null)
            Debug.LogError("GroundMovement: actionsAsset is not assigned (drag InputSystem_Actions in).");

        var map = actionsAsset.FindActionMap(actionMapName, true);
        moveAction = map.FindAction(moveActionName, true);
        jumpAction = map.FindAction(jumpActionName, true);
        dropAction = map.FindAction(dropActionName, true);
        sprintAction = map.FindAction(sprintActionName, true);
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        dropAction.Enable();
        sprintAction.Enable();

        jumpAction.performed += OnJump;
        jumpAction.canceled += OnJumpCanceled;
        dropAction.performed += OnDrop;
        sprintAction.performed += ctx => moveSpeed *= 1.67f; // simple sprint implementation, can be expanded with upgrades or stamina system
        sprintAction.canceled += ctx => moveSpeed /= 1.67f;

        Unit.onKnockedBack += OnKnockedBack;
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJump;
        jumpAction.canceled -= OnJumpCanceled;
        dropAction.performed -= OnDrop;

        moveAction.Disable();
        jumpAction.Disable();
        dropAction.Disable();

        Unit.onKnockedBack -= OnKnockedBack;

    }

    //knockback event subscription
    private void OnKnockedBack(Unit damagedUnit, Vector2 force)
    {
        if (unit == null || damagedUnit != unit) return;
        StartCoroutine(DoKnockback(force));
    }


    //call this method to apply knockback to players
    private IEnumerator DoKnockback(Vector2 force)
    {
        isKnockedBack = true;

        // Apply the force after zeroing horizontal velocity
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.AddForce(force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        isKnockedBack = false;
    }

    private void Update()
    {
        // Read horizontal movement from Move Vector2
        Vector2 move = moveAction.ReadValue<Vector2>();
        xInput = Mathf.Clamp(move.x, -1f, 1f);

        //fixing timer so it counts down properly
        jumpBufferTimer -= Time.deltaTime;
        if (jumpBufferTimer < 0f) jumpBufferTimer = 0f;

        // Ground check + timers
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundMask);
        //isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckSize.y /2 + 0.5f, slopeMask);
        
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;

            // Reset air jumps when you touch the ground
            airJumpsRemaining = enableDoubleJump ? maxAirJumps : 0;

            ResetVariableJump();
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        if (!isKnockedBack)
            HandleHorizontal();

        HandleJumpBuffered();
        CheckIfOnSlope();
        

        if (rb.linearVelocity.y > 0f && isJumpPressed)
        {
            //if the player is still holding the jump button and hasn't exceeded max hold time, apply extra gravity to allow for variable jump height
            if (!isJumpReleased && jumpHoldTimer < max_jumpHoldTime)
            {
                jumpHoldTimer += Time.fixedDeltaTime;
                //velocity.y += gravity_y * held_jump_gravity_scale * delta
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + Physics2D.gravity.y * (rb.gravityScale - 5) * Time.fixedDeltaTime);
            }
            //if the player realsezes jump earlier than the max hold time, apply extra gravity immediately to create a snappier jump feel
            else
            {
                if (jumpHoldTimer < max_jumpHoldTime && isJumpReleased)
                {
                    //apply extra gravity immediately on jump release for snappier feel
                    //velocity.y += gravity_y * released_jump_gravity_scale * delta
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y + Physics2D.gravity.y * (rb.gravityScale - 5) * Time.fixedDeltaTime);
                }    //velocity.y += gravity_y * released_jump_gravity_scale * delta
                isJumpReleased = false; // reset for next jump
            }

        }
        else
        {
            isJumpPressed = false;
            jumpHoldTimer = 0f;
        }
    }

    private void ResetVariableJump()
    {
        isJumpPressed = false;
        isJumpReleased = false;
        jumpHoldTimer = 0f;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        jumpBufferTimer = jumpBufferTime; // buffer jump press

    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        isJumpReleased = true;
    }

    private void OnDrop(InputAction.CallbackContext ctx)
    {
        TryDropThrough(); // Only allows one press 
    }

    private void HandleHorizontal()
    {
        float upgradedMoveSpeed = GetMoveSpeedWithUpgrades();
        float targetSpeed = xInput * upgradedMoveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        rb.AddForce(new Vector2(speedDiff * accelRate, 0f));

        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -upgradedMoveSpeed, upgradedMoveSpeed);

        rb.bodyType = RigidbodyType2D.Dynamic;
        if (IsOnSlope)
        {
            if (xInput == 0f && moveAction.ReadValue<Vector2>().y == 0f)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.linearVelocity = Vector2.zero; 
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);
        }



        // Only update facing direction when not knocked back so sprite doesn't flip
        if (!isKnockedBack)
        {
            if (xInput > 0.01f)
            {
                isFacingRight = true;
                GetComponent<SpriteRenderer>().flipX = false;
            }
            else if (xInput < -0.01f)
            {
                isFacingRight = false;
                GetComponent<SpriteRenderer>().flipX = true;
            }
        }
    }

    private void HandleJumpBuffered()
    {
        if (isDropping) return;

        bool buffered = jumpBufferTimer > 0f;
        if (!buffered) return;

        bool canGroundJump = coyoteTimer > 0f; // grounded or just left ground (coyote)
        bool canAirJump = enableDoubleJump && !isGrounded && airJumpsRemaining > 0;

        if (canGroundJump || canAirJump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, GetJumpVelocityWithUpgrades());

            jumpBufferTimer = 0f;
            isJumpPressed = true;
            isJumpReleased = false;
            jumpHoldTimer = 0f; // reset jump hold timer on new jump press

            if (canGroundJump)
            {

                coyoteTimer = 0f; // saftey to prevent double jump within coyote time
            }
            else
            {
                airJumpsRemaining--;
            }

        }
    }

    private void CheckIfOnSlope()
    {
        if (!isGrounded)
        {
            print("not grounded");
            IsOnSlope = false;
            return;
        }
        IsOnSlope = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckSize.y /2 + 0.5f, slopeMask);
        //RaycastHit2D raycastHit2D = Physics2D.BoxCast
    }



    private void TryDropThrough()
    {
        if (isDropping) return;

        // Only drop if standing on a one-way platform
        Collider2D oneWay = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, oneWayMask);
        if (oneWay == null) return;

        StartCoroutine(DoDrop(oneWay));
    }

    private IEnumerator DoDrop(Collider2D platformCollider)
    {
        isDropping = true;

        Physics2D.IgnoreCollision(playerCol, platformCollider, true);
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -1f);

        yield return new WaitForSeconds(dropDuration);

        Physics2D.IgnoreCollision(playerCol, platformCollider, false);
        isDropping = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.DrawRay(groundCheck.position, Vector2.down * (groundCheckSize.y /2 + 0.5f)); // slope
    }

    private float GetMoveSpeedWithUpgrades()
    {
        float moveSpeedUpgrade = SaveManager.instance != null ? SaveManager.instance.GetGroundMoveSpeedUpgradeBoost() : 0f;
        return moveSpeed + moveSpeedUpgrade;
    }

    private float GetJumpVelocityWithUpgrades()
    {
        float jumpVelocityUpgrade = SaveManager.instance != null ? SaveManager.instance.GetGroundJumpVelocityUpgradeBoost() : 0f;
        return jumpVelocity + jumpVelocityUpgrade;
    }
}
