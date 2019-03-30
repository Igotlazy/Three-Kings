using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthControl))]
[RequireComponent(typeof(KnockbackControl))]
public class LivingEntity : MonoBehaviour
{
    [Header("ENTITY")]
    [Header("Movement:")]
    public AdvancedFloat baseInputSpeed = new AdvancedFloat();
    public AdvancedFloat knockbackSpeed = new AdvancedFloat();
    public AdvancedFloat outsideSourceSpeed = new AdvancedFloat();
    [SerializeField] private float totalXVelocity;

    [Header("Input:")]
    public float inputSensitivity = 3f;
    public float inputGravity = 3f;
    public float inputDead = 0.001f;

    public float smoothingValue;
    private bool lockSmoothing;
    public bool LockSmoothing
    {
        get
        {
            return lockSmoothing;
        }
        set
        {
            lockSmoothing = value;
            if(value == true)
            {
                smoothingValue = 0;
            }
        }
    }
    private float flipControlLockCounter;
    public float FlipControlLockCounter
    {
        get
        {
            return flipControlLockCounter;
        }
        set
        {
            if(value > flipControlLockCounter)
            {
                flipControlLockCounter = value;
            }
        }

    }
    public bool lockFlip;

    [Header("Physics:")]
    public float originalTerminalVelocity = -20f;
    [SerializeField] private float currentTerminalVelocity;
    public List<TerminalVel> terminalVels = new List<TerminalVel>();
    public float groundRayLength = 0.12f;
    protected LayerMask groundMask;

    [Header("State Bools:")]
    public bool isLookingRight;
    [SerializeField] protected bool isGrounded;
    public bool IsGrounded
    {get {return isGrounded;}}


    [Header("References:")]
    [HideInInspector] public BoxCollider2D entityBC2D;
    [HideInInspector] public Rigidbody2D entityRB2D;
    [HideInInspector] public HealthControl healthControl;
    [HideInInspector] public KnockbackControl knockbackControl;

    public Action SetControlMethod { get; private set; }
    public Action SetActionUpdateMethod { get; private set; }
    public Action SetActionFixedUpMethod { get; private set; }

    public StateSetter currentController;
    private StateSetter originalState;

    public bool SetLivingEntityState(StateSetter setter, bool ignoreStrength)
    {
        if(currentController == null || ignoreStrength || currentController.setStrength <= setter.setStrength)
        {
            if(currentController != null)
            {
                currentController.CancelState();
            }

            if (setter.nullsOverride)
            {
                SetControlMethod = setter.ControlMethod;
                SetActionUpdateMethod = setter.UpdateMethod;
                SetActionFixedUpMethod = setter.FixedUpdateMethod;
            }
            else
            {
                if(setter.ControlMethod != null)
                {
                    SetControlMethod = setter.ControlMethod;
                }
                if(setter.UpdateMethod != null)
                {
                    SetActionUpdateMethod = setter.UpdateMethod;
                }
                if(setter.FixedUpdateMethod != null)
                {
                    SetActionFixedUpMethod = setter.FixedUpdateMethod;
                }
            }

            currentController = setter;

            return true;
        }
        else
        {
            setter.CancelState();
        }

        return false;
    }

    protected virtual void Awake()
    {
        groundMask = (1 << LayerMask.NameToLayer("Ground - Soft")) | (1 << LayerMask.NameToLayer("Ground - Hard"));

        entityRB2D = this.GetComponent<Rigidbody2D>();
        entityBC2D = this.GetComponent<BoxCollider2D>();

        healthControl = GetComponent<HealthControl>();
        healthControl.onHitAlive += HitReactionOnAlive;
        healthControl.onHitDeath += PhysicsCleanUpOnDeath;

        knockbackControl = GetComponent<KnockbackControl>();
        waitForNotPaused = new WaitUntil(() => !isPaused);

        

        currentTerminalVelocity = originalTerminalVelocity;

        originalState = new StateSetter(this, BaseActionControl, BaseActionUpdate, BaseActionFixedUpdate);
        SetLivingEntityState(originalState, true);
    }


    protected virtual void Start()
    {
        EntityInitialLookingCheck();

        SetControlMethod = BaseActionControl;
        SetActionUpdateMethod = BaseActionUpdate;
        SetActionFixedUpMethod = BaseActionFixedUpdate;
        
    }

