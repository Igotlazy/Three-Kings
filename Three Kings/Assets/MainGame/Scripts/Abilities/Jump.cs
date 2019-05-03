using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Jump : Ability
{
    [Header("Jump:")]
    public float jumpVelocity = 15.5f;
    public bool isJumping;
    public bool coyote;
    public float coyoteTime = 0.05f;

    [Header("Extra Jump:")]
    public float maxExtraJumps = 1;
    public float currentExtraJumps;
    public float extraJumpMultiplier = 0.8f;

    [Header("References:")]
    public GameObject extraJumpParticles;

    public Action onRegularJump;
    public Action onExtraJump;

    private float coyoteTracker;


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

    protected override void Update()
    {
        CoyoteFrames();
    }

    public override void AbilityUpdate()
    {
        base.AbilityUpdate();
        GroundSet();
    }

    protected override void CastAbilityImpl()
    {
        JumpMain();
    }

    public override void Cancel()
    {
        base.Cancel();
        if (isJumping && aEntity.EntityVelocity.y > 0)
        {
            JumpCancel();
            aEntity.gravity.ModifierValue = 0;
        }
    }

    private void JumpMain()
    {
        if ((aEntity.IsGrounded || coyote) && !isJumping)
        {
            onRegularJump?.Invoke();

            aEntity.gravity.ModifierValue = jumpVelocity;

            isJumping = true;

            coyote = false;
            coyoteTracker = 0;
        }
        // Extra Jumps
        else if (currentExtraJumps > 0)
        {
            onExtraJump?.Invoke();

            aEntity.gravity.ModifierValue = jumpVelocity * extraJumpMultiplier;
            Instantiate(extraJumpParticles, new Vector3(transform.position.x, transform.position.y - aEntity.EntityBC2D.bounds.size.y / 2, 0f), Quaternion.identity);
            isJumping = true;

            currentExtraJumps -= 1;
        }
    }

    private void JumpCancel()
    {
        coyote = false;
        coyoteTracker = 0;
        isJumping = false;
    }

    private void GroundSet()
    {
        if(aEntity.IsGrounded)
        {
            if(aEntity.EntityVelocity.y == 0)
            {
                JumpCancel();
                currentExtraJumps = maxExtraJumps;
            }
        }
    }

    private void CoyoteFrames()
    {
        if (coyote)
        {
            if(coyoteTracker > coyoteTime)
            {
                coyote = false;
            }
            coyoteTracker += Time.deltaTime;
        }
        if(!aEntity.IsGrounded && lastFrameGrounded)
        {
            coyote = true;
            coyoteTracker = 0;
        }

        lastFrameGrounded = aEntity.IsGrounded;
    }
    bool lastFrameGrounded;
}
