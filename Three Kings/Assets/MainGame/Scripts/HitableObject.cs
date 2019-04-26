using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HitableObject : HealthControl
{
    [Header("HitableObject")]
    public SpriteRenderer sprite;

    protected override void Start()
    {
        base.Start();
    }

    public override float CurrentHealth
    {
        get
        {
            return base.CurrentHealth;
        }
        set
        {
            base.CurrentHealth = value;
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, (CurrentHealth / MaxHealth) + ((1 - (CurrentHealth / MaxHealth)) * 0.4f));
        }
    }

    protected override void Hit()
    {
        CurrentHealth -= 1;
    }
}
