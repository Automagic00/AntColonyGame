using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private const int ST_GROUND = 0, ST_AIR = 1, ST_GROUND_ATK = 2;
    private int state = ST_AIR;

    public float jumpSpeed = 10.0f;
    public float defaultGroundSpeed = 12.0f;
    public float defaultAirSpeed = 6.0f;
    public float defaultSpeed;
    public int defaultJumps = 1;
    private int jumps = 0;

    private float frictionCoefficient = 0.96f;
    public float groundFrictionCoefficient = 0.96f;
    public float airFrictionCoefficient = 0.4f;
    public float grav = 2.25f;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        rb.gravityScale = grav;
    }

    void updateState()
    {
        //var below = Physics2D.Raycast(rb.position, Vector2.down, col.bounds.extents.y + 0.5f, LayerMask.GetMask("Ground"));
        var below = Physics2D.CapsuleCast(rb.position, col.size, CapsuleDirection2D.Vertical, 0, Vector2.down, col.bounds.extents.y + 0.5f, LayerMask.GetMask("Ground"));
        

        switch (state)
        {
            default:
            case ST_GROUND:
                if (!below)
                {
                    state = ST_AIR;
                }
                else
                {
                    jumps = defaultJumps;
                }
                break;
            case ST_AIR:
                if (below)
                {
                    state = ST_GROUND;
                    jumps = defaultJumps;
                }
                break;
        }


    }


    public void TryMove(float hIn, float vIn, float jumpIn)
    {
        defaultSpeed = (state == ST_GROUND) ? defaultGroundSpeed : defaultAirSpeed;
        frictionCoefficient = (state == ST_GROUND) ? groundFrictionCoefficient : airFrictionCoefficient;

        //Horizontal Movement Forces
        float hSpeed = hIn * defaultSpeed;
        float fricForce = frictionCoefficient * rb.velocity.x;


        // Airtime & fastfall
        if (vIn == 0)
            rb.gravityScale = grav;
        else if (vIn < 0 && rb.velocity.y > 0) rb.gravityScale = grav * 0.65f;
        else if (vIn > 0) rb.gravityScale = grav * 2f;




        //Vertical Movement Forces
        if (jumpIn != 0 && jumps > 0)
        {
            float vSpeed = jumpIn * jumpSpeed;
            rb.AddForce(new Vector2(0, vSpeed), ForceMode2D.Impulse);
            jumps--;
        }

        //Apply Forces
        rb.AddForce(new Vector2(hSpeed - fricForce, 0));

        //Stop at Low Speed
        if (Mathf.Abs(rb.velocity.x) < 1)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        updateState();
    }

    //
}
