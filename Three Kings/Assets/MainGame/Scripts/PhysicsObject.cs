using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsObject : MonoBehaviour
{
    public Rigidbody2D ObjectRb2d { get; private set; }
    public Collider2D ObjectBc2d { get; private set; }

    public Vector2 ObjectVelocity { get; private set; }

    public FloatModifier gravity;
    public FloatModifier xInput;
    public bool hasGravity;

    public float terminalVelocity = -5f;

    public AdvancedFloat xMovement = new AdvancedFloat();
    public AdvancedFloat yMovement = new AdvancedFloat();


    protected ContactFilter2D contactFilter2D;
    protected const float minMoveDistance = 0.001f;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
    protected const float shellRadius = 0.01f;

    public Vector2 velocity;
    [SerializeField]private CollisionStates lastFrameCollisions = new CollisionStates();


    private void Awake()
    {
        ObjectRb2d = GetComponent<Rigidbody2D>();
        ObjectBc2d = GetComponent<Collider2D>();

        gravity = new FloatModifier(0f, FloatModifier.FloatModType.Flat);
        gravity.ignoreRemove = true;
        xInput = new FloatModifier(7f, FloatModifier.FloatModType.Flat);

        yMovement.AddSingleModifier(gravity);
        xMovement.AddSingleModifier(xInput);

        groundMask = (1 << LayerMask.NameToLayer("Ground - Soft")) | (1 << LayerMask.NameToLayer("Ground - Hard")); 

        contactFilter2D.useTriggers = false;
        contactFilter2D.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter2D.useLayerMask = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        xInput.multiplier = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
        if (Input.GetButtonUp("Jump") && isJumping)
        {
            if(velocity.y > 0)
            {
                gravity.ModifierValue = 0;
                isJumping = false;
            }
        }
        ObjectMove(new Vector2(CalculateMoveX(), CalculateMoveY()) * Time.fixedDeltaTime);
    }



    private void ObjectMove(Vector2 moveVector)
    {
        float distance = moveVector.magnitude;
        lastFrameCollisions.Reset();
        if (distance > 0)
        {           
            int count = ObjectRb2d.Cast(moveVector, contactFilter2D, hitBuffer, distance + shellRadius);

            if (count > 0)
            {
                Debug.Log(count);
                Vector2 interVec = Vector2.zero;

                foreach (RaycastHit2D hit in hitBuffer)
                {
                    if (hit.collider != null)
                    {

                        if (hit.normal.x > 0 && moveVector.x < 0)
                        {
                            lastFrameCollisions.hitLeft = true;
                            moveVector.x = 0;
                        }
                        if(hit.normal.x < 0 && moveVector.x > 0)
                        {
                            lastFrameCollisions.hitRight = true;
                            moveVector.x = 0;
                        }
                        if(hit.normal.y < 0 && moveVector.y > 0)
                        {
                            lastFrameCollisions.hitTop = true;
                            moveVector.y = 0;
                        }
                        if(hit.normal.y > 0 && moveVector.y < 0)
                        {
                            lastFrameCollisions.hitBottom = true;
                            moveVector.y = 0;
                        }
                        

                        ColliderDistance2D dis = hit.collider.Distance(ObjectBc2d);
                        interVec += (dis.pointA - dis.pointB);
                    }
                }

                ObjectRb2d.MovePosition(ObjectRb2d.position + moveVector + interVec);

            }
            else
            {
                ObjectRb2d.MovePosition(ObjectRb2d.position + moveVector);
            }
        
        }

        velocity = moveVector / Time.fixedDeltaTime;
    }


    private float CalculateMoveX()
    {
        return xMovement.Value; 
    }
    private float CalculateMoveY()
    {
        if (hasGravity)
        {
            if (!isGrounded)
            {
                gravity.ModifierValue += Physics2D.gravity.y * Time.deltaTime;
                gravity.ModifierValue = Mathf.Clamp(gravity.ModifierValue, terminalVelocity, 1000);
            }

            GroundCheck();

            if ((isGrounded && lastFrameCollisions.hitBottom) || lastFrameCollisions.hitTop)
            {
                gravity.ModifierValue = 0;
            }
        }
        else
        {
            Debug.Log("Hello");
            if(gravity.ModifierValue != 0)
            {
                gravity.ModifierValue = Mathf.MoveTowards(gravity.ModifierValue, 0, Physics2D.gravity.y * Time.deltaTime);
                Debug.Log(gravity.ModifierValue);

                if (lastFrameCollisions.hitBottom || lastFrameCollisions.hitTop) 
                {
                    gravity.ModifierValue = 0;
                }
            }
        }

        return yMovement.Value;
    }

    private void Jump()
    {
        if (isGrounded)
        {
            gravity.ModifierValue = 17.5f;
            isJumping = true;
        }
    }
    bool isJumping = true;

    private void GroundCheck()
    {
        BoxCollider2D box = (BoxCollider2D)ObjectBc2d;

        Vector2 leftRayOrigin = new Vector2(box.bounds.center.x - box.bounds.extents.x - shellRadius, box.bounds.center.y - box.bounds.extents.y - shellRadius);
        Vector2 rightRayOrigin = new Vector2(box.bounds.center.x + box.bounds.extents.x + shellRadius, box.bounds.center.y - box.bounds.extents.y - shellRadius);
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
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    [SerializeField] bool isGrounded;
    public float groundRayLength = 0.12f;
    protected LayerMask groundMask;

    [System.Serializable]
    private class CollisionStates
    {
        public bool hitLeft, hitRight;
        public bool hitTop, hitBottom;

        public void Reset()
        {
            hitLeft = hitRight = hitTop = hitBottom = false;
        }
    }
}
