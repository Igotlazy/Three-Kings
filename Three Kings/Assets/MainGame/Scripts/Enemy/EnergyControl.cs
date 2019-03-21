using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyControl : MonoBehaviour
{
    public int currentEnergy;
    [SerializeField] protected int maxEnergy = 5;
    public virtual int CurrentEnergy
    {
        get
        {
            return currentEnergy;
        }
        set
        {
            currentEnergy = value;
        }
    }
}
