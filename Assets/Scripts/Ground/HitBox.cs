// this script holds info for hitboxes, kinda named it wrong, should be named as attackInfo
// WARNING: Not supposed to used as the visible object, meant to attach to other sprite object
// WARNING: One of the objects which is interacting has to have a rigidbody2D for OnTriggerEnter2D to work
// I might just add a rigidbody2D to the hitbox if more problems appear.
using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using System;

public class HitBox : MonoBehaviour
{
    [Header("HitBox Settings")]
    [SerializeField] private string hitBoxName = "BaseHitBox";
    [SerializeField] private int damage = 10;
    [SerializeField] private bool destroyEnemyProjectile = false; // if the hitbox should destroy enemy projectile when it hits it, for player hitbox, it should be false, for enemy hitbox, it should be true
    [SerializeField] private bool isPermanent = false; // for hitboxes you attach to the enemy itself
    [SerializeField] private float duration = 1f;
    [SerializeField] private float activationDelay = 0f;

    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackVerticalForce = 3f;

    [SerializeField] private bool isMelee = true;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private LayerMask targetLayer; // which layer the hitbox should interact with (player, enemy, etc.)
    [SerializeField] private LayerMask ignoreLayer; // which layer the hitbox should ignore 
    [SerializeField] private Vector3 offset = new Vector3(1f, 0f, 0f); // gameplay spawn offset for the whole attack
    [SerializeField] private Vector3 visualOffset = Vector3.zero; // art-only offset used by melee visuals
    [SerializeField] private Sprite sprite;
    private Collider2D hitBoxCollider;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private float currentHitboxActiveDurration = 0f; // how long has the hitbox out
    [SerializeField] private bool displayHitbox = false;
    private Coroutine activationCoroutine;

    public bool animateBaseSprite = false;
    public static event Action<int> onDurationOver;
    public ProjectilePool projectilePool;
    public int attackListIndex = 0;
    public GameObject owner;
    [SerializeField] string AnimatorTriggerName;
    [SerializeField] UnityEditor.Animations.AnimatorController animatorController;

    private void Awake()
    {
        hitBoxCollider = GetComponent<Collider2D>();
        if (hitBoxCollider == null)
        {
            Debug.LogError("HitBox: No Collider2D found on the GameObject.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyHitBoxSprite();
    }

    private void OnEnable()
    {
        currentHitboxActiveDurration = 0f;
        ApplyHitBoxSprite();

        if (hitBoxCollider == null)
        {
            return;
        }

        hitBoxCollider.enabled = false;
        activationCoroutine = StartCoroutine(EnableColliderAfterDelay());
    }

    private void OnDisable()
    {
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
            activationCoroutine = null;
        }

        if (hitBoxCollider != null)
        {
            hitBoxCollider.enabled = false;
        }
    }

    private void ApplyHitBoxSprite()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sprite = displayHitbox ? sprite : null;
    }

    private IEnumerator EnableColliderAfterDelay()
    {
        if (activationDelay > 0f)
        {
            yield return new WaitForSeconds(activationDelay);
        }

        yield return ResetCollider();
        activationCoroutine = null;
    }

    //resets collider so that when player continues to be in the hitbox after it is created, it can still trigger the hitbox
    private IEnumerator ResetCollider()
    {
        hitBoxCollider.enabled = false;
        yield return null;
        hitBoxCollider.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the time the hitbox has been out for
        currentHitboxActiveDurration += Time.deltaTime;
        if (currentHitboxActiveDurration > duration && !isPermanent)
        {
            DestroyAttack();
        }
        if (!isMelee)
        {

        }

    }


    private void OnTriggerEnter2D(Collider2D other)
    {

        GameObject otherObject = other.gameObject;

        //if enemy projecitle desotry it 
        if (destroyEnemyProjectile && otherObject.GetComponent<EnemyProjectile>() != null)
        {
            otherObject.GetComponent<EnemyProjectile>().ReturnToPool();
            return;
        }

        //self protection can not hit itself
        if (other.transform.IsChildOf(transform.root) ||
        other.transform == transform.root)
            return;

        if ((targetLayer.value & (1 << otherObject.layer)) == 0 || (ignoreLayer.value & (1 << otherObject.layer)) != 0) // Checks if objects layer is in the layer mask, found from https://discussions.unity.com/t/checking-if-a-layer-is-in-a-layer-mask/860331
        {
            //print("hit wrong layer, ignoring");
            return;
        }
        Unit unit = other.GetComponent<Unit>();
        if (unit != null)
        {
            unit.TakeDamage(damage, knockbackForce, knockbackVerticalForce, transform.position);
            if (!isMelee)
            {
                DestroyAttack();

            }
        }
    }

    public void SetOwner(GameObject go)
    {
        owner = go;
    }

    public bool GetIsMelee()
    {
        return isMelee;
    }

    public Vector3 GetOffset()
    {
        return offset;
    }

    public Vector3 GetVisualOffset()
    {
        return visualOffset;
    }

    public Sprite GetSprite()
    {
        return sprite;
    }

    public float GetKnockbackForce() => knockbackForce;
    public float GetKnockbackVerticalForce() => knockbackVerticalForce;
    public float GetActivationDelay() => activationDelay;

    public void DestroyAttack()
    {
        onDurationOver?.Invoke(attackListIndex);
        if (!isMelee)
        {
            projectilePool.ReturnProjectile(transform.parent.gameObject);
            resetDuration();
            return;
        }
        Destroy(transform.parent.gameObject);
        Destroy(gameObject);
        
    }
    public void ForceDestroy()
    {
        DestroyAttack();
    }



    public void setPool(ProjectilePool pool)
    {
        projectilePool = pool;
    }

    private void resetDuration()
    {
        currentHitboxActiveDurration = 0f;
    }

    public float GetProjectileSpeed()
    {
        return projectileSpeed;
    }

    public string GetAnimatorTrigger()
    {
        return AnimatorTriggerName;
    }
}
