using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : HealthControl
{
    Color lerpedColor;
    float flashingTimer;

    [Header("Swathe Health")]
    public float flashingSpeed = 1f;
    public override float MaxHealth
    {
        get
        {
            return base.MaxHealth;
        }
        set
        {
            base.MaxHealth = value;
            HealthSetEvent?.Invoke(CurrentHealth, MaxHealth);
        }
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
            HealthSetEvent?.Invoke(CurrentHealth, MaxHealth);
        }
    }
    public override bool IsInvincible
    {
        get
        {
            return base.IsInvincible;

        }

        set
        {
            base.IsInvincible = value;
            if (value == true)
            {
                lerpedColor = Color.white;
                flashingTimer = 0;
            }
            else
            {
                Player.instance.sprite.color = Color.white;
            }

        }
    }

    public delegate void HealthSet(float currentHealth, float maxHealth);
    public event HealthSet HealthSetEvent;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (IsInvincible)
        {
            lerpedColor = Color.Lerp(Color.white, Color.black, Mathf.PingPong(flashingTimer * flashingSpeed, 1));
            Player.instance.sprite.color = lerpedColor;
            flashingTimer += Time.deltaTime;
        }
    }

    protected override void AliveHit()
    {
        base.AliveHit();

        CameraManager.instance.AddShake(new CameraManager.Shake(5f, 1.5f, 0.3f));
        GameController.instance.TimeScaleSlowDown(0.01f, 0.3f, 0.2f);

        if (lastAttack.damageSource.CompareTag("Hazard"))
        {
            GameController.instance.respawnManager.InitiateMiniRespawn();
        }
    }

    public override void Death()
    {
        base.Death();

        CameraManager.instance.AddShake(new CameraManager.Shake(5f, 1.5f, 1f));
        GameController.instance.TimeScaleSlowDown(0.01f, 0.5f, 0.4f);
        GameController.instance.respawnManager.InitiateMajorRespawn();

        Debug.Log("Player Death");
    }
}


