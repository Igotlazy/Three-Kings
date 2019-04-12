using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PhysicsObject : MonoBehaviour
{
    public Rigidbody2D ObjectRb2d { get; private set; }

    public Vector2 ObjectVelocity { get; private set; }

    public FloatModifier gravity;
    public bool hasGravity;

    public float terminalVelocity = -5f;

    public AdvancedFloat xMovement = new AdvancedFloat();
    public AdvancedFloat yMovement = new AdvancedFloat();


    protected ContactFilter2D contactFilter2D;
    protected const float minMoveDistance = 0.001f;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
    protected const float shellRadius = 0.01f;
    protected float minGroundNormalY = 0.065f;

    protected bool grounded;

    private void Awake()
    {
        ObjectRb2d = GetComponent<Rigidbody2D>();
        gravity = new FloatModifier(0f, FloatModifier.FloatModType.Flat);
        yMovement.AddSingleModifier(gravity);

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
        
    }

    private void FixedUpdate()
    {
        grounded = false;
        ObjectMove(new Vector2(CalculateMoveX(), CalculateMoveY()));
    }



    private void ObjectMove(Vector2 moveVector)
    {
        float distance = moveVector.magnitude;
        if(distance > minMoveDistance)
        {
            int count = ObjectRb2d.Cast(moveVector, contactFilter2D, hitBuffer, distance + shellRadius);
            hitBufferList.Clear();
            for(int i = 0; i < count; i++)
            {
                hitBufferList.Add(hitBuffer[i]);
            }

            for(int i = 0; i < hitBufferList.Count; i++)
            {
                Vector2 currentNormal = hitBufferList[i].normal;
                if(currentNormal.y > minGroundNormalY)
                {
                    grounded = true;
                }
            }
        }
        ObjectRb2d.position += moveVector;
    }


    private float CalculateMoveX()
    {
        return xMovement.Value; 
    }
    private float CalculateMoveY()
    {
        if (hasGravity)
        {
            gravity.ModifierValue += Physics2D.gravity.y * Time.deltaTime;
            gravity.ModifierValue = Mathf.Clamp(gravity.ModifierValue, terminalVelocity, 1000);
        }
        else
        {
            gravity.ModifierValue = 0;
        }

        return yMovement.Value;
    }

}