    protected virtual void Update()
    {
        /*
        {
            if (currControlType != ControlType.OtherControl)
            {
                //Flipping 
                EntityFlipControl();
            }
        }
        */

        SetControlMethod?.Invoke();
        SetActionUpdateMethod?.Invoke();
    }


    protected virtual void FixedUpdate()
    {
        //Controlling Entity
        /*
        {
            TerminalVelocityUpdate();
            if (currControlType == ControlType.CanControl || currControlType == ControlType.CannotControl)
            {
                EntityMove();
            }
        }
        */

        SetActionFixedUpMethod?.Invoke();
    }


    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // [FUNCTIONS] //

    public virtual void BaseActionUpdate()
    {
        EntityFlipControl();
    }
    public virtual void BaseActionFixedUpdate()
    {
        TerminalVelocityUpdate();
        EntityMove();
    }

    public virtual void BaseActionControl() { }

    public virtual void NoControl() { }

    public void EntityMove()
    {
        //Horizontal Movement
        if (knockbackControl.isHardKnocking)
        {
            LockSmoothing = true;
        }
        else
        {
            LockSmoothing = false;
        }
             
        
        totalXVelocity = ((baseInputSpeed.Value + outsideSourceSpeed.Value) * knockbackControl.inputReducer) + knockbackControl.knockbackX.Value;
        entityRB2D.velocity = new Vector2(totalXVelocity, entityRB2D.velocity.y);

        //Vertical Movement
        float yVel = Mathf.Clamp(entityRB2D.velocity.y, currentTerminalVelocity, 100.0f);
        entityRB2D.velocity = new Vector2(entityRB2D.velocity.x, yVel);
    }
    private void TerminalVelocityUpdate()
    {
        float trueVel = originalTerminalVelocity;

        for(int i = terminalVels.Count -1 ; i >= 0; i--)
        {
            if(terminalVels[i].vel > trueVel)
            {
                trueVel = terminalVels[i].vel;
            }
            if (terminalVels[i].isTimed)
            {
                terminalVels[i].duration -= Time.fixedDeltaTime;
                if(terminalVels[i].duration <= 0)
                {
                    terminalVels.Remove(terminalVels[i]);
                }
            }
        }

        currentTerminalVelocity = trueVel;
    }


    protected float InputSmoothing(string axisName)
    {
        if (!lockSmoothing)
        {
            float target = Input.GetAxisRaw(axisName);
            if (target != 0)
            {
                if (target > 0)
                {
                    target = 1;

                    if(smoothingValue < 0)
                    {
                        smoothingValue = 0;
                    }
                }
                if(target < 0)
                {
                    target = -1;

                    if(smoothingValue > 0)
                    {
                        smoothingValue = 0;
                    }
                }
                smoothingValue = Mathf.MoveTowards(smoothingValue, target, inputSensitivity * Time.deltaTime);
            }
            else
            {
                smoothingValue = Mathf.MoveTowards(smoothingValue, 0f, inputGravity * Time.deltaTime);
            }
            if (Mathf.Abs(smoothingValue) < inputDead)
            {
                return 0f;
            }
            else
            {
                return smoothingValue;
            }
        }
        else
        {
            return 0;
        }
    }

    public virtual void InputAndPhysicsCleanUp()
    {
        baseInputSpeed.BaseValue = 0;
        smoothingValue = 0;
        entityRB2D.velocity = Vector2.zero;
        knockbackControl.StopKnockback();
    }


    public void EntityInitialLookingCheck()
    {
        float localScale = this.transform.localScale.x;
        if (localScale >= 0)
        {
            isLookingRight = true;
        }
        else
        {
            isLookingRight = false;
        }
    }


    public void EntityFlipControl()
    {
        if (flipControlLockCounter > 0)
        {
            flipControlLockCounter -= Time.deltaTime;
        }
        else
        {
            if (baseInputSpeed.Value < 0.0f && isLookingRight == true)
            {
                EntityFlip();
            }
            else if (baseInputSpeed.Value > 0.0f && isLookingRight == false)
            {
                EntityFlip();
            }
        }
    }


    public bool EntityFlip()
    {
        if (!lockFlip)
        {
            Debug.Log("Flip");

            isLookingRight = !isLookingRight;
            Vector2 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;

            return true;
        }
        return false;
    }


