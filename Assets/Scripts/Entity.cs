using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum EntityStates
    {
        Ground = 0,
        Air = 1,
        Ground_Atk = 2,
        Air_Atk = 3,
        Hurt = 4
    }
    // private const int ST_GROUND = 0, ST_AIR = 1, ST_GROUND_ATK = 2;
    private EntityStates state = EntityStates.Ground;

    public float jumpSpeed = 10.0f;
    public float defaultGroundSpeed = 12.0f;
    public float defaultAirSpeed = 6.0f;
    public float defaultSpeed;
    public int defaultJumps = 1;
    private int jumps = 0;

    private float frictionCoefficient;
    public float groundFrictionCoefficient = 0.96f;
    public float airFrictionCoefficient = 0.4f;
    float fricForce;
    public float grav = 2.25f;

    public float maxHealth = 100;
    public float currentHealth;
    bool below;

    private Rigidbody2D rb;
    private BoxCollider2D col;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        rb.gravityScale = grav;
        currentHealth = maxHealth;
    }

    void updateState()
    {
        //var below = Physics2D.Raycast(rb.position, Vector2.down, col.bounds.extents.y + 0.5f, LayerMask.GetMask("Ground"));
        below = Physics2D.CapsuleCast(rb.position, col.size, CapsuleDirection2D.Vertical, 0, Vector2.down, col.bounds.extents.y + 0.1f, LayerMask.GetMask("Ground"));


        switch (state)
        {
            default:
            case EntityStates.Ground:
                if (!below)
                {
                    state = EntityStates.Air;
                }
                else
                {
                    jumps = defaultJumps;
                }
                break;
            case EntityStates.Air:
                if (below)
                {
                    state = EntityStates.Ground;
                    jumps = defaultJumps;
                }
                break;
        }


    }


    public void TryMove(float hIn, float vIn, float jumpIn)
    {
        defaultSpeed = (state == EntityStates.Ground) ? defaultGroundSpeed : defaultAirSpeed;
        frictionCoefficient = (state == EntityStates.Ground) ? groundFrictionCoefficient : airFrictionCoefficient;

        //Horizontal Movement Forces
        float hSpeed = hIn * defaultSpeed;
        fricForce = frictionCoefficient * rb.velocity.x;

        // Airtime & fastfall
        if (vIn == 0)
            rb.gravityScale = grav;
        else if (vIn < 0 && rb.velocity.y > 0) rb.gravityScale = grav * 0.65f;
        else if (vIn > 0) rb.gravityScale = grav * 2f;


        //Vertical Movement Forces
        if (jumpIn != 0 && jumps > 0)
        {
            float vSpeed = jumpIn * jumpSpeed;
            //rb.AddForce(new Vector2(0, vSpeed), ForceMode2D.Impulse);
            rb.velocity = new Vector2(rb.velocity.x, vSpeed);
            jumps--;
        }

        //Apply Forces
        rb.AddForce(new Vector2(hSpeed - fricForce, 0));

        //Stop at Low Speed
        if (Mathf.Abs(rb.velocity.x) < 0.2)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            fricForce = 0;
        }

        updateState();
    }

    public EntityStates GetState()
    {
        return state;
    }
    public float GetCurrentXVelocity()
    {
        return rb.velocity.x;
    }

    //
}
