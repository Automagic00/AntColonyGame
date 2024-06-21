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
        Entity.EntityStates currentState = entity.GetState();
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
}

