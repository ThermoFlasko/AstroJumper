using System.Collections;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
// This is where the boss decision making will go
public class BossAI : Unit
{
    private static WaitForSeconds _waitForSeconds0_4 = new WaitForSeconds(0.4f);
    public GameObject[] BossZones;
    public Transform[] BossPlacementTransforms;
    public Transform AttackOneTransform;
    public Transform AttackThreeTransform;
    public Player player;
    public int PlayerInZone;
    public Animator animator;
    public string currentAttack;

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
            print("give room gameobject");
            return false;
        }

        if(currZone.PlayerInZone)
        {
            return true;
        }
        return false;
    }

    public GameObject GetZonePlayerIn()
    {
        foreach (GameObject zone in BossZones)
        {
            BossRoomZone currZone = zone.GetComponent<BossRoomZone>();

            if (currZone.PlayerInZone)
            {
                return zone;
            }
        }
        return null;
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
        currentAttack = "Attack1";
        animator.SetTrigger("Vanish");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0 && animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "VanishDelay");
        yield return new WaitForSecondsRealtime(1.6f);
        Transform bossPos = gameObject.GetComponent<Transform>();
        bossPos.position = AttackOneTransform.position;

        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Stationary attack");

        print("attack ready");
        BeginAttack(hitBoxPrefab);
        yield return null;
        
    }

    public IEnumerator DoAttackTwo()
    {
        currentAttack = "Attack2";
        animator.SetTrigger("Vanish");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0);
        yield return new WaitForSeconds(1.6f);
        Transform goToTransform = FindNearestLocationToPlayer();
        Transform bossPos = gameObject.GetComponent<Transform>();
        bossPos.position = goToTransform.position;
        
        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Move Attack");
        print("doing actually hitbox");
        BeginAttack(hitBoxPrefab2);
        yield return null;
    }

    public IEnumerator DoAttackThree()
    {
        currentAttack = "Attack3";
        animator.SetTrigger("Vanish");
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0);
        yield return new WaitForSeconds(1.6f);
        Transform bossPos = gameObject.GetComponent<Transform>();
        bossPos.position = AttackThreeTransform.position;

        yield return new WaitUntil(() => animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Air Attack_Clip");
        yield return new WaitForSeconds(1.5f);
        BeginAttack(hitBoxPrefab3);

        animator.SetTrigger("AttackOver");
    }

    public Transform FindNearestLocationToPlayer()
    {
        // Find nearest possible boss location
        // Transform nearestToPlayer = null;
        // float minDis = 100000.0f;
        // foreach(Transform transform in BossPlacementTransforms)
        // {
        //     float dis = Vector3.Distance(transform.position, player.GetComponent<Transform>().position);

        //     if (dis < minDis)
        //     {
        //         minDis = dis;
        //         nearestToPlayer = transform;
        //     }                
        // }
        // return nearestToPlayer;

        GameObject playerInZone = GetZonePlayerIn();
        Transform nearestToPlayer = playerInZone.transform.GetChild(0).transform;
        print($"nearest transform is {nearestToPlayer.name}");
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
