using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BasicSlash : Ability
{
    [Header("Slash:")]
    public float slashDamage;
    [Space]

    public Vector2 slashKnockback;
    public Vector2 slashAttackSize;
    public float pogoVelocity = 13f;
    public bool noRecoil;
    public bool noFullRecoil;
    [Space]

    public float slashCooldownDuration;
    private float slashCooldownCounter;
    [Space]

    [Header("References:")]
    public Transform slashSideTrans;
    public Transform slashUpTrans;
    public Transform slashDownTrans;
    private Transform currSlashTrans;
    public GameObject slashParticles;

    public Action<HealthControl> onSlashHit;
    public Action onPogo;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void AbilityUpdate()
    {
        base.AbilityUpdate();

        if (!CanActivate && slashCooldownCounter > 0)
        {
            slashCooldownCounter -= Time.deltaTime;
            if (slashCooldownCounter <= 0)
            {
                slashCooldownCounter = 0;
                CanActivate = true;
            }
        }
    }

    protected override void CastAbilityImpl()
    {
        PSlashAttack();
    }

    public Collider2D[] PSlashAttack()
    {
        currSlashTrans = slashSideTrans;

        float verticalAxis = Input.GetAxis("Vertical");
        if (verticalAxis != 0)
        {
            if (verticalAxis > 0)
            {
                currSlashTrans = slashUpTrans;
            }
            else if (verticalAxis < 0 && !aEntity.IsGrounded)
            {
                currSlashTrans = slashDownTrans;
            }
        }

        Instantiate(slashParticles, currSlashTrans.position, Quaternion.identity);

        Collider2D[] targetsHit = Physics2D.OverlapBoxAll(new Vector2(currSlashTrans.position.x, currSlashTrans.position.y), slashAttackSize, 0f);
        bool hitAtLeastOne = false;
        HealthControl healthControl = null;

        foreach (Collider2D col in targetsHit)
        {
            if (col.gameObject.CompareTag("Enemy") || col.gameObject.CompareTag("Hazard") || col.gameObject.CompareTag("Hitable"))
            {
                healthControl = col.gameObject.GetComponent<HealthControl>();
                if (healthControl != null)
                {
                    Attack slashAttack = new Attack(slashDamage, gameObject, slashKnockback);
                    if (currSlashTrans == slashUpTrans)
                    {
                        slashAttack.knockback.x = 0;
                    }
                    if (currSlashTrans == slashDownTrans)
                    {
                        slashAttack.knockback.x = 0;
                        slashAttack.knockback.y *= -1;
                    }

                    Debug.Log("Hit");
                    healthControl.DealDamage(slashAttack);
                }

                hitAtLeastOne = true;
                onSlashHit?.Invoke(healthControl);
            }

            healthControl = null;
        }


        if (hitAtLeastOne)
        {
            CameraManager.instance.AddShake(new CameraManager.Shake(1f, 1f, 0.2f));

            if (!noFullRecoil)
            {
                if (!noRecoil)
                {
                    DownRecoil();
                    SideRecoil();
                }
                UpRecoil();
            }

        }

        slashCooldownCounter = slashCooldownDuration;
        CanActivate = false;

        return targetsHit;
    }

    private void SideRecoil()
    {
        if (currSlashTrans == slashSideTrans) //Recoil
        {
            GameController.instance.TimeScaleSlowDown(0, 0.02f, 0f);

            if (currSlashTrans.position.x > transform.position.x)
            {
                if (aEntity.IsGrounded)
                {
                    aEntity.knockbackControl.StartKnockback(new Vector2(-7f, 0f), 0.05f, 0.05f);
                }
                else
                {
                    aEntity.knockbackControl.StartKnockback(new Vector2(-7f, 0f), 0.08f, 0.1f);
                }
            }
            else
            {
                if (aEntity.IsGrounded)
                {
                    aEntity.knockbackControl.StartKnockback(new Vector2(7f, 0f), 0.05f, 0.05f);
                }
                else
                {
                    aEntity.knockbackControl.StartKnockback(new Vector2(7f, 0f), 0.08f, 0.1f);
                }
            }
        }
    }
    private void DownRecoil()
    {
        if (currSlashTrans == slashUpTrans && aEntity.EntityVelocity.y > 0)
        {
            GameController.instance.TimeScaleSlowDown(0, 0.03f, 0f);
            aEntity.gravity.ModifierValue = -2f;
        }
    }
    private void UpRecoil()//Pogo
    {
        if (currSlashTrans == slashDownTrans)
        {
            onPogo?.Invoke();
            Debug.Log("Down Slash");
            aEntity.gravity.ModifierValue = pogoVelocity;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (currSlashTrans != null)
        {
            Gizmos.DrawWireCube(
                new Vector3(currSlashTrans.transform.position.x, currSlashTrans.transform.position.y, 0),
                new Vector3(slashAttackSize.x, slashAttackSize.y, 0));
        }
    }
}
