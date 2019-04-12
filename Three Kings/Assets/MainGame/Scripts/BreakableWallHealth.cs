using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableWallHealth : HealthControl
{
    public SpriteRenderer sprite;

    public override float CurrentHealth
    {
        get
        {
            return base.CurrentHealth;
        }
        set
        {
            base.CurrentHealth = value;
            Debug.Log("Hello");
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, (CurrentHealth / MaxHealth) + ((1 - (CurrentHealth / MaxHealth)) * 0.4f));
        }
    }

    protected override void Hit()
    {
        CurrentHealth -= 1;
    }

    public override void Death()
    {
        base.Death();
        Destroy(gameObject);
    }

}
