using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerEnergy : EnergyControl
{
    private PlayerHealth playerHealth;

    public override int CurrentEnergy
    {
        get
        {
            return currentEnergy;
        }
        set
        {
            if (!(base.CurrentEnergy == maxEnergy - 1 && playerHealth.CurrentHealth == playerHealth.MaxHealth))
            {
                int mid = (int)Mathf.Clamp(value, 0f, maxEnergy);
                if (mid == maxEnergy)
                {
                    currentEnergy = 0;
                    playerHealth.CurrentHealth += 1;
                }
                else
                {
                    currentEnergy = mid;
                }
            }

            EnergySetEvent?.Invoke(CurrentEnergy, maxEnergy);
        }
    }

    public delegate void EnergySet(float currentEnergy, float maxEnery);
    public event EnergySet EnergySetEvent;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        CurrentEnergy = 0;
    }
}
