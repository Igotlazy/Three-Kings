using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalletManager : MonoBehaviour
{
    public int testMoney;
    [SerializeField] private int currentMoney;
    public int CurrentMoney
    {
        get
        {
            return currentMoney;
        }
        set
        {
            currentMoney = Mathf.Clamp(value, 0, 99999);
            MoneySetEvent?.Invoke(currentMoney);
        }
    }

    public delegate void MoneySet(int newMoneyValue);
    public event MoneySet MoneySetEvent;

    public bool MakePurchase(int cost)
    {
        if(cost <= CurrentMoney)
        {
            CurrentMoney -= cost;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            CurrentMoney += testMoney;
        }
    }
}
