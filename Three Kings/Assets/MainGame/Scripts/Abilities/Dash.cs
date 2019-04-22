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
    Vector2 currentVector;

    private StateSetter dashState;

    public Action DashControl;

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
        dashState = new StateSetter(this, DashControl, null, DashMain, CancelState, StateSetter.SetStrength.Weak);
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
        currentVector = dashVector;

        GameObject particles = Instantiate(dashParticles, gameObject.transform.position, Quaternion.identity);
        particles.transform.SetParent(gameObject.transform);
        if (aEntity.isLookingRight)
        {
            particles.transform.localScale = new Vector3(-particles.transform.localScale.x, particles.transform.localScale.y, particles.transform.localScale.z);
        }


        if (!doLock)
        {
            interCounter = 0;
        }

        aEntity.lockFlip = true;
        isDashing = true;

        currentTime = 0;

        aEntity.InputAndPhysicsCleanUp();
        aEntity.SetLivingEntityState(dashState, false);
        dashStartPos = aEntity.transform.position;
        setStartPos = false;
    }
    float currentTime;
    float xPos;
    float yPos;
    Vector3 dashStartPos;
    bool setStartPos;

    private void DashMain()
    {
        if (!setStartPos)
        {
            xPos = aEntity.transform.position.x;
            yPos = aEntity.transform.position.y;
            aEntity.transform.position = dashStartPos;
            setStartPos = true;
        }

        Vector2 dashVel = currentVector * dashCurve.Evaluate(currentTime / dashTime) * dashSpeed /** FloatModifier.TimeClamp(currentTime, dashTime, Time.fixedDeltaTime)*/;
        aEntity.EntityVelocity = dashVel;

        GroundSets();

        if (currentTime >= dashTime)
        {
            //Vector3 displacement = currentVector * dashSpeed * dashTime;
            //aEntity.transform.position = dashStartPos + displacement;
            Debug.Log(Mathf.Abs(aEntity.transform.position.x - xPos));
            Debug.Log(Mathf.Abs(aEntity.transform.position.y - yPos));
            interCounter = interTime;

            aEntity.lockFlip = false;
            isDashing = false;


            if (currentVector.x > 0 && currentVector.y < 0.75f)
            {
                aEntity.smoothingValue = 1;
            }
            if (currentVector.x < 0 && currentVector.y < 0.75f)
            {
                aEntity.smoothingValue = -1;
            }

            aEntity.gravity.ModifierValue = dashVel.y;
            aEntity.OriginalStateSet();           
        }
        else
        {
            currentTime += Time.fixedDeltaTime;
        }
    }


    private void PDashCancel()
    {
        aEntity.EntityVelocity = Vector2.zero;

        interCounter = 0;
        aEntity.lockFlip = false;

        isDashing = false;
        GroundSets();
    }

    private void GroundSets()
    {
        if(aEntity.IsGrounded)
        {
            currentDashCharge = maxDashCharge;
        }
    }

    public void ApplyLock()
    {
        interCounter = interTime;
    }


    public void CancelState()
    {
        Debug.Log("Dash Cancelled");
        aEntity.EntityRB2D.gravityScale = 1;
        interCounter = 0;
        aEntity.lockFlip = false;
        isDashing = false;
        setStartPos = false;
    }
}
