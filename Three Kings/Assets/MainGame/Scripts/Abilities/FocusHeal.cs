using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FocusHeal : Ability
{
    [Header("Focus Heal:")]
    public int cost;
    public float timeBetweenCasts = 0.25f;   
    public float completeTime = 1f;
    [Range(0, 100)]
    public float paymentTime = 40;
    public float speedReductionPercent = 0.50f;
    public bool isHealing;
    public bool hasPayed;

    [Header("References:")]
    public GameObject healProgressParticles;
    public GameObject healthFinishParticles;

    FloatModifier speedReducer;

    public Action FocusHealthControl;
    public Action onPayment;
    public Action onComplete;

    private StateSetter focusState;

    float healTimer;
    float interTimer;

    CameraManager.Shake progressShake = new CameraManager.Shake(0.7f, 1f);
    CameraManager.Shake finishShake = new CameraManager.Shake(2.3f, 1f, 0.25f);

    protected override void Awake()
    {
        base.Awake();

        speedReducer = new FloatModifier(speedReductionPercent, FloatModifier.FloatModType.PercentMult, this);
        Action FocusHealInterUpdate = aEntity.BaseActionUpdate;
        FocusHealInterUpdate += FocusHealUpdate;
        focusState = new StateSetter(this, FocusHealthControl, FocusHealInterUpdate, aEntity.BaseActionFixedUpdate, CancelFocusHeal);
    }

    protected override void Update()
    {
        base.Update();
        if(interTimer < 0)
        {
            CanActivate = true;
        }
        else
        {
            CanActivate = false;
            interTimer -= Time.deltaTime;
        }
    }

    protected override void CastAbilityImpl()
    {
        if (!isHealing)
        {
            BeginHeal();
        }
    }

    private void FocusHealControl()
    {
        base.BaseActionControl();

        InputVector.X.BaseValue = InputSmoothing("Horizontal") * InputMultiplier;

        if (!Input.GetKey(KeyCode.B))
        {
            focusHealAbility.Cancel();
            OriginalStateSet();
        }

        if (Input.GetButtonDown("Dash"))
        {
            dashAbility.CastAbility();
        }

    }

    private void BeginHeal()
    {
        isHealing = true;

        if (!aEntity.InputVector.X.FloatModifiers.Contains(speedReducer))
        {
            aEntity.InputVector.X.AddSingleModifier(speedReducer);
        }
        aEntity.SetLivingEntityState(focusState, false);
        hasPayed = false;
        healTimer = 0;
        healProgressParticles.SetActive(true);
    }

    private void FocusHealUpdate()
    {
        if(!hasPayed && healTimer > (completeTime * (paymentTime/100)))
        {
            onPayment?.Invoke();
            CameraManager.instance.AddShake(progressShake);
            hasPayed = true;
        }

        if(healTimer > completeTime)
        {
            onComplete?.Invoke();

            CameraManager.instance.RemoveShake(progressShake);
            CameraManager.instance.AddShake(finishShake);

            healProgressParticles.SetActive(false);
            Instantiate(healthFinishParticles, transform.position, Quaternion.identity);

            isHealing = false;
            interTimer = timeBetweenCasts;

            aEntity.OriginalStateSet();
        }

        healTimer += Time.deltaTime;
    }



    public override void Cancel()
    {
        base.Cancel();
        if (isHealing)
        {
            CancelFocusHeal();
        }
    }

    private void CancelFocusHeal()
    {
        isHealing = false;
        hasPayed = false;
        healTimer = 0;
        CameraManager.instance.RemoveShake(progressShake);
        healProgressParticles.SetActive(false);
        aEntity.InputVector.X.RemoveSingleModifier(speedReducer, false);
    }


}
