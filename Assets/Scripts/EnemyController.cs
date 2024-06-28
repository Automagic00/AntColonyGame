using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : Entity
{
    public float aggroRangeStart = 5;
    public float aggroRangeEnd = 10;

    public float attackRange = 3;
    public float attackCooldown = 4;
    private bool attackOnCooldown = false;

    private GameObject player;
    private bool aggro = false;
    public enum attackType
    {
        None = 0,
        Melee = 1,
        Ranged = 2,
        Hybrid = 3
    }

    public attackType enemyAttackType = attackType.None;
    public int facing;

    public override void Start()
    {
        player = GameObject.Find("Player");

        facing = (int)Mathf.Sign(transform.localScale.x);

        base.Start();
    }

    public override void FixedUpdate()
    {
        // TryMove(0, 0, 0);
        if (GetCurrentSubState() != EntitySubStates.Hurt && GetCurrentSubState() != EntitySubStates.Dead)
        {
            CheckAggro();
            if (aggro == false)
            {
                Patrol();
            }
            else
            {
                AggroSequence();
            }
        }
        base.FixedUpdate();
    }

    public void CheckAggro()
    {
        if (aggro == false && Vector2.Distance(player.transform.position, transform.position) < aggroRangeStart)
        {
            aggro = true;
        }
        if ((aggro == true && Vector2.Distance(player.transform.position, transform.position) > aggroRangeEnd) || player.GetComponent<PlayerController>().GetCurrentSubState() == EntitySubStates.Dead)
        {
            aggro = false;
        }
    }

    public void AggroSequence()
    {
        facing = (int)Mathf.Sign(transform.localScale.x);
        bool ledge = !Physics2D.Raycast(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), -col.bounds.extents.y), Vector2.down, 0.2f, LayerMask.GetMask("Ground"));
        bool wall = Physics2D.Raycast(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), 0), new Vector2((float)(0.1 * facing), 0), col.bounds.extents.y + 0.1f, LayerMask.GetMask("Ground"));

        bool jump;
        int direction;

        if (attackOnCooldown == false)
        {
            //If Enemy Has an Attack type and is in range
            if (enemyAttackType == attackType.Melee && Vector2.Distance(player.transform.position, transform.position) < attackRange)
            {
                Attack();
                jump = false;
                direction = 0;
                StartCoroutine(AttackCooldown());
            }
            else if (enemyAttackType == attackType.Ranged && Vector2.Distance(player.transform.position, transform.position) < attackRange)
            {
                FireProjectile(projectiles[Random.Range(0, projectiles.Length - 1)]);
                jump = false;
                direction = 0;
                StartCoroutine(AttackCooldown());
            }
            else
            {
                jump = ledge || wall;
                direction = (int)Mathf.Sign(player.transform.position.x - transform.position.x);
            }
        }
        else
        {
            jump = false;
            direction = 0;
        }

        Debug.Log(jump);
        Move(direction, jump ? 1 : 0, jump);
    }

    public void Patrol()
    {
        bool ledge = !Physics2D.Raycast(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), -col.bounds.extents.y), Vector2.down, 0.2f, LayerMask.GetMask("Ground"));
        bool wall = Physics2D.Raycast(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), 0), new Vector2((float)(0.1 * facing), 0), col.bounds.extents.y + 0.1f, LayerMask.GetMask("Ground"));
        Debug.DrawRay(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), -col.bounds.extents.y), Vector2.down);
        Debug.DrawRay(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), 0), new Vector2((float)(0.1 * facing), 0));
        //Debug.Log(ledge);

        if (ledge == true || wall == true) //If there is a ledge or wall, turn around
        {
            facing *= -1;
            //transform.localScale *= new Vector2(-1,1);
        }

        Move(facing, 0, false);
    }

    public IEnumerator AttackCooldown()
    {
        attackOnCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        attackOnCooldown = false;
    }

    public override void Die()
    {
        col.excludeLayers = (int)Mathf.Pow(2, 7);
        rb.excludeLayers = (int)Mathf.Pow(2, 7);
        base.Die();
    }
    public bool GetAggro() => aggro;
}
