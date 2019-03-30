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
    private bool canActivate;
    public virtual bool CanActivate
    {
        get
        {
            return canActivate;
        }
        set
        {
            canActivate = value;
        }
    }

    public Action onAbilityCast;
    public Func<bool> castRestriction;

    public Action AbilityUpdateMethod;
    public Action AbilityFixedUpMethod;
    public Action AbilityControlMethod;

    protected virtual void Awake()
    {
        CanActivate = true;

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
    protected virtual void Update()
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
        if (AbilityActivated && CanActivate && (castRestriction?.Invoke() ?? true))
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
