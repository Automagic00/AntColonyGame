using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    float hSpeed = 0f;
    float vSpeed = 0f;
    bool jumpKeyDown = false;

    public float jumpSpeed = 10.0f;
    public float defaultGroundSpeed = 12.0f;
    public float defaultAirSpeed = 6.0f;
    public float defaultSpeed;

    private float finalMoveSpeed;
    float fricForce;

    private float distanceToGround;
    private bool grounded;

    public float frictionCoefficient = 0.96f;
    public float grav = 2.25f;
    private Rigidbody2D rb;
    private BoxCollider2D col;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        distanceToGround = col.bounds.extents.y;
        rb.gravityScale = grav;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void TryMove(float hIn, float vIn,float jumpIn)
    {
        //Set Default Speed
        defaultSpeed = grounded ? defaultGroundSpeed : defaultAirSpeed;

        //Horizontal Movement Forces
        hSpeed = hIn * defaultSpeed;
        fricForce = frictionCoefficient * rb.velocity.x;

        //Vertical Movement Forces
        vSpeed = (jumpIn * jumpSpeed) * (grounded?1:0);

        //Apply Forces
        rb.AddForce(new Vector2(hSpeed - fricForce, 0));
        rb.AddForce(new Vector2(0, vSpeed), ForceMode2D.Impulse);
        
        //Stop at Low Speed
        if (Mathf.Abs(rb.velocity.x) < 1)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    //
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.transform.tag == "Ground" && Physics2D.Raycast(rb.position, Vector2.down, distanceToGround + 0.5f, LayerMask.GetMask("Ground")))
        {
            grounded = true;
        } 
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.transform.tag == "Ground")
        {
            grounded = false;
        }
    }
}
