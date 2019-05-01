using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyRewardHealth : MoneyReward
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

        healthControl.onHitDeath += GiveReward;
        if (giveOnHit)
        {
            healthControl.onHitAlive += GivePercent;
        }

        divideNum = (int)healthControl.MaxHealth - 1;
        if (divideNum <= 0)
        {
            divideNum = 1;
        };
    }

    private void OnDestroy()
    {
        healthControl.onHitAlive -= GivePercent;
    }
}