    protected void GroundCheck()
    {
        Vector2 leftRayOrigin = new Vector2(entityBC2D.bounds.center.x - entityBC2D.bounds.extents.x, entityBC2D.bounds.center.y - entityBC2D.bounds.extents.y);
        Vector2 rightRayOrigin = new Vector2(entityBC2D.bounds.center.x + entityBC2D.bounds.extents.x, entityBC2D.bounds.center.y - entityBC2D.bounds.extents.y);
        Vector2 centerRayOrigin = new Vector2(entityBC2D.bounds.center.x, entityBC2D.bounds.center.y - entityBC2D.bounds.extents.y);

        //Left Raycast
        bool leftHit = Physics2D.Raycast(leftRayOrigin, Vector2.down, groundRayLength, groundMask);
        Debug.DrawRay(leftRayOrigin, Vector2.down * groundRayLength, Color.red);

        //Right Raycast
        bool rightHit = Physics2D.Raycast(rightRayOrigin, Vector2.down, groundRayLength, groundMask);
        Debug.DrawRay(rightRayOrigin, Vector2.down * groundRayLength, Color.red);

        //Center Raycast
        bool centerHit = Physics2D.Raycast(centerRayOrigin, Vector2.down, groundRayLength, groundMask);
        Debug.DrawRay(centerRayOrigin, Vector2.down * groundRayLength, Color.red);

        if (leftHit || rightHit || centerHit)
        {
            isGrounded =  true;
        }
        else
        {
            isGrounded =  false;
        }
        
    }


    protected virtual void HitReactionOnAlive(Attack attack)
    {
        if (attack.damageSource.CompareTag("Hazard"))
        {
            knockbackControl.StopKnockback();
        }
        else if (attack.doesKnockback)
        {
            hitTimeCounter = 0;
            SetLivingEntityState(new StateSetter(this, HitKnockControl, BaseActionUpdate, BaseActionFixedUpdate), false);

            Vector2 knock = attack.knockback;
            if (attack.damageSource.transform.position.x > gameObject.transform.position.x)
            {
                knock.x *= -1;
            }

            knockbackControl.StartKnockback(knock, 0.2f, 0.5f);
        }
    }

    private void HitKnockControl()
    {
        if(hitTimeCounter > 0.35f)
        {
            OriginalStateSet();
        }
        hitTimeCounter += Time.deltaTime;
    }
    float hitTimeCounter;


    protected virtual void PhysicsCleanUpOnDeath(Attack attack)
    {
        InputAndPhysicsCleanUp();
    }

    public bool OriginalStateSet()
    {
        if (SetLivingEntityState(originalState, true))
        {
            return true;
        }
        return false;
    }

    protected IEnumerator WaitForSecondsHitStop(float duration, bool isFixed, bool isRealTime)
    {
        float time = 0;
        while(time < duration)
        {
            if (isPaused)
            {
                yield return waitForNotPaused;
            }

            if (!isFixed)
            {
                time += isRealTime ? Time.unscaledDeltaTime : Time.deltaTime;

                yield return null;
            }
            else
            {
                time += isRealTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime;

                yield return waitForFix;
            }
        }
    }
    bool isPaused;
    protected virtual bool IsPaused
    {
        get
        {
            return isPaused;
        }
        set
        {
            if (value)
            {
                oldType = entityRB2D.bodyType;
                entityRB2D.bodyType = RigidbodyType2D.Kinematic;

                entityRB2D.gravityScale = 0;

                oldvel = entityRB2D.velocity;
                entityRB2D.velocity = Vector2.zero;
            }
            if(!value && isPaused)
            {
                entityRB2D.bodyType = oldType;
                entityRB2D.gravityScale = 1;
                entityRB2D.velocity = oldvel;
            }

            isPaused = value;
        }
    }
    protected WaitForFixedUpdate waitForFix = new WaitForFixedUpdate();
    protected WaitUntil waitForNotPaused;

    public void HitStop()
    {
        hitStopTimer = 0;
        IsPaused = true;
    }

    float hitStopDuration = 2f;
    float hitStopTimer = 5;
    Vector2 oldvel = Vector2.zero;
    RigidbodyType2D oldType;



    public class TerminalVel
    {
        public float vel;
        public bool isTimed;
        public float duration;

        public TerminalVel(float _vel)
        {
            vel = _vel;
            isTimed = false;
        }

        public TerminalVel(float _vel, float _duration)
        {
            vel = _vel;
            isTimed = true;
            duration = _duration;
        }
    }

}
