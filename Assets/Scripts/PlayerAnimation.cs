using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerAnimation : MonoBehaviour
{
    public Entity entity;
    private Animator animator;

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
}

