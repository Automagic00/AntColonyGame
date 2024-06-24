using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyController : Entity
{
    public float aggroRangeStart = 5;
    public float aggroRangeEnd = 10;
    private GameObject player;
    private bool aggro = false;
    public int facing;

    public override void Start()
    {
        GameObject.Find("Player");

        facing = (int)Mathf.Sign(transform.localScale.x);

        base.Start();
    }

    public override void Update()
    {
        // TryMove(0, 0, 0);
        Patrol();
        base.Update();
    }

    public void CheckAggro()
    {
        if (aggro == false && Vector2.Distance(player.transform.position,transform.position) < aggroRangeStart)
        {
            aggro = true;
        }
        if (aggro == true && Vector2.Distance(player.transform.position, transform.position) > aggroRangeEnd)
        {
            aggro = false;
        }
    }

    public void Patrol()
    {
        bool ledge = !Physics2D.Raycast(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), -col.bounds.extents.y), Vector2.down,  0.2f, LayerMask.GetMask("Ground"));
        bool wall = Physics2D.Raycast(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), 0), new Vector2 ((float)(0.1 * facing), 0), col.bounds.extents.y + 0.1f, LayerMask.GetMask("Ground"));
        Debug.DrawRay(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), -col.bounds.extents.y),Vector2.down);
        Debug.DrawRay(rb.position + new Vector2((float)((col.bounds.extents.x + 0.1) * facing), 0), new Vector2((float)(0.1 * facing), 0));
        //Debug.Log(ledge);

        if (ledge == true || wall == true) //If there is a ledge or wall, turn around
        {
            facing *= -1;
            transform.localScale *= new Vector2(-1,1);
        }

        Move(facing, 0, 0);
    }
}
