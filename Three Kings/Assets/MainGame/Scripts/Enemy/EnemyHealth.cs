using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : HealthControl
{
    public SpriteRenderer sprite;
    Color lerpedColor;
    float flashTimer;
    public float flashSpeed = 0.3f;

    public bool flashRed;
    protected override void Start()
    {
        base.Start();
    }


    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (flashRed)
        {
            lerpedColor = Color.Lerp(Color.blue, Color.white, flashTimer / flashSpeed);
            sprite.color = lerpedColor;
            flashTimer += Time.deltaTime;
            if(lerpedColor == Color.white)
            {
                flashRed = false;
                flashTimer = 0;
            }
        }
    }

    protected override void AliveHit()
    {
        flashRed = true;
        flashTimer = 0;
        base.AliveHit();
    }

    public override void Death()
    {
        base.Death();

        Destroy(this.gameObject);
    }
}
