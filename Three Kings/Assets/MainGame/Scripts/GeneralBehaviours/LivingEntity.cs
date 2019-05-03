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

    public AdvancedVector2 InputVector = new AdvancedVector2();
    public AdvancedVector2 ForcesVector = new AdvancedVector2();
    public AdvancedVector2 OutsideVelVector = new AdvancedVector2();

    public FloatModifier gravity;
    public bool hasGravity;


    [Header("Input:")]
    public float inputSensitivity = 6f;
    public float inputGravity = 10f;
    public float inputDead = 0.2f;

    public float smoothingValue;

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
    [SerializeField] private Vector2 velocity = Vector2.zero;
    public virtual Vector2 EntityVelocity
    {
        get
        {
            return velocity;
        }
        set
        {
            EntityRB2D.velocity = value;
            velocity = value;
        }
    }
    public float gravityMultiplier = 1f;
    public float originalTerminalVelocity = -20f;
    [SerializeField] private float currentTerminalVelocity;
    public List<TerminalVel> terminalVels = new List<TerminalVel>();

    public float groundRayLength = 0.05f;
    protected LayerMask groundMask;

    bool collidedWithGround;
    ContactPoint2D[] contacts = new ContactPoint2D[16];
    public struct Collision2DStates
    {
        public bool hitTop, hitBot, hitLeft, hitRight;

        public void SetFalse()
        {
            hitTop = hitBot = hitLeft = hitRight = false;
        }
    }
    Collision2DStates colStates = new Collision2DStates();


    [Header("State Bools:")]
    public bool isLookingRight;
    [SerializeField] private bool isGrounded;
    public bool IsGrounded
    {get {return isGrounded;}}



    public BoxCollider2D EntityBC2D { get; private set; }
    public Rigidbody2D EntityRB2D { get; private set; }
    [HideInInspector] public HealthControl healthControl;
    [HideInInspector] public KnockbackControl knockbackControl;

    public Action CurrControlMethod { get; private set; }
    public Action CurrActionUpdateMethod { get; private set; }
    public Action CurrActionFixedMethod { get; private set; }

    public StateSetter currentController;
    private StateSetter originalState;

    

    protected virtual void Awake()
    {
        EntityRB2D = this.GetComponent<Rigidbody2D>();
        EntityBC2D = this.GetComponent<BoxCollider2D>();

        groundMask = (1 << LayerMask.NameToLayer("Ground - Soft")) | (1 << LayerMask.NameToLayer("Ground - Hard"));

        healthControl = GetComponent<HealthControl>();
        healthControl.onHitAlive += HitReactionOnAlive;
        healthControl.onHitDeath += PhysicsCleanUpOnDeath;

        knockbackControl = GetComponent<KnockbackControl>();
        //waitForNotPaused = new WaitUntil(() => !isPaused);

        gravity = new FloatModifier(0, FloatModifier.FloatModType.Flat) { ignoreRemove = true };
        ForcesVector.Y.AddSingleModifier(gravity);

        currentTerminalVelocity = originalTerminalVelocity;

        originalState = new StateSetter(this, null, BaseActionControl, BaseActionUpdate, BaseActionFixedUpdate);
        SetLivingEntityState(originalState, true);
    }


    protected virtual void Start()
    {
        EntityInitialLookingCheck();        
    }

    protected virtual void Update()
    {
        CurrControlMethod?.Invoke();
        CurrActionUpdateMethod?.Invoke();
    }


    protected virtual void FixedUpdate()
    {
        CurrActionFixedMethod?.Invoke();
    }

    public bool SetLivingEntityState(StateSetter setter, bool ignoreStrength)
    {
        if (currentController == null || ignoreStrength || currentController.setStrength <= setter.setStrength)
        {
            if (currentController != null)
            {
                currentController.CancelMethod?.Invoke();
            }

            setter.SetUpMethod?.Invoke();

            CurrControlMethod = setter.ControlMethod;
            CurrActionUpdateMethod = setter.UpdateMethod;
            CurrActionFixedMethod = setter.FixedUpdateMethod;

            currentController = setter;

            return true;
        }

        return false;
    }


    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // [FUNCTIONS] //

    public virtual void BaseActionUpdate()
    {
        EntityFlipControl();
    }
    public virtual void BaseActionFixedUpdate()
    {
        EntityVelocity = new Vector2(CalculateXMovement(), CalculateYMovement());
    }

    public virtual void BaseActionControl() { }

    public virtual void NoControl() { }

    public void EntityMove()
    {
        /*
        totalXVelocity = ((baseInputSpeed.Value + outsideSourceSpeed.Value) * knockbackControl.inputReducer) + knockbackControl.knockbackX.Value;
        EntityRB2D.velocity = new Vector2(totalXVelocity, EntityRB2D.velocity.y);

        //Vertical Movement
        float yVel = Mathf.Clamp(EntityRB2D.velocity.y, currentTerminalVelocity, 100.0f);
        EntityRB2D.velocity = new Vector2(EntityRB2D.velocity.x, yVel); 
        */
    }

    float CalculateXMovement()
    {
        float fix = Time.fixedDeltaTime;
        return InputVector.XValueTimeClamp(fix)
            + ForcesVector.XValueTimeClamp(fix)
            + OutsideVelVector.XValueTimeClamp(fix);
    }

    float CalculateYMovement()
    {
        if (hasGravity)
        {
            GroundCheck();
            TerminalVelocityUpdate();

            if (!IsGrounded && collidedWithGround)
            {
                collidedWithGround = false;
            }

            if (!IsGrounded && !collidedWithGround)
            {
                gravity.ModifierValue += Physics2D.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
                gravity.ModifierValue = Mathf.Clamp(gravity.ModifierValue, currentTerminalVelocity, 1000);
            }
        }
        else
        {
            gravity.ModifierValue = Mathf.MoveTowards(gravity.ModifierValue, 0, Mathf.Abs(Physics2D.gravity.y) * Time.fixedDeltaTime);
        }

        float fix = Time.fixedDeltaTime;
        return InputVector.YValueTimeClamp(fix)
            + ForcesVector.YValueTimeClamp(fix)
            + OutsideVelVector.YValueTimeClamp(fix);
    }

    private void TerminalVelocityUpdate()
    {
        float trueVel = originalTerminalVelocity;
        float fix = Time.fixedDeltaTime;

        for(int i = terminalVels.Count -1 ; i >= 0; i--)
        {
            if(terminalVels[i].vel > trueVel)
            {
                trueVel = terminalVels[i].vel;
            }
            if (terminalVels[i].isTimed)
            {
                terminalVels[i].duration -= fix;
                if(terminalVels[i].duration <= 0)
                {
                    terminalVels.Remove(terminalVels[i]);
                }
            }
        }

        currentTerminalVelocity = trueVel;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        int collisionNum = collision.GetContacts(contacts);
        collisionNum = Mathf.Clamp(collisionNum, 0, contacts.Length);
        colStates.SetFalse();

        for(int i = 0; i < collisionNum; i++)
        {
            //Debug.Log(contacts[i].collider.name + ": "  + contacts[i].normal + " " + contacts[i].point);
            if (!colStates.hitBot && contacts[i].normal == Vector2.up)
            {
                colStates.hitBot = true;

                if(EntityVelocity.y < 0)
                {
                    ForcesVector.Y.RemoveAllModifiers();
                    collidedWithGround = true;
                }
                if(gravity.ModifierValue < 0)
                {
                    gravity.ModifierValue = 0;
                    collidedWithGround = true;
                }
            }
            else if (!colStates.hitTop && contacts[i].normal == Vector2.down && EntityVelocity.y > 0)
            {
                colStates.hitTop = true;

                ForcesVector.Y.RemoveAllModifiers();
                gravity.ModifierValue = 0;
            }
            else if (!colStates.hitLeft && contacts[i].normal == Vector2.right && EntityVelocity.x < 0)
            {
                colStates.hitLeft = true;

                ForcesVector.X.RemoveAllModifiers();
            }
            else if (!colStates.hitRight && contacts[i].normal == Vector2.left && EntityVelocity.x > 0)
            {
                colStates.hitRight = true;

                ForcesVector.X.RemoveAllModifiers();
            }
        }
    }


    protected float InputSmoothing(string axisName)
    {
        if (!knockbackControl.isHardKnocking)
        {
            float target = Input.GetAxisRaw(axisName);
            if (target != 0)
            {
                if (target > 0)
                {
                    target = 1;

                    if (smoothingValue < 0)
                    {
                        smoothingValue = 0;
                    }
                }
                if (target < 0)
                {
                    target = -1;

                    if (smoothingValue > 0)
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
        InputVector.X.BaseValue = 0;
        InputVector.Y.BaseValue = 0;

        ForcesVector.RemoveAllModifiers();

        smoothingValue = 0;

        EntityVelocity = Vector2.zero;
        gravity.ModifierValue = 0;

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
            if (InputVector.X.Value < 0.0f && isLookingRight)
            {
                EntityFlip();
            }
            else if (InputVector.X.Value > 0.0f && !isLookingRight)
            {
                EntityFlip();
            }
        }
    }


    public bool EntityFlip()
    {
        if (!lockFlip)
        {
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
        Vector2 leftRayOrigin = new Vector2(EntityBC2D.bounds.center.x - EntityBC2D.bounds.extents.x, EntityBC2D.bounds.center.y - EntityBC2D.bounds.extents.y);
        Vector2 rightRayOrigin = new Vector2(EntityBC2D.bounds.center.x + EntityBC2D.bounds.extents.x, EntityBC2D.bounds.center.y - EntityBC2D.bounds.extents.y);
        Vector2 centerRayOrigin = new Vector2(EntityBC2D.bounds.center.x, EntityBC2D.bounds.center.y - EntityBC2D.bounds.extents.y);

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
            SetLivingEntityState(new StateSetter(this, null, HitKnockControl, BaseActionUpdate, BaseActionFixedUpdate), false);

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



    /*
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
                oldType = EntityRB2D.bodyType;
                EntityRB2D.bodyType = RigidbodyType2D.Kinematic;

                EntityRB2D.gravityScale = 0;

                oldvel = EntityRB2D.velocity;
                EntityRB2D.velocity = Vector2.zero;
            }
            if(!value && isPaused)
            {
                EntityRB2D.bodyType = oldType;
                EntityRB2D.gravityScale = 1;
                EntityRB2D.velocity = oldvel;
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
    */



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
