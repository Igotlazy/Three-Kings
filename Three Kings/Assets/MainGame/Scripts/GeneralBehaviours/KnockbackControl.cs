using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class KnockbackControl : MonoBehaviour
{
    [Header("Knockback")]
    public bool isKnockingBack;
    public bool isHardKnocking;
    public bool isSoftKnocking;
    private Rigidbody2D entityRB2D;

    public float inputReducer = 1;

    public AdvancedFloat knockbackX;
    private FloatModifier xMod;



    private void Start()
    {
        entityRB2D = GetComponent<Rigidbody2D>();
        xMod = new FloatModifier(0, FloatModifier.FloatModType.PercentMult, this);
        knockbackX.AddSingleModifier(xMod);
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
            entityRB2D.velocity = new Vector2(entityRB2D.velocity.x, knock.y);
        }
        if (knock.x != 0)
        {
            isHardKnocking = true;

            inputReducer = 0;
            xMod.ModifierValue = 1;

            knockbackX.BaseValue = knock.x;

            while(internalTime <= initialDur)
            {
                internalTime += Time.fixedDeltaTime;
                yield return waitForFix;
            }

            isHardKnocking = false;

            while (!(inputReducer == 1 && xMod.ModifierValue == 0))
            {
                if (internalTime <= (initialDur + subDur))
                {
                    isSoftKnocking = true;
                    knockbackSubLerpTime = (internalTime - initialDur) / subDur;
                    inputReducer = Mathf.Lerp(0f, 1f, knockbackSubLerpTime);
                    xMod.ModifierValue = Mathf.Lerp(1f, 0f, knockbackSubLerpTime);

                    internalTime += Time.fixedDeltaTime;
                    yield return waitForFix;
                }
                else
                {
                    inputReducer = 1;
                    knockbackX.BaseValue = 0;
                    xMod.ModifierValue = 0;
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
            inputReducer = 1;
            knockbackX.BaseValue = 0;
            xMod.ModifierValue = 0;

            isSoftKnocking = false;
            isHardKnocking = false;
            isKnockingBack = false;
        }

    }
}
