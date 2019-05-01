using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiddenAreaHealth : HiddenArea
{
    [Header("Hooks:")]
    public HealthControl healthControl;
    
    protected override void SetUpHooks()
    {
        base.SetUpHooks();

        if(healthControl == null)
        {
            healthControl = GetComponent<HealthControl>();
        }

        healthControl.onHitDeath += DisableCover;
    }

    protected void DisableCover(Attack givenAttack)
    {
        Disable = true;
    }

    private void OnDestroy()
    {
        healthControl.onHitDeath -= DisableCover;
    }
}
