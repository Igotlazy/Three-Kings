using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Jump : Ability
{
    [Header("Jump:")]
    public float jumpVelocity = 15.5f;
    public bool isJumping;

    [Header("Extra Jump:")]
    public float maxExtraJumps = 1;
    public float currentExtraJumps;
    public float extraJumpMultiplier = 0.8f;

    [Header("References:")]
    public GameObject extraJumpParticles;

    public Action onRegularJump;
    public Action onExtraJump;


    public override bool AbilityActivated
    {
        get
        {
            return base.AbilityActivated;
        }
        set
        {
            base.AbilityActivated = value;
            if (value && aEntity.IsGrounded)
            {
                currentExtraJumps = maxExtraJumps;
            }
            else
            {
                currentExtraJumps = 0;
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
        currentExtraJumps = maxExtraJumps;
    }

    protected override void Start()
    {
        base.Start();
    }


    public override void AbilityUpdate()
    {
        base.AbilityUpdate();
    }

    public override void AbilityFixedUpdate()
    {
        base.AbilityFixedUpdate();
        GroundSet();
    }

    protected override void CastAbilityImpl()
    {
        PJump();
    }

    public override void Cancel()
    {
        base.Cancel();
        if (isJumping)
        {
            PJumpCancel();
        }
    }

    private void PJump()
    {
        if (aEntity.IsGrounded)
        {
            onRegularJump?.Invoke();

            aEntity.entityRB2D.velocity = new Vector2(aEntity.entityRB2D.velocity.x, jumpVelocity);
            isJumping = true;
        }
        // Extra Jumps
        else if (currentExtraJumps > 0)
        {
            onExtraJump?.Invoke();

            aEntity.entityRB2D.velocity = new Vector2(aEntity.entityRB2D.velocity.x, jumpVelocity * extraJumpMultiplier);
            Instantiate(extraJumpParticles, new Vector3(transform.position.x, transform.position.y - aEntity.entityBC2D.bounds.size.y / 2, 0f), Quaternion.identity);
            isJumping = true;

            currentExtraJumps -= 1;
        }
    }

    private void PJumpCancel()
    {
        if (aEntity.entityRB2D.velocity.y > 0)
        {
            aEntity.entityRB2D.velocity = new Vector2(aEntity.entityRB2D.velocity.x, 0);
        }
    }

    private void GroundSet()
    {
        if(isJumping && aEntity.IsGrounded && aEntity.entityRB2D.velocity.y <= 0)
        {
            isJumping = false;
            currentExtraJumps = maxExtraJumps;
        }
    }
}
