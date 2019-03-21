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

    public float baseInputMultiplier = 1;
    public float knockbackMultiplier;
    public float knockbackIntensity;



    private void Start()
    {
        entityRB2D = GetComponent<Rigidbody2D>();
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

            baseInputMultiplier = 0;
            knockbackMultiplier = 1;

            knockbackIntensity = knock.x;
            while(internalTime <= initialDur)
            {
                internalTime += Time.fixedDeltaTime;
                yield return waitForFix;
            }

            isHardKnocking = false;

            while (!(baseInputMultiplier == 1 && knockbackMultiplier == 0))
            {
                if (internalTime <= (initialDur + subDur))
                {
                    isSoftKnocking = true;
                    knockbackSubLerpTime = (internalTime - initialDur) / subDur;
                    baseInputMultiplier = Mathf.Lerp(0f, 1f, knockbackSubLerpTime);
                    knockbackMultiplier = Mathf.Lerp(1f, 0f, knockbackSubLerpTime);

                    internalTime += Time.fixedDeltaTime;
                    yield return waitForFix;
                }
                else
                {
                    baseInputMultiplier = 1;
                    knockbackMultiplier = 0;
                    knockbackIntensity = 0;
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
            baseInputMultiplier = 1;
            knockbackMultiplier = 0;
            knockbackIntensity = 0;

            isSoftKnocking = false;
            isHardKnocking = false;
            isKnockingBack = false;
        }

    }
}
