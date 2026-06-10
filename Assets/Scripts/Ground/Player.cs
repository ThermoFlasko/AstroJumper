using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using MilkShake;
 
public class Player : Unit
{
    
    [SerializeField] private InputActionAsset actionsAsset; //this is jsut to test, will move to GroundMovement when it is updated
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string attackActionName = "Attack";
    [SerializeField] private string attackActionName2 = "Attack2";
    [SerializeField] private GroundAttackCatalogSO groundAttackCatalog;
   
    private InputAction attackAction;
    private InputAction attackAction2;
    public static event Action<Unit> onPlayerDeath;
    public static event Action<Unit> onPlayerDamaged;
    private bool isAttacking2 = false;

    public GameObject healthUIGameObject;
    private Animator  UIhealth;
    public Shaker MyShaker;
    public ShakePreset CameraShake;

    private PlayerAnimator playerAnimator;
    private bool inMeleeAnimation = false;





    [Header("Projectile Variables")]
    [SerializeField] private int projectileCount = 0; 
    [SerializeField] private int maxProjectile = 3;

    [Header("Player Spawnpoint")]
    [SerializeField] private GameObject playerSpawn;

    [Header("Damage Settings")]
    [SerializeField] private float damageCooldown = 0.5f;
    private float lastDamageTime = -999f;
    [SerializeField] public int startingHealth;
    public new int Health
    {
        get {return _health;}
        set 
        {
            _health = value;
            if(_health >= startingHealth)
            {
                _health = startingHealth;
            }
            onPlayerDamaged?.Invoke(this);
            if (Health <= 0)
            {
                onPlayerDeath?.Invoke(this);
                //Reset();
                return;
            }
        }
        
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        startingHealth = Health;
        var map = actionsAsset.FindActionMap(actionMapName, true);
        attackAction = map.FindAction(attackActionName);
        if (attackAction == null)
        {
            Debug.LogError("Player: Attack action not found in the InputActionAsset.");
        }
        attackAction2 = map.FindAction(attackActionName2);
        if (attackAction2 == null)
        {
            Debug.LogError("Player: Attack2 action not found in the InputActionAsset.");
        }
        RefreshGroundAttackLoadout();
        ApplyGroundTrooperDefaultUpgrades();
        if (healthUIGameObject != null)
        {
            UIhealth = healthUIGameObject.GetComponent<Animator>();
        }
        else
        {
            Debug.LogWarning($"{name} is missing a health UI GameObject. Damage UI animations will be skipped.", this);
        }

        playerAnimator = null;
        playerAnimator = GetComponent<PlayerAnimator>();

   }
    private void OnEnable()
    {
        ApplyGroundTrooperDefaultUpgrades();

        attackAction.Enable();
        attackAction.performed += OnAttack;

        attackAction2.Enable();
        attackAction2.performed += OnAttack2;

        HitBox.onDurationOver += OnHitBoxDurationOver;
    }

    private void OnDisable()
    {
        attackAction.performed -= OnAttack;
        attackAction.Disable();

        attackAction2.performed -= OnAttack2;
        attackAction2.Disable();

        HitBox.onDurationOver -= OnHitBoxDurationOver;
    }

    private void OnAttack(InputAction.CallbackContext context) //GUN ATTACK
    {
        if (isAttacking)
            return;

        if (!TryGetHitBox(hitBoxPrefab, nameof(hitBoxPrefab), out HitBox hitBoxInfo))
            return;

        // check for projectile attack
        if(unitProjectilePool && projectileCount < maxProjectile && !hitBoxInfo.GetIsMelee())
        {
            projectileCount++;
            
            BeginAttack(hitBoxPrefab);
            return;
        }
        
        else if(projectileCount < maxProjectile)
        {
            //print("no projectile pool, creating projectile");
            BeginAttack(hitBoxPrefab);
            return;
        }
    }

    private void OnAttack2(InputAction.CallbackContext context) //MELEE ATTACK 
    {
        if (isAttacking2)
            return;

        if (!TryGetHitBox(hitBoxPrefab2, nameof(hitBoxPrefab2), out HitBox hitBoxInfo))
            return;

        //// check for projectile attack
        //if(unitProjectilePool && projectileCount < maxProjectile && !hitBoxInfo.GetIsMelee())
        //{
        //    //print("Projectile attack from pool");
        //    projectileCount++;
        //    BeginAttack(hitBoxPrefab2);
        //    return;
        //}

        //Changing and moving attack into Player script
        //else if(hitBoxInfo.GetIsMelee())
        //{
        //    print("melee attack");
        //    BeginAttack(hitBoxPrefab2);
        //    isAttacking2 = true;
        //    return;
        //}
        //else if(projectileCount < maxProjectile)
        //{
        //    //print("no projectile pool, creating projectile");
        //    BeginAttack(hitBoxPrefab2);
        //    return;
        //}
        if (playerAnimator.isGrounded && !inMeleeAnimation)
        {
            //Debug.Log("Hit the melee");
            inMeleeAnimation = true;
            DisableInputs();
            playerAnimator.MakePlayerMelee();
            OSCHandler.Instance.SendMessageToClient("pd", "/unity/melee", 1);
            print("sounds");
        }




    }

