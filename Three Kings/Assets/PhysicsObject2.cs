using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject2 : MonoBehaviour
{
    [Header("Movement Vectors:")]
    public AdvancedVector2 InputVector = new AdvancedVector2();
    public AdvancedVector2 ForcesVector = new AdvancedVector2();
    public AdvancedVector2 OutsideVelVector = new AdvancedVector2();

    public FloatModifier gravity;
    public FloatModifier xInput;

    [Header("Physics Properties:")]
    public bool hasGravity;
    public float terminalVelocity = -20f;

    public bool IsGrounded { get; private set; }
    public float groundRayLength = 0.05f;
    protected LayerMask groundMask;

    public Rigidbody2D ObjectRb2d { get; private set; }
    public Collider2D ObjectBc2d { get; private set; }

    [Header("Feedback:")]
    public Vector2 velocity = Vector2.zero;


    void Start()
    {
        ObjectRb2d = GetComponent<Rigidbody2D>();
        ObjectBc2d = GetComponent<Collider2D>();

        xInput = new FloatModifier(7, FloatModifier.FloatModType.Flat);
        gravity = new FloatModifier(0, FloatModifier.FloatModType.Flat);
        gravity.ignoreRemove = true;

        groundMask = (1 << LayerMask.NameToLayer("Ground - Soft")) | (1 << LayerMask.NameToLayer("Ground - Hard"));

        InputVector.X.AddSingleModifier(xInput);
        ForcesVector.Y.AddSingleModifier(gravity);
    }


    void Update()
    {
        if (Input.GetButtonDown("Jump"))
        {
            gravity.ModifierValue = 17.5f;
            isJumping = true;
            Debug.Log("Jump");
        }
        if(Input.GetButtonUp("Jump") && isJumping && gravity.ModifierValue > 0)
        {
            gravity.ModifierValue = 0;
            isJumping = false;
        }

        xInput.multiplier = Input.GetAxis("Horizontal");
    }
    bool isJumping;

    private void FixedUpdate()
    {
        velocity = new Vector2(CalculateXMovement(), CalculateYMovement());
        MoveObject(velocity);
    }

    private float TimeClamp(float current, float target, float step)
    {
        if(current > target)
        {
            return 0;
        }

        float normalAdd = current + step;
        if(normalAdd <= target)
        {
            return step;
        }

        float over = normalAdd - target;

        return step * (1 - (over / normalAdd));
    }

    private void MoveObject(Vector2 deltaMove)
    {
        //ObjectRb2d.MovePosition(ObjectRb2d.position + deltaMove);
        ObjectRb2d.velocity = deltaMove;
    }

    float CalculateXMovement()
    {
        return InputVector.XValue + ForcesVector.XValue + OutsideVelVector.XValue;
    }

    float CalculateYMovement()
    {
        if (hasGravity)
        {
            GroundCheck();

            if(!IsGrounded && hitDown)
            {
                hitDown = false;
            }
        
            if(!IsGrounded && !hitDown)
            {
                gravity.ModifierValue += Physics2D.gravity.y * Time.fixedDeltaTime;
                gravity.ModifierValue = Mathf.Clamp(gravity.ModifierValue, terminalVelocity, 1000);
            }
        }
        else
        {
            gravity.ModifierValue = Mathf.MoveTowards(gravity.ModifierValue, 0, Mathf.Abs(Physics2D.gravity.y) * Time.fixedDeltaTime);
        }

        return InputVector.YValue + ForcesVector.YValue + OutsideVelVector.YValue;
    }

    private void GroundCheck()
    {
        BoxCollider2D box = (BoxCollider2D)ObjectBc2d;

        Vector2 leftRayOrigin = new Vector2(box.bounds.center.x - box.bounds.extents.x, box.bounds.center.y - box.bounds.extents.y);
        Vector2 rightRayOrigin = new Vector2(box.bounds.center.x + box.bounds.extents.x, box.bounds.center.y - box.bounds.extents.y);
        Vector2 centerRayOrigin = new Vector2(box.bounds.center.x, box.bounds.center.y - box.bounds.extents.y);

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
            IsGrounded = true;
        }
        else
        {
            IsGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        if (contact.normal.Equals(Vector2.up))
        {
            Debug.Log("GravityReset");
            hitDown = true;

            ForcesVector.Y.RemoveAllModifiers();
            gravity.ModifierValue = 0;
        }
        if (contact.normal.Equals(Vector2.down))
        {
            ForcesVector.Y.RemoveAllModifiers();
            gravity.ModifierValue = 0;
        }
        if(contact.normal.Equals(Vector2.left) || contact.normal.Equals(Vector2.right))
        {
            ForcesVector.X.RemoveAllModifiers();
        }
    }
    bool hitDown;
}
