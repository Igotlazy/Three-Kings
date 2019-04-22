using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LivingEntity))]
public class KnockbackControl : MonoBehaviour
{
    [Header("Knockback")]
    public bool isKnockingBack;
    public bool isHardKnocking;
    public bool isSoftKnocking;
    private LivingEntity entity;

    private FloatModifier knockForce;
    private FloatModifier inputNullifer;



    private void Start()
    {
        entity = GetComponent<LivingEntity>();
        knockForce = new FloatModifier(0, FloatModifier.FloatModType.Flat, this);
        inputNullifer = new FloatModifier(0, FloatModifier.FloatModType.PercentMult, this);
        //knockbackX.AddSingleModifier(knockForce);
    }

    public virtual void StartKnockback(Vector2 knock, float xInitialDur, float xSubDur)
    {
        if (lastKnockback != null)
        {
            StopCoroutine(lastKnockback);
        }
        lastKnockback = StartCoroutine(KnockbackCouroutine(knock, xInitialDur, xSubDur));
    }
    Coroutine lastKnockback;

    protected WaitForFixedUpdate waitForFix = new WaitForFixedUpdate();


    private IEnumerator KnockbackCouroutine(Vector2 knock, float initialDur, float subDur)
    {
        isKnockingBack = true;
        float internalTime = 0;
        float knockbackSubLerpTime = 0;

        if (knock.y != 0)
        {
            entity.gravity.ModifierValue = knock.y;
        }
        if (knock.x != 0)
        {
            isHardKnocking = true;
            entity.smoothingValue = 0;

            if (!entity.InputVector.X.FloatModifiers.Contains(inputNullifer))
            {
                entity.InputVector.X.AddSingleModifier(inputNullifer);
            }
            if (!entity.ForcesVector.X.FloatModifiers.Contains(knockForce))
            {
                entity.ForcesVector.X.AddSingleModifier(knockForce);
            }

            inputNullifer.ModifierValue = 0;

            knockForce.ModifierValue = knock.x;
            knockForce.multiplier = 1;

            while (internalTime <= initialDur)
            {
                internalTime += Time.fixedDeltaTime;
                yield return waitForFix;
            }

            isHardKnocking = false;

            while (!(inputNullifer.ModifierValue == 1 && knockForce.multiplier == 0))
            {
                if (internalTime <= (initialDur + subDur))
                {
                    isSoftKnocking = true;
                    knockbackSubLerpTime = (internalTime - initialDur) / subDur;
                    inputNullifer.ModifierValue = Mathf.Lerp(0f, 1f, knockbackSubLerpTime);
                    knockForce.multiplier = Mathf.Lerp(1f, 0f, knockbackSubLerpTime);

                    internalTime += Time.fixedDeltaTime;
                    yield return waitForFix;
                }
                else
                {
                    inputNullifer.ModifierValue = 1;
                    knockForce.multiplier = 0;
                    knockForce.ModifierValue = 0;

                    entity.InputVector.X.RemoveSingleModifier(inputNullifer, false);
                    entity.ForcesVector.X.RemoveSingleModifier(knockForce, false);

                    isSoftKnocking = false;
                }
            }
        }

        isKnockingBack = false;
    }

    public void StopKnockback()
    {
        if (lastKnockback != null)
        {
            StopCoroutine(lastKnockback);

            inputNullifer.ModifierValue = 1;
            knockForce.multiplier = 0;
            knockForce.ModifierValue = 0;

            entity.InputVector.X.RemoveSingleModifier(inputNullifer, false);
            entity.ForcesVector.X.RemoveSingleModifier(knockForce, false);

            isSoftKnocking = false;
            isHardKnocking = false;
            isKnockingBack = false;
        }

    }
}