    public override void TakeDamage(int amount, float knockbackForce, float knockbackVerticalForce, Vector2 sourcePosition)
    {
        // Ignore hits that happen too close together
        if (Time.time - lastDamageTime < damageCooldown)
            return;
        lastDamageTime = Time.time;

        //print("Taking damage");
        Health -= amount;
        if (UIhealth != null)
        {
            UIhealth.SetTrigger("IsDamaged");
        }

        Vector2 knockbackDir = ((Vector2)transform.position - sourcePosition).normalized;
        Vector2 knockbackVector = new Vector2(knockbackDir.x * knockbackForce, knockbackVerticalForce);
        InvokeKnockback(this, knockbackVector);
        MyShaker.Shake(CameraShake);

      SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (!isDamageAnimation)
            StartCoroutine(DamageEffect(spriteRenderer)); //Im not sure why it plays twice after taking dmg once It was like this before and flashes twice im not sure where it is, 
        Debug.Log("Invoking onPlayerDamaged animation");// the animation plays an extra time after taking dmg like a second later, but this isn't being called twice so its most likely a sprite issue

    }

    private void OnHitBoxDurationOver(int attackIndex, string owner)
    {
        if (owner != this.gameObject.name)
        {
            return;
        }
    
        if(attackIndex == 1)
        {
            isAttacking = false;
            if(TryGetHitBox(hitBoxPrefab, nameof(hitBoxPrefab), out HitBox hitBoxInfo) && !hitBoxInfo.GetIsMelee())
            {
                projectileCount--;
            }
        }
        else if(attackIndex == 2)
        {
            isAttacking2 = false;
            if(TryGetHitBox(hitBoxPrefab2, nameof(hitBoxPrefab2), out HitBox hitBoxInfo) && !hitBoxInfo.GetIsMelee())
            {
                projectileCount--;
            }
        }
    }

    // This should NOT be final, it is only meant to be used for the playtest
    private void Reset()
    {
        ApplyGroundTrooperDefaultUpgrades();
        transform.position = playerSpawn.transform.position;
        GetComponent<SpriteRenderer>().flipX = false;

    }

    private void ApplyGroundTrooperDefaultUpgrades()
    {
        int healthUpgrade = SaveManager.instance != null ? SaveManager.instance.GetGroundMaxHealthUpgradeBoost() : 0;
        Health = startingHealth + healthUpgrade;
    }

    public void RefreshGroundAttackLoadout()
    {
        ApplySavedGroundAttackLoadout();
        AssignAttackListIndices();
    }

    private void ApplySavedGroundAttackLoadout()
    {
        if (groundAttackCatalog == null)
        {
            Debug.LogWarning($"{name} is missing a ground attack catalog. Using the hitbox prefabs already assigned on the player.", this);
            return;
        }

        ApplyGroundAttackToSlot(GroundAttackType.Ranged, ref hitBoxPrefab);
        ApplyGroundAttackToSlot(GroundAttackType.Melee, ref hitBoxPrefab2);
    }

    private void ApplyGroundAttackToSlot(GroundAttackType attackType, ref GameObject slot)
    {
        string equippedAttackId = SaveManager.instance != null
            ? SaveManager.instance.GetEquippedGroundAttackId(attackType)
            : string.Empty;

        GroundAttackDefinition attack = groundAttackCatalog.GetSafeAttack(equippedAttackId, attackType);
        if (attack == null)
        {
            Debug.LogWarning($"{name} could not resolve a {attackType} ground attack. Check the ground attack catalog defaults.", this);
            return;
        }

        slot = attack.HitBoxPrefab;

        //if (attackType == GroundAttackType.Melee && attack.MeleeAttackAnimation != null)
        //{
        //    meleeAnimator = attack.MeleeAttackAnimation;
        //}
    }

    private void AssignAttackListIndices()
    {
        AssignAttackListIndex(hitBoxPrefab, 1, nameof(hitBoxPrefab));
        AssignAttackListIndex(hitBoxPrefab2, 2, nameof(hitBoxPrefab2));
    }

    private void AssignAttackListIndex(GameObject attackPrefab, int attackListIndex, string slotName)
    {
        if (!TryGetHitBox(attackPrefab, slotName, out HitBox hitBox))
            return;

        hitBox.attackListIndex = attackListIndex;
    }

    private bool TryGetHitBox(GameObject attackPrefab, string slotName, out HitBox hitBox)
    {
        hitBox = null;

        if (attackPrefab == null)
        {
            Debug.LogWarning($"{name} is missing {slotName}.", this);
            return false;
        }

        hitBox = attackPrefab.GetComponent<HitBox>();
        if (hitBox == null)
        {
            Debug.LogWarning($"{name}'s {slotName} does not have a HitBox component.", this);
            return false;
        }

        return true;
    }

    public void DisableInputs()
    {
        attackAction.Disable();
        attackAction2.Disable();
    }

    public void EnableInputs()
    {
        attackAction.Enable();
        attackAction2.Enable();
        inMeleeAnimation = false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Player))]
public class PlayerInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if(GUILayout.Button("Deal Damage"))
        {
            Player player = (Player)target;

            player.Health -= 20;
        }
    }
}
#endif
