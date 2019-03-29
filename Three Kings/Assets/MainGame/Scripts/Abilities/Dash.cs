using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Dash : Ability
{
    [Header("Dash:")]
    public float dashSpeed = 21f;
    public float dashTime = 0.25f;
    public AnimationCurve dashCurve;
    public float interTime = 0.2f;
    public bool isDashing;
    [Space]
    public float maxDashCharge = 1f;
    public float currentDashCharge;

    private float interCounter;

    [Header("References:")]
    public GameObject dashParticles;

    public Func<Vector2, Vector2> onDashVectorCalculated;

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
                currentDashCharge = maxDashCharge;
            }
            else
            {
                currentDashCharge = 0;
            }
        }
    }


    private WaitForFixedUpdate waitForFix = new WaitForFixedUpdate();

    protected override void Awake()
    {
        base.Awake();
        currentDashCharge = maxDashCharge;
    }

    protected override void Start()
    {
        base.Start();
    }


    public override void AbilityUpdate()
    {
        base.AbilityUpdate();

        if(interCounter > 0)
        {
            interCounter -= Time.deltaTime;
        }
    }

    public override void AbilityFixedUpdate()
    {
        base.AbilityFixedUpdate();
        GroundSets();
    }

    protected override void CastAbilityImpl()
    {
        PDashInitInput();
    }

    public override void Cancel()
    {
        base.Cancel();

        if (isDashing)
        {
            PDashCancel();
        }
    }

    private void PDashInitInput()
    {
        if (currentDashCharge > 0 && !isDashing && interCounter <= 0)
        {
            Vector2 dashVec = Vector2.right;

            if (Input.GetAxisRaw("Vertical") > 0 || Input.GetAxisRaw("Horizontal") != 0)
            {
                dashVec = new Vector2(Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")), Mathf.RoundToInt(Mathf.Clamp(Input.GetAxisRaw("Vertical"), 0, 1))).normalized;
            }
            else
            {
                if (aEntity.isLookingRight)
                {
                    dashVec.x = 1;
                }
                else
                {
                    dashVec.x = -1;
                }
            }

            currentDashCharge -= 1;

            dashVec = dashVec.normalized;


            dashVec = onDashVectorCalculated?.Invoke(dashVec) ?? dashVec;

            DashInitiate(dashVec, dashSpeed, dashTime, true);
        }
    }

    public void DashInitiate(Vector2 dashVector, float speed, float time, bool doLock)
    {
        if (dashVector.x > 0 && !aEntity.isLookingRight)
        {
            aEntity.EntityFlip();
            aEntity.FlipControlLockCounter = 0;
        }
        if (dashVector.x < 0 && aEntity.isLookingRight)
        {
            aEntity.EntityFlip();
            aEntity.FlipControlLockCounter = 0;
        }

        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
        }

        dashCoroutine = StartCoroutine(DashMain(dashVector, speed, time, doLock));
    }

    Coroutine dashCoroutine;

    private IEnumerator DashMain(Vector2 dashVector, float dSpeed, float dTime, bool doLock)
    {
        if (!doLock)
        {
            interCounter = 0;
        }

        aEntity.lockFlip = true;

        aEntity.EntityControlTypeSet(LivingEntity.ControlType.OtherControl, true);
        aEntity.entityRB2D.gravityScale = 0;

        isDashing = true;

        GameObject particles = Instantiate(dashParticles, gameObject.transform.position, Quaternion.identity);
        if (aEntity.isLookingRight)
        {
            particles.transform.localScale = new Vector3(-particles.transform.localScale.x, particles.transform.localScale.y, particles.transform.localScale.z);
        }


        float currentTime = 0;
        while (currentTime <= dTime)
        {
            particles.transform.position = transform.position;
            aEntity.entityRB2D.velocity = dashVector * dashCurve.Evaluate(currentTime / dTime) * dSpeed;
            currentTime += Time.fixedDeltaTime;
            yield return waitForFix;
        }

        aEntity.entityRB2D.gravityScale = 1;
        aEntity.EntityControlTypeSet(LivingEntity.ControlType.CanControl, false);

        if (doLock)
        {
            interCounter = interTime;
        }

        aEntity.lockFlip = false;
        isDashing = false;
        if (dashVector.x > 0 && dashVector.y < 0.75f)
        {
            aEntity.smoothingValue = 1;
        }
        if (dashVector.x < 0 && dashVector.y < 0.75f)
        {
            aEntity.smoothingValue = -1;
        }
    }


    private void PDashCancel()
    {
        if (dashCoroutine != null)
        {
            StopCoroutine(dashCoroutine);
        }

        aEntity.entityRB2D.gravityScale = 1;
        aEntity.entityRB2D.velocity = Vector2.zero;

        aEntity.EntityControlTypeSet(LivingEntity.ControlType.CanControl, false);

        interCounter = 0;
        aEntity.lockFlip = false;

        isDashing = false;
        GroundSets();
    }

    private void GroundSets()
    {
        if(!isDashing && aEntity.IsGrounded)
        {
            currentDashCharge = maxDashCharge;
        }
    }

    public void ApplyLock()
    {
        interCounter = interTime;
    }
}
