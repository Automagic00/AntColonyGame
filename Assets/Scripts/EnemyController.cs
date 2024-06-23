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

    public override void Start()
    {
        GameObject.Find("Player");

        base.Start();
    }

    public override void Update()
    {
        // TryMove(0, 0, 0);

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
}
