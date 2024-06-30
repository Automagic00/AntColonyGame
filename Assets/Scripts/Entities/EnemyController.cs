using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Pathfinding;

public class EnemyController : Entity
{
    public float aggroRangeStart = 5;
    public float aggroRangeEnd = 10;

    private Vector2 spawnPoint;
    public float waypointDistance = 3f;
    Path path;
    Vector2 targetPosition;
    int currentWaypoint = 0;
    bool reachedEndOfPath;
    Seeker seeker;

    public float attackRange = 3;
    public float attackCooldown = 4;
    private bool attackOnCooldown = false;
    private Vector2 angle;

    private GameObject player;
    private bool aggro = false;
    public float aggroSpeedMod = 1;
    public enum attackType
    {
        None = 0,
        Melee = 1,
        Ranged = 2,
        Hybrid = 3
    }
    public enum movementType
    {
        Stationary = 0,
        Ground = 1,
        Flying = 2
    }    

    public attackType enemyAttackType = attackType.None;
    public movementType enemyMovementType = movementType.Ground;
    public float flyingPatrolRange = 3f;
    public int facing;

    public override void Start()
    {
        player = GameObject.Find("Player");

        if (enemyMovementType == movementType.Flying)
        {
            seeker = GetComponent<Seeker>();
            spawnPoint = rb.position;
            targetPosition = spawnPoint;

            InvokeRepeating("UpdatePath", 0f, 0.5f);
            InvokeRepeating("UpdatePatrolPoint", 0f, 4f);
            rb.drag = airFriction;

        }

        facing = (int)Mathf.Sign(transform.localScale.x);

        base.Start();
    }

    void UpdatePath()
    {
        if(seeker.IsDone())
            seeker.StartPath(rb.position, targetPosition, OnPathComplete);
    }

    void UpdatePatrolPoint()
    {
        if (aggro)
            return;

        
        targetPosition = new Vector2(Random.Range(-flyingPatrolRange, flyingPatrolRange) + spawnPoint.x, Random.Range(-flyingPatrolRange, flyingPatrolRange) + spawnPoint.y);
        
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
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

        bool jump = false;
        int direction = 0;

        if (attackOnCooldown == false)
        {
            //Ground Enemy Aggro Path
            if (enemyMovementType == movementType.Ground)
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
                    angle = transform.position - player.transform.position;
                    angle = angle.normalized;
                    Attack(-angle.x, -angle.y);
                    //FireProjectile(projectiles[Random.Range(0, projectiles.Length - 1)],angle);
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
            //Flying Enemy Aggro Path
            else if (enemyMovementType == movementType.Flying)
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
                    angle = transform.position - player.transform.position;
                    angle = angle.normalized;
                    Attack(-angle.x, -angle.y);
                    //FireProjectile(projectiles[Random.Range(0, projectiles.Length - 1)],angle);
                    jump = false;
                    direction = 0;
                    StartCoroutine(AttackCooldown());
                }
                //Move into attack range
                else
                {
                    targetPosition = player.transform.position;
                    if (path == null)
                        return;

                    if (currentWaypoint >= path.vectorPath.Count)
                    {
                        reachedEndOfPath = true;
                        return;
                    }
                    else
                    {
                        reachedEndOfPath = false;
                    }

                    Vector2 dir = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
                    Vector2 force = dir * defaultAirSpeed;

                    rb.AddForce(force);

                    float dist = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

                    if (dist < waypointDistance)
                    {
                        currentWaypoint++;
                    }

                    Vector3 scale = transform.localScale;
                    if ((scale.x > 0 && rb.velocity.x < 0) || (scale.x < 0 && rb.velocity.x > 0))
                        transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
                }
            }
        }
        else
        {
            jump = false;
            direction = 0;
        }
        Move(direction * aggroSpeedMod, jump ? 1 : 0, jump);
        
    }

    public void Patrol()
    {
        if (enemyMovementType == movementType.Ground)
        {
            bool ledge = !Physics2D.Raycast(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), -col.bounds.extents.y), Vector2.down, 0.2f, LayerMask.GetMask("Ground"));
            bool wall = Physics2D.Raycast(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), 0), new Vector2((float)(0.1 * facing), 0), col.bounds.extents.y + 0.1f, LayerMask.GetMask("Ground"));
            bool otherEnemy = Physics2D.Raycast(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), 0), new Vector2((float)(0.1 * facing), 0), col.bounds.extents.y + 0.1f, LayerMask.GetMask("Enemy"));
            Debug.DrawRay(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), -col.bounds.extents.y), Vector2.down);
            Debug.DrawRay(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), 0), new Vector2((float)(0.1 * facing), 0));
            //Debug.Log(ledge);

            if (ledge == true || wall == true || otherEnemy == true) //If there is a ledge or wall, turn around
            {
                facing *= -1;
                //transform.localScale *= new Vector2(-1,1);
            }
        
            Move(facing, 0, false);
        }
        else if (enemyMovementType == movementType.Flying)
        {
            if (path == null)
                return;

            if (currentWaypoint >= path.vectorPath.Count)
            {
                reachedEndOfPath = true;
                return;
            }
            else
            {
                reachedEndOfPath = false;
            }

            Vector2 dir = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
            Vector2 force = dir * defaultAirSpeed;

            rb.AddForce(force);

            float dist = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);

            if (dist < waypointDistance)
            {
                currentWaypoint++;
            }

            Vector3 scale = transform.localScale;
            if ((scale.x > 0 && rb.velocity.x < 0) || (scale.x < 0 && rb.velocity.x > 0))
                transform.localScale = new Vector3(-scale.x, scale.y, scale.z);
        }
    }

    public IEnumerator AttackCooldown()
    {
        attackOnCooldown = true;
        yield return new WaitForSeconds(attackCooldown);
        attackOnCooldown = false;
    }

    public override void Die()
    {
        col.excludeLayers = (int)Mathf.Pow(2, 7) + (int)Mathf.Pow(2, 8) + (int)Mathf.Pow(2, 6) + (int)Mathf.Pow(2, 9);
        rb.excludeLayers = (int)Mathf.Pow(2, 7) + (int)Mathf.Pow(2, 8) + (int)Mathf.Pow(2, 6) + (int)Mathf.Pow(2, 9);
        base.Die();
    }
    public bool GetAggro() => aggro;
}
