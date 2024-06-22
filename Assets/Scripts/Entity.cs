using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum EntityStates
    {
        Ground = 0,
        Air = 1,
        
    }
    public enum EntitySubStates
    {
        None = 0,
        Atk = 1,
        Hurt = 2,
        Dead = 3
    }

    // private const int ST_GROUND = 0, ST_AIR = 1, ST_GROUND_ATK = 2;
    private EntityStates state = EntityStates.Ground;
    private EntitySubStates subState = EntitySubStates.None;

    public float jumpSpeed = 10.0f;
    public float defaultGroundSpeed = 12.0f;
    public float defaultAirSpeed = 6.0f;
    public float defaultSpeed;
    public int defaultJumps = 1;
    private int jumps = 0;

    public float groundFrictionCoefficient = 0.96f;
    public float airFrictionCoefficient = 0.4f;
    public float grav = 2.25f;

    public float maxHealth = 100;
    public float currentHealth;
    public bool hurt;
    private bool invuln = false;

    public float defaultInvulnTime = 0.2f;


    
    [System.Serializable]
    public class HitboxDataClass
    {
        public Vector2 origin;
        public Vector2 size;
        public float duration;
        public float damage;
        public float knockback;

        public HitboxData Convert()
        {
            HitboxData convert = new HitboxData(origin, size,duration,damage,knockback);
            return convert;
        }
    }
    public HitboxDataClass[] attacks;

    bool below;

    private Rigidbody2D rb;
    private BoxCollider2D col;

    // Start is called before the first frame update
    public virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        rb.gravityScale = grav;
        currentHealth = maxHealth;
    }

    void updateState()
    {
        //var below = Physics2D.Raycast(rb.position, Vector2.down, col.bounds.extents.y + 0.5f, LayerMask.GetMask("Ground"));
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
                    if (subState == EntitySubStates.Hurt)
                    {
                        subState = EntitySubStates.None;
                    }
                }
                break;
        }
    }


    public void Move(float hIn, float vIn, float jumpIn)
    {
        if (PauseController.gameIsPaused)
            return;

        defaultSpeed = (state == EntityStates.Ground) ? defaultGroundSpeed : defaultAirSpeed;

        //Horizontal Movement Forces
        float hSpeed = hIn * defaultSpeed;
        rb.AddForce(new Vector2(hSpeed, 0));

        //Vertical Movement Forces
        if (jumpIn != 0 && jumps > 0)
        {
            float vSpeed = jumpIn * jumpSpeed;
            rb.velocity = new Vector2(rb.velocity.x, vSpeed);
            jumps--;
        }

        // Airtime & fastfall
        if (vIn == 0)
            rb.gravityScale = grav;
        else if (vIn < 0 && rb.velocity.y > 0) rb.gravityScale = grav * 0.65f;
        else if (vIn > 0) rb.gravityScale = grav * 2f;
    }

    public void Attack()
    {
        if(subState == EntitySubStates.None)
        {
            subState = EntitySubStates.Atk;
        }
    }

    public void CreateHitbox(int i)
    {
        Vector2 hitboxOffset = new Vector3(1, 0, 0) * Mathf.Sign(transform.localScale.x);
        HitboxData hitbox = attacks[i].Convert();

        Hitbox.CreateHitbox(hitbox, this);
    }

    public void EndAttack()
    {
        if(subState == EntitySubStates.Atk)
        {
            subState = EntitySubStates.None;
        }
    }

    public void EndHurt()
    {
        if (subState == EntitySubStates.Hurt)
        {
            subState = EntitySubStates.None;
        }
    }

    public virtual void Update()
    {
        if (PauseController.gameIsPaused)
            return;

        // Friction
        float frictionCoefficient = (state == EntityStates.Ground) ? groundFrictionCoefficient : airFrictionCoefficient;
        float fricForce = frictionCoefficient * rb.velocity.x;
        rb.AddForce(new Vector2(-fricForce, 0));

        //Stop at Low Speed
        if (Mathf.Abs(rb.velocity.x) < 0.2)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        /*Debug.Log(collision.tag);
        Debug.Log("This " + gameObject.tag);*/

        if (collision.collider.tag == "Ground")
        {
            below = Physics2D.BoxCast(rb.position, new Vector2(col.size.x - 0.15f, col.size.y), 0, Vector2.down, col.bounds.extents.y + 0.1f, LayerMask.GetMask("Ground"));
            
            

        }
        else if (collision.collider.tag == "Enemy" && gameObject.tag == "Player" && invuln == false)
        {
            HitboxData hitboxData = new HitboxData(Vector2.zero, Vector2.zero, 0, 10, 8);
            Hurt(hitboxData, collision.collider.gameObject);
        }

        updateState();
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == "Ground")
        {
            below = false;
        }
        updateState();
    }


    public EntityStates GetState() => state;
    public float GetCurrentXVelocity() => rb.velocity.x;

    public float GetCurrentYVelocity() => rb.velocity.y;

    public float GetCurrentHealth() => currentHealth;

    public EntitySubStates GetCurrentSubState() => subState;

    public IEnumerator DestroyHitbox(float lifetime, BoxCollider2D hitbox)
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(hitbox.gameObject);
    }

    public IEnumerator DamageInvulnerabilityPeriod(float invulnTime)
    {
        invuln = true;
        yield return new WaitForSeconds(invulnTime);
        invuln = false;
        subState = EntitySubStates.None;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.tag == "Hitbox" && invuln == false)
        {
            HitboxData hitboxData = collision.GetComponent<HitboxData>();
            Hurt(hitboxData, collision.transform.parent.gameObject);
        }
        
    }

    private void Hurt(HitboxData hitboxData, GameObject owner)
    {
        subState = EntitySubStates.Hurt;
        currentHealth -= hitboxData.damage;
        rb.velocity = new Vector2(hitboxData.knockback * Mathf.Sign(transform.position.x - owner.transform.position.x), 5);

        StartCoroutine(DamageInvulnerabilityPeriod(defaultInvulnTime));
    }
}
