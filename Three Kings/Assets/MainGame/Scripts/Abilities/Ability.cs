using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Ability : MonoBehaviour
{
    [HideInInspector] public LivingEntity aEntity;
    [Header("ABILITY:")]
    [SerializeField] private bool abilityActivated = true;
    public virtual bool AbilityActivated
    {
        get
        {
            return abilityActivated;
        }
        set
        {
            abilityActivated = value;
        }
    }
    private bool canCast;
    public virtual bool CanCast
    {
        get
        {
            return canCast;
        }
        set
        {
            canCast = value;
        }
    }

    public Action onAbilityCast;
    public Func<bool> castRestriction;

    protected virtual void Awake()
    {
        CanCast = true;

        LivingEntity entity = transform.parent.GetComponent<LivingEntity>();
        if (entity != null)
        {
            aEntity = entity;
        }
        else
        {
            Debug.LogError("ABILITY HAS NO ENTITY");
        }
    }

    protected virtual void Start()
    {

    }

    
    public virtual void AbilityUpdate()
    {
        
    }

    public virtual void AbilityFixedUpdate()
    {

    }

    public virtual void CastAbility()
    {
        if (AbilityActivated && CanCast && (castRestriction?.Invoke() ?? true))
        {
            onAbilityCast?.Invoke();
            CastAbilityImpl();
        }
    }
    protected abstract void CastAbilityImpl();

    public virtual void Cancel()
    {

    }
}
