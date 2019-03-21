using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glide : Ability
{

    [Header("Glide:")]
    public float glideSpeed = -4f;
    public bool isGliding;
    public LivingEntity.TerminalVel currentTerminal;

    [Header("References:")]
    public ParticleSystem glideParticles;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }


    public override void AbilityUpdate()
    {
        base.AbilityUpdate();
    }

    protected override void CastAbilityImpl()
    {
        if (!isGliding)
        {
            PGlide();
            glideParticles.gameObject.SetActive(true);
        }
    }

    public override void Cancel()
    {
        if (isGliding)
        {
            GlideCancel();
            glideParticles.gameObject.SetActive(false);
        }
        base.Cancel();
    }


    private void PGlide()
    {
        Debug.Log("Glide");
        isGliding = true;
        if (aEntity.terminalVels.Contains(currentTerminal))
        {
            aEntity.terminalVels.Remove(currentTerminal);
        }
        LivingEntity.TerminalVel term = new LivingEntity.TerminalVel(glideSpeed);
        currentTerminal = term;
        aEntity.terminalVels.Add(term);
    }

    private void GlideCancel()
    {
        Debug.Log("Cancel Glide");
        isGliding = false;
        if (aEntity.terminalVels.Contains(currentTerminal))
        {
            aEntity.terminalVels.Remove(currentTerminal);
        }
        currentTerminal = null;
    }
}
