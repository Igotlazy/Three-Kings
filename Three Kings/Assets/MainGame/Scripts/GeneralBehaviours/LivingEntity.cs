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

    [HideInInspector] public HealthControl healthControl;
    [HideInInspector] public KnockbackControl knockbackControl;

    [Header("Physics:")]
    public float originalTerminalVelocity = -20f;
    [SerializeField] private float currentTerminalVelocity;
    public List<TerminalVel> terminalVels = new List<TerminalVel>();
    

    public float groundRayLength = 0.12f;
    protected LayerMask groundMask;

    public enum ControlType
    {
        CanControl,
        CannotControl,
        OtherControl
    }
    [Tooltip("0 = Can Control, 1 = Cannot Control, 2 = Other Movement")]
    public ControlType currControlType;

    [Header("State Bools:")]
    public bool isLookingRight;
    [SerializeField] protected bool isGrounded;
    public bool IsGrounded
    {get {return isGrounded;}}


    [Header("References:")]
    [HideInInspector] public BoxCollider2D entityBC2D;
    [HideInInspector] public Rigidbody2D entityRB2D;




    protected virtual void Awake()
    {
        groundMask = (1 << LayerMask.NameToLayer("Ground - Soft")) | (1 << LayerMask.NameToLayer("Ground - Hard"));

        entityRB2D = this.GetComponent<Rigidbody2D>();
        entityBC2D = this.GetComponent<BoxCollider2D>();

        healthControl = GetComponent<HealthControl>();
        healthControl.onHitAlive += HitReactionOnAlive;
        healthControl.onHitDeath += PhysicsCleanUpOnDeath;

        knockbackControl = GetComponent<KnockbackControl>();
        waitForStopped = new WaitUntil(() => !isStopped); 

        currentTerminalVelocity = originalTerminalVelocity;
    }


    protected virtual void Start()
    {
        EntityInitialLookingCheck();
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            IsStopped = !IsStopped;
        }
        //Flipping 
        EntityFlipControl();
    }


    protected virtual void FixedUpdate()
    {
        //Controlling Entity
        TerminalVelocityUpdate();
        if (currControlType == ControlType.CanControl || currControlType == ControlType.CannotControl)
        {
            EntityMove();
        }
    }


    //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    // [FUNCTIONS] //

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
      
        
        //totalXVelocity = (baseInputSpeed.Value * knockbackControl.baseInputMultiplier) + (knockbackControl.knockbackIntensity * knockbackControl.knockbackMultiplier) + outsideSourceSpeed.Value;
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
                if ((target > 0 && smoothingValue < 0) || (target < 1 && smoothingValue > 0))
                {
                    smoothingValue = 0;
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


    public void EntityFlip()
    {
        if (!lockFlip)
        {
            Debug.Log("Flip");
            isLookingRight = !isLookingRight;
            Vector2 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
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
            EntityControlTypeSet(ControlType.CannotControl, true, 0.35f);
            Debug.Log("Set to CannotControl");

            Vector2 knock = attack.knockback;
            if (attack.damageSource.transform.position.x > gameObject.transform.position.x)
            {
                knock.x *= -1;
            }

            knockbackControl.StartKnockback(knock, 0.2f, 0.5f);
        }
    }


    protected virtual void PhysicsCleanUpOnDeath(Attack attack)
    {
        knockbackControl.StopKnockback();
    }


    public void EntityControlTypeSet(ControlType givenType, bool instaStop)
    {
        if(returnToControlType != null)
        {
            StopCoroutine(returnToControlType);
        }

        if (instaStop)
        {
            ControlTypeInstaStopAux(givenType);
        }

        currControlType = givenType;

        Debug.Log("Set To: " + givenType.ToString());
    }
    public void EntityControlTypeSet(ControlType givenType, bool instaTop, float returnToCanControlTime)
    {
        EntityControlTypeSet(givenType, instaTop);

        returnToControlType = StartCoroutine(ReturnToControlType(returnToCanControlTime));
    }
    Coroutine returnToControlType;


    protected virtual void ControlTypeInstaStopAux(ControlType newType)
    {
        this.entityRB2D.velocity = Vector2.zero;
        knockbackControl.StopKnockback();
        smoothingValue = 0;
    }


    private IEnumerator ReturnToControlType(float returnTime)
    {
        float starTime = 0;

        while(starTime <= returnTime)
        {
            starTime += Time.deltaTime;
            yield return null;
        }

        currControlType = ControlType.CanControl;
    }


    protected IEnumerator WaitForSecondsHitStop(float duration, bool isFixed, bool isRealTime)
    {
        float time = 0;
        while(time < duration)
        {
            if (isStopped)
            {
                yield return new WaitUntil(() => !isStopped);
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
    bool isStopped;
    bool IsStopped
    {
        get
        {
            return isStopped;
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
            if(!value && isStopped)
            {
                entityRB2D.bodyType = oldType;
                entityRB2D.gravityScale = 1;
                entityRB2D.velocity = oldvel;
            }

            isStopped = value;
        }
    }
    protected WaitForFixedUpdate waitForFix = new WaitForFixedUpdate();
    protected WaitUntil waitForStopped;

    public void HitStop()
    {
        hitStopTimer = 0;
        IsStopped = true;
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
