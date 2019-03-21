using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WallJump : Ability
{

    [Header("Wall Jump:")]
    public Vector2 wallJumpForce = new Vector2(7f, 15f);
    public float wallJumpRayLength;
    public float wallFallSpeed = -4f;
    [Space]
    public float sensorGap = 0.3f;
    [Space]
    public LayerMask wallJumpMask;

    [Header("References:")]
    public Transform sensorPosition;

    public Action onWallCling;

    public override bool AbilityActivated
    {
        get
        {
            return base.AbilityActivated;
        }
        set
        {
            base.AbilityActivated = value;
            if (value == false)
            {
                CanCast = false;
            }
        }
    }


    private LivingEntity.TerminalVel currentTerminal;

    protected override void Awake()
    {
        base.Awake();
        CanCast = false;
    }

    protected override void Start()
    {
        base.Start();
    }

    public override void AbilityUpdate()
    {
        base.AbilityUpdate();
        if (AbilityActivated)
        {
            PWallJumpSensor();
        }
        else
        {
            if(currentTerminal != null)
            {
                if (currentTerminal != null)
                {
                    if (aEntity.terminalVels.Contains(currentTerminal))
                    {
                        aEntity.terminalVels.Remove(currentTerminal);
                    }
                    currentTerminal = null;
                }
            }
        }
    }


    private void PWallJumpSensor()
    {
        Vector2 wallJumpRayDir = Vector2.right;

        if (!aEntity.isLookingRight)
        {
            wallJumpRayDir = Vector2.left;
        }
        if (!aEntity.IsGrounded 
           && (Physics2D.Raycast(new Vector3(sensorPosition.position.x, sensorPosition.position.y + sensorGap, sensorPosition.position.z), wallJumpRayDir, wallJumpRayLength, wallJumpMask)
           || Physics2D.Raycast(new Vector3(sensorPosition.position.x, sensorPosition.position.y - sensorGap, sensorPosition.position.z), wallJumpRayDir, wallJumpRayLength, wallJumpMask))           )
        {
            if (!CanCast)
            {
                onWallCling?.Invoke();
            }

            CanCast = true;

            if(currentTerminal == null)
            {
                if (aEntity.terminalVels.Contains(currentTerminal))
                {
                    aEntity.terminalVels.Remove(currentTerminal);
                }
                LivingEntity.TerminalVel term = new LivingEntity.TerminalVel(wallFallSpeed);
                currentTerminal = term;
                aEntity.terminalVels.Add(term);
            }
        }
        else
        {
            CanCast = false;

            if(currentTerminal != null)
            {
                if (aEntity.terminalVels.Contains(currentTerminal))
                {
                    aEntity.terminalVels.Remove(currentTerminal);
                }
                currentTerminal = null;
            }
        }
        Debug.DrawRay(new Vector3(sensorPosition.position.x, sensorPosition.position.y + sensorGap, sensorPosition.position.z), wallJumpRayDir * wallJumpRayLength, Color.cyan);
        Debug.DrawRay(new Vector3(sensorPosition.position.x, sensorPosition.position.y - sensorGap, sensorPosition.position.z), wallJumpRayDir * wallJumpRayLength, Color.cyan);
    }


    protected override void CastAbilityImpl()
    {
        PWallJump();
    }

    public override void Cancel()
    {
        base.Cancel();

        if (currentTerminal != null)
        {
            if (aEntity.terminalVels.Contains(currentTerminal))
            {
                aEntity.terminalVels.Remove(currentTerminal);
            }
            currentTerminal = null;
        }
    }

    private void PWallJump()
    {
        float initialDur = 0.05f;
        float subDur = 0.3f;

        //[isJumping = false;]

        if (aEntity.isLookingRight)
        {
            aEntity.knockbackControl.StartKnockback(new Vector2(-wallJumpForce.x, wallJumpForce.y), initialDur, subDur);
        }
        else
        {
            aEntity.knockbackControl.StartKnockback(wallJumpForce, initialDur, subDur);
        }

        //Flip Sets
        aEntity.FlipControlLockCounter = 0.25f;
        aEntity.EntityFlip();

        //Other Ability Interactions


        //[ResetMobility();]
    }
}
