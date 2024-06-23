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
        Dead = 3,
        Dodge = 4
    }

    // private const int ST_GROUND = 0, ST_AIR = 1, ST_GROUND_ATK = 2;
    private EntityStates state = EntityStates.Ground;
    private EntitySubStates subState = EntitySubStates.None;

    public float defaultJumpSpeed = 10.0f;
    private float jumpSpeed;
    public float defaultGroundSpeed = 12.0f;
    private float groundSpeed;
    public float defaultAirSpeed = 6.0f;
    private float airSpeed;
    public int defaultJumps = 1;
    private int jumps;

    private int currentJumps = 0;

    public float groundFrictionCoefficient = 0.96f;
    public float airFrictionCoefficient = 0.4f;
    public float grav = 2.25f;

    public float defaultMaxHealth = 100;
    [HideInInspector] public float maxHP;
    public float currentHealth;
    public bool hurt;
    private bool invuln = false;

    public float defaultRollSpeed = 12;
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
            HitboxData convert = new HitboxData(origin, size, duration, damage, knockback);
            return convert;
        }
    }
    public HitboxDataClass[] attacks;

    bool below;

    private Rigidbody2D rb;
    private BoxCollider2D col;

    public virtual void Awake()
    {
        // Init all stats to default
        ModifyStats(1, 1, 1, 0, 1);
    }

    public virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        rb.gravityScale = grav;
        currentHealth = maxHP;
    }

    public void ModifyStats(float groundSpeedMod, float airSpeedMod, float jumpSpeedMod, int doubleJumpMod, float hpMod)
    {
        groundSpeed = defaultGroundSpeed * groundSpeedMod;
        airSpeed = defaultAirSpeed * airSpeedMod;
        jumpSpeed = defaultJumpSpeed * jumpSpeedMod;
        jumps = defaultJumps + doubleJumpMod;

        // Keep hp:maxHP the same before and after
        float hpRatio = (maxHP != 0) ? currentHealth / maxHP : 1;
        maxHP = defaultMaxHealth * hpMod;
        currentHealth = maxHP * hpRatio;
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
                    currentJumps = jumps;
                }
                break;
            case EntityStates.Air:
                if (below)
                {
                    state = EntityStates.Ground;
                    currentJumps = jumps;
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

        //Horizontal Movement Forces
        float hSpeed = hIn * ((state == EntityStates.Ground) ? groundSpeed : airSpeed);
        rb.AddForce(new Vector2(hSpeed, 0));

        //Vertical Movement Forces
        if (jumpIn != 0 && currentJumps > 0)
        {
            float vSpeed = jumpIn * jumpSpeed;
            rb.velocity = new Vector2(rb.velocity.x, vSpeed);
            currentJumps--;
        }

        // Airtime & fastfall
        if (vIn == 0)
            rb.gravityScale = grav;
        else if (vIn < 0 && rb.velocity.y > 0) rb.gravityScale = grav * 0.65f;
        else if (vIn > 0) rb.gravityScale = grav * 2f;
    }

    public void Attack()
    {
        if (subState == EntitySubStates.None)
        {
            subState = EntitySubStates.Atk;
        }
    }

    public void Dodge()
    {
        if (subState == EntitySubStates.None)
        {
            subState = EntitySubStates.Dodge;
            Physics2D.IgnoreLayerCollision(6, 7);
            rb.velocity = new Vector2(Mathf.Sign(transform.localScale.x) * defaultRollSpeed, rb.velocity.y);
        }
    }

    public void EndDodge()
    {
        if (subState == EntitySubStates.Dodge)
        {
            subState = EntitySubStates.None;
            Physics2D.IgnoreLayerCollision(6, 7, false);
            //rb.velocity = new Vector2(Mathf.Sign(transform.localScale.x) * defaultRollSpeed, rb.velocity.y);
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
        if (subState == EntitySubStates.Atk)
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
        if (collision.collider.tag == "Ground")
        {
            below = Physics2D.BoxCast(rb.position, new Vector2(col.size.x - 0.15f, col.size.y), 0, Vector2.down, col.bounds.extents.y + 0.1f, LayerMask.GetMask("Ground"));
        }
        if (collision.collider.tag == "Enemy" && gameObject.tag == "Player" && invuln == false)
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
        StartInvuln();
        yield return new WaitForSeconds(invulnTime);
        EndInvuln();
        subState = EntitySubStates.None;
    }
    public void StartInvuln()
    {
        invuln = true;
    }
    public void EndInvuln()
    {
        invuln = false;
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
