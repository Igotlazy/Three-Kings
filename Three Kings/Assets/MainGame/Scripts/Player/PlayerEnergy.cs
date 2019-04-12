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
            return base.CurrentEnergy;
        }
        set
        {
            base.CurrentEnergy = value;
            EnergySetEvent?.Invoke(CurrentEnergy, maxEnergy);
        }
    }

    public delegate void EnergySet(float currentEnergy, float maxEnery);
    public event EnergySet EnergySetEvent;

    protected override void Awake()
    {
        base.Awake();

        playerHealth = GetComponent<PlayerHealth>();
    }
}
