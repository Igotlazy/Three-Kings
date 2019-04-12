using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyControl : MonoBehaviour
{
    [SerializeField] private int currentEnergy;
    [SerializeField] protected int maxEnergy = 5;
    public virtual int CurrentEnergy
    {
        get
        {
            return currentEnergy;
        }
        set
        {
            currentEnergy = Mathf.Clamp(value, 0, maxEnergy);
        }
    }

    public bool PayEnergy(int energyToPay)
    {
        if(CurrentEnergy >= energyToPay)
        {
            CurrentEnergy -= energyToPay;
            return true;
        }
        return false;
    }

    protected virtual void Awake()
    {
        CurrentEnergy = currentEnergy;
    }
}
