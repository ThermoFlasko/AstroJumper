using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
// This is where the boss decision making will go
public class BossAI : Unit
{
    public GameObject[] BossZones;
    public Transform[] BossPlacementTransforms;
    public Transform AttackOneTransform;
    public Transform AttackThreeTransform;
    public Player player;
    public int PlayerInZone;
    public Animator animator;

    private void OnDisable()
    {
        HitBox.onDurationOver -= AttackOver;
    }

    private void AttackOver(int value)
    {
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateMachine.Start();
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }

    public void OnEnable()
    {
        stateMachine.ChangeState(new BossStateIdle(this));   
        HitBox.onDurationOver += AttackOver;

    }

    public IState DecideAttack()
    {
        // things to look for when deciding attack, player location, recent attacks, current health
        IState newState;

        bool[] validAttacks = new bool[3];

        AnimatorClipInfo[] animatorinfo = animator.GetCurrentAnimatorClipInfo(0);
        string name = animatorinfo[0].clip.name;
        print($"current anim {name}");
        if (name != "Idle Low")
        {
            return null;
        }

        int timesTried = 0;
        while(timesTried < 12)
        {
            // Randomly choose which attack 
            int whichAttack = Random.Range(0,100);
            if (whichAttack < 50)
            {
                whichAttack = 1;
            }
            else if (whichAttack > 50 && whichAttack < 88)
            {
                whichAttack = 0;
            }
            else
            {
                whichAttack = 2;
            }
            print($"trying {whichAttack}");
            if (whichAttack == 0 && IsPlayerInBottom())
            {
                print("Attack one");
                StartCoroutine(DoAttackOne());
                break;
            }

            if (whichAttack == 1 && !IsPlayerInMiddle())
            {
                print("Attack two");
                StartCoroutine(DoAttackTwo());
                break;
            }

            if (whichAttack == 2)
            {
                print("attack three");
                StartCoroutine(DoAttackThree());
                break;
            }
            timesTried++;
        }
        return null;
    }

    public bool PlayerIsInZone(GameObject zone)
    {
        
        BossRoomZone currZone = zone.GetComponent<BossRoomZone>();
        if (!currZone)
        {
            print("gave room gameobject");
            return false;
        }

        if(currZone.PlayerInZone)
        {
            return true;
        }
        return false;
    }

    public int WhatZoneIsPlayer()
    {
        return 0;
    }

    public bool IsPlayerInMiddle()
    {
        for (int i = 0; i < 4; i++)
        {
            if(PlayerIsInZone(BossZones[i * 3 + 1]))
            {
                print("player is in middle");
                return true;
            }
        }
        return false;
    }

    public bool IsPlayerInBottom()
    {
        for (int i = 0 ; i < 3; i++)
        {
            if (PlayerIsInZone(BossZones[i+9]))
            {
                print("Player is at the bottom");
                return true;
            }
        }
        return false;
    }

    public IEnumerator DoAttackOne()
    {
        animator.SetTrigger("Vanish");

        yield return new WaitForSeconds(0.5f);
        Transform bossPos = gameObject.GetComponent<Transform>();
        bossPos.position = AttackOneTransform.position;
        yield return new WaitForSeconds(0.2f);
        animator.ResetTrigger("Vanish");
        animator.SetTrigger("Appear");


        yield return new WaitForSeconds(0.5f);

        animator.SetTrigger("Attack1");
        yield return new WaitForSeconds(0.5f);

        BeginAttack(hitBoxPrefab);

        yield return new WaitForSeconds(1f);
        animator.SetTrigger("AttackOver");
    }

    public IEnumerator DoAttackTwo()
    {
        animator.SetTrigger("Vanish");
        yield return new WaitForSeconds(0.5f);
        Transform goToTransform = FindNearestLocationToPlayer();
        Transform bossPos = gameObject.GetComponent<Transform>();
        bossPos.position = goToTransform.position;
        yield return new WaitForSeconds(0.2f);
        animator.ResetTrigger("Vanish");
        animator.SetTrigger("Appear");


        yield return new WaitForSeconds(0.5f);
        animator.SetTrigger("Attack2");
        yield return new WaitForSeconds(0.2f);

        print("doing actually hitbox");
        BeginAttack(hitBoxPrefab2);
    
        yield return new WaitForSeconds(0.5f);
        animator.SetTrigger("AttackOver");
    }

    public IEnumerator DoAttackThree()
    {
        animator.SetTrigger("Vanish");

        yield return new WaitForSeconds(0.5f);
        Transform bossPos = gameObject.GetComponent<Transform>();
        bossPos.position = AttackThreeTransform.position;
        yield return new WaitForSeconds(0.2f);
        animator.ResetTrigger("Vanish");
        animator.SetTrigger("Appear");

        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Attack1");
        animator.SetTrigger("Attack3");
        yield return new WaitForSeconds(2.5f);

        BeginAttack(hitBoxPrefab3);

        yield return new WaitForSeconds(0.5f);
        animator.SetTrigger("AttackOver");
    }

    public Transform FindNearestLocationToPlayer()
    {
        // Find nearest possible boss location
        Transform nearestToPlayer = null;
        float minDis = 100000.0f;
        foreach(Transform transform in BossPlacementTransforms)
        {
            float dis = Vector3.Distance(transform.position, player.GetComponent<Transform>().position);

            if (dis < minDis)
            {
                minDis = dis;
                nearestToPlayer = transform;
            }                
        }
        return nearestToPlayer;
    }

    protected override bool IsFacingRight()
    {
        if (gameObject.transform.position.x < -140.1f)
        {
            gameObject.GetComponent<SpriteRenderer>().flipX = true;
            return false;
        }
        else
        {
            gameObject.GetComponent<SpriteRenderer>().flipX = false;
            return true;

        }
    }

}
