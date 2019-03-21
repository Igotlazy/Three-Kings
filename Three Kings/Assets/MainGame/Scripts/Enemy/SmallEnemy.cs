using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallEnemy : Enemy
{
    [Header("Small Enemy")]
    public float movementSpeed = 1.5f;
    public float changeDirectionBaseTimer = 2f;
    float changeDirectionCounter;


    protected override void Awake()
    {
        base.Awake();
        changeDirectionBaseTimer += Random.Range(-1f, 1f);
    }


    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if(currControlType == ControlType.CanControl)
        {
            EnemyMove();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void EnemyMove()
    {
        baseInputSpeed = (movementSpeed);

        changeDirectionCounter -= Time.deltaTime;
        if (changeDirectionCounter < 0)
        {
            movementSpeed *= -1;
            changeDirectionCounter = changeDirectionBaseTimer + Random.Range(-1f, 1f);
        }
    }

}


