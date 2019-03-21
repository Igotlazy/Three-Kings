using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyPickup : Pickup
{
    public int moneyValue = 1;

    protected override void OnPickup()
    {
        GameController.instance.walletManager.CurrentMoney += moneyValue;
        base.OnPickup();
    }

}
