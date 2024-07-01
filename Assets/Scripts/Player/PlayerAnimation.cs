using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerAnimation : MonoBehaviour
{
    public Entity entity;
    private Animator animator;
    public new ParticleSystem particleSystem;
    public ParticleSystemRenderer psr;
    public Inventory inventory;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Health", entity.GetCurrentHealth());
        Entity.EntityStates currentState = entity.GetState();
        Entity.EntitySubStates currentSubState = entity.GetCurrentSubState();

        if (currentSubState == Entity.EntitySubStates.Hurt)
        {
            animator.SetTrigger("Hurt");
            animator.ResetTrigger("Attack");
        }
        else if (currentSubState == Entity.EntitySubStates.Atk)
        {
            animator.SetTrigger("Attack");
            //animator.ResetTrigger("Hurt");
        }
        else if (currentSubState == Entity.EntitySubStates.Dodge)
        {
            animator.SetTrigger("Dodge");
        }
        else
        {
            animator.ResetTrigger("Hurt");
            animator.ResetTrigger("Attack");
            animator.ResetTrigger("Dodge");
        }


        if (currentState == Entity.EntityStates.Ground)
        {
            animator.SetFloat("Walking", Mathf.Abs(entity.GetCurrentXVelocity()));

            animator.SetBool("IsGrounded", true);
        }
        else if (currentState == Entity.EntityStates.Air)
        {
            animator.SetBool("IsGrounded", false);
            animator.SetFloat("YVelo", entity.GetCurrentYVelocity());
        }

        if (entity.GetComponent<EnemyController>() != null)
        {
            animator.SetBool("Aggro", entity.GetComponent<EnemyController>().GetAggro());
        }

        if (inventory != null)
        {
            if (inventory.weapon != null)
            {
                animator.SetBool("MeleeWeaponEquipped", inventory.weapon.weaponType == Weapon.WeaponType.Melee ? true : false);
                animator.SetBool("RangedWeaponEquipped", inventory.weapon.weaponType == Weapon.WeaponType.Ranged ? true : false);
            }
            else
            {
                animator.SetBool("MeleeWeaponEquipped", false);
                animator.SetBool("RangedWeaponEquipped", false);
            }
        }

        animator.SetFloat("AttackSpeed", entity.attackSpeed);
    }
    public void EndHurt()
    {
        entity.EndHurt();
    }
    public void EndAttack()
    {
        entity.EndAttack();
    }
    public void CreateHitbox(int i)
    {
        entity.CreateHitbox(i);
    }
    public void StartInvuln()
    {
        entity.StartInvuln();
    }
    public void EndInvuln()
    {
        entity.EndInvuln();
    }
    public void EndDodge()
    {
        entity.EndDodge();
    }
    public void FireProjectile(int i)
    {
        entity.FireProjectile(entity.projectiles[i]);
    }

    public void AddVelocity(float speed)
    {
        entity.rb.AddForce(new Vector2(speed * Mathf.Sign(entity.rb.transform.localScale.x), 0), ForceMode2D.Impulse);
    }

    public void AttackSplash(int i)
    {
        var main = particleSystem.main;

        int flip = Mathf.Sign(entity.transform.localScale.x) == -1 ? 1 : 0;
        psr.flip = new Vector3(flip, 0, 0);
        particleSystem.transform.localRotation = Mathf.Sign(entity.transform.localScale.x) == -1 ? Quaternion.Euler(-90, -180, 0) : Quaternion.Euler(-90, 0, 0);

        main.flipRotation = Mathf.Sign(entity.transform.localScale.x) == -1 ? 1 : 0;

        psr.material = entity.attacks[i].splash.splashMaterial;
        main.startSize3D = true;
        main.startSizeX = entity.attacks[i].splash.splashScale.x;
        main.startSizeY = entity.attacks[i].splash.splashScale.y;
        main.startSpeed = entity.attacks[i].splash.speed;

        particleSystem.Play();
    }
}

