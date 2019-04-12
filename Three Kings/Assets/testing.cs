using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testing : MonoBehaviour
{
    float dashTime;
    float dashSpeed;
    Rigidbody2D rb;

    bool isDashing;
    float dashTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            dashTimer = 0;
            isDashing = true;
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.velocity = dashSpeed * Vector2.right;

        }
    }
}
