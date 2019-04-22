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
    public float lockOutTimer;

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
                CanActivate = false;
            }
        }
    }


    private LivingEntity.TerminalVel currentTerminal;

    protected override void Awake()
    {
        base.Awake();
        CanActivate = false;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (lockOutTimer > 0)
        {
            lockOutTimer -= Time.deltaTime;
            CanActivate = false;
        }
    }

    public override void AbilityUpdate()
    {
        base.AbilityUpdate();
        if (AbilityActivated)
        {
            if(lockOutTimer <= 0)
            {
                PWallJumpSensor();
            }
        }
        else
        {
            if(currentTerminal != null)
            {
                if (currentTerminal != null)
                {
                    aEntity.terminalVels.Remove(currentTerminal);
                    currentTerminal = null;
                }
            }
        }
    }


    private void PWallJumpSensor()
    {
        float rayLength = aEntity.EntityBC2D.size.x / 2 + wallJumpRayLength;
        Vector2 wallJumpRayDir = Vector2.right;

        if (!aEntity.isLookingRight)
        {
            wallJumpRayDir = Vector2.left;
        }
        if (!aEntity.IsGrounded)
        {
            Vector3 topOrigin = new Vector3(sensorPosition.position.x, sensorPosition.position.y + sensorGap, sensorPosition.position.z);
            Vector3 botOrigin = new Vector3(sensorPosition.position.x, sensorPosition.position.y - sensorGap, sensorPosition.position.z);

            bool topRight = Physics2D.Raycast(topOrigin, Vector2.right, rayLength, wallJumpMask);
            bool botRight = Physics2D.Raycast(botOrigin, Vector2.right, rayLength, wallJumpMask);

            bool topLeft = Physics2D.Raycast(topOrigin, Vector2.left, rayLength, wallJumpMask);
            bool botLeft = Physics2D.Raycast(botOrigin, Vector2.left, rayLength, wallJumpMask);
            bool isOnWall = false;

            if (topRight || botRight || topLeft || botLeft)
            {
                if (aEntity.isLookingRight && (topRight || botRight))
                {
                    if (aEntity.EntityFlip())
                    {
                        aEntity.lockFlip = true;
                    }
                }
                if(!aEntity.isLookingRight && (topLeft || botLeft))
                {
                    if (aEntity.EntityFlip())
                    {
                        aEntity.lockFlip = true;
                    }
                }

                isOnWall = true;
            }

            if (isOnWall)
            {
                onWallCling?.Invoke();

                /*
                if (!CanCast)
                {
                    onWallCling?.Invoke();
                }
                */

                CanActivate = true;

                if (currentTerminal == null)
                {
                    aEntity.terminalVels.Remove(currentTerminal);
                    LivingEntity.TerminalVel term = new LivingEntity.TerminalVel(wallFallSpeed);
                    currentTerminal = term;
                    aEntity.terminalVels.Add(term);
                }
            }
            else
            {
                CanActivate = false;
                aEntity.lockFlip = false;

                if (currentTerminal != null)
                {
                    aEntity.terminalVels.Remove(currentTerminal);
                    currentTerminal = null;
                }
            }
        }
        else
        {
            CanActivate = false;
            aEntity.lockFlip = false;

            if (currentTerminal != null)
            {
                aEntity.terminalVels.Remove(currentTerminal);
                currentTerminal = null;
            }
        }
        Debug.DrawRay(new Vector3(sensorPosition.position.x, sensorPosition.position.y + sensorGap, sensorPosition.position.z), wallJumpRayDir * rayLength, Color.cyan);
        Debug.DrawRay(new Vector3(sensorPosition.position.x, sensorPosition.position.y - sensorGap, sensorPosition.position.z), wallJumpRayDir * rayLength, Color.cyan);
        Debug.DrawRay(new Vector3(sensorPosition.position.x, sensorPosition.position.y + sensorGap, sensorPosition.position.z), -wallJumpRayDir * rayLength, Color.cyan);
        Debug.DrawRay(new Vector3(sensorPosition.position.x, sensorPosition.position.y - sensorGap, sensorPosition.position.z), -wallJumpRayDir * rayLength, Color.cyan);
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
            aEntity.terminalVels.Remove(currentTerminal);
            currentTerminal = null;
            aEntity.lockFlip = false;
        }
    }

    private void PWallJump()
    {
        float initialDur = 0.035f;
        float subDur = 0.2f;

        //[isJumping = false;]

        if (!aEntity.isLookingRight)
        {
            aEntity.knockbackControl.StartKnockback(new Vector2(-wallJumpForce.x, wallJumpForce.y), initialDur, subDur);
        }
        else
        {
            aEntity.knockbackControl.StartKnockback(wallJumpForce, initialDur, subDur);
        }

        //Flip Sets
        aEntity.FlipControlLockCounter = 0.2f;
        //aEntity.EntityFlip();

        //Other Ability Interactions


        //[ResetMobility();]
    }
}
