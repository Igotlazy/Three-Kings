using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HealthControl : MonoBehaviour
{
    [Header("Health Control")]
    [SerializeField] private float maxHealth;
    public virtual float MaxHealth
    {
        get
        {
            return maxHealth;
        }
        set
        {
            maxHealth = value;
        }
    }
    [SerializeField] private float currentHealth;
    public virtual float CurrentHealth
    {
        get
        {
            return currentHealth;
        }
        set
        {
            currentHealth = value;
        }
    }

    [Header("Invincibility")]
    public bool isInvincible;
    public virtual bool IsInvincible
    {
        set
        {
            isInvincible = value;
        }
        get
        {
            return isInvincible;
        }
    }
    public float invincibilityDuration;
    float invincibilityCounter;
    public bool undamagable;

    public GameObject deathParticles;
    public GameObject aliveHitParticles;

    public Action<Attack> onHitDeath;
    public Action<Attack> onHitAlive;
    public Action<Attack> onHit;
    public Attack lastAttack;
    public List<Collider2D> ignoreColliders;

    protected virtual void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public virtual void DealDamage(Attack receivedAttack)
    {
        if ((!IsInvincible || receivedAttack.ignoreInvincibility) && !undamagable && CurrentHealth > 0)
        {
            Collider2D collider = receivedAttack.damageSource.GetComponent<Collider2D>();
            if(collider != null && ignoreColliders.Contains(collider))
            {
                return;
            }

            lastAttack = receivedAttack;

            onHit?.Invoke(receivedAttack);
            if (receivedAttack.isInstaKill)
            {
                receivedAttack.damageValue = CurrentHealth;
            }

            Hit();

            if (CurrentHealth <= 0)
            {
                onHitDeath?.Invoke(receivedAttack);
                Death();
            }
            else
            {
                invincibilityCounter = invincibilityDuration;
                IsInvincible = true;

                onHitAlive?.Invoke(receivedAttack);
                AliveHit();
            }
        }
    }

    protected virtual void Update()
    {
        InvincibilityCounter();
    }

    public void ProcInvincibility()
    {
        invincibilityCounter = invincibilityDuration;
        IsInvincible = true;
    }

    protected virtual void InvincibilityCounter()
    {
        if (invincibilityCounter > 0)
        {
            invincibilityCounter -= Time.deltaTime;
        }
        else if (isInvincible)
        {
            IsInvincible = false;
        }
    }

    protected virtual void Hit()
    {
        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        float midHealth = CurrentHealth;
        midHealth -= lastAttack.damageValue;
        CurrentHealth = Mathf.Clamp(midHealth, 0.0f, MaxHealth);
    }

    protected virtual void AliveHit()
    {
        if (aliveHitParticles != null)
        {
            Instantiate(aliveHitParticles, transform.position, Quaternion.identity);
        }
    }

    public virtual void Death()
    {
        if (deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }
    }
}
