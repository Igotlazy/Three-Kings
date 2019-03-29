using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastAttack : Ability
{
    [Header("Blast:")]
    public float blastDamage;
    public Vector2 blastKnockback;
    public float blastFireVel;
    public float blastCooldownDuration;
    private float blastCooldownCounter;
    public GameObject blastProjectile;
    private GameObject spawnedBlastProjectile;

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

        if(!CanActivate && blastCooldownCounter > 0)
        {
            blastCooldownCounter -= Time.deltaTime;
            if(blastCooldownCounter <= 0)
            {
                blastCooldownCounter = 0;
                CanActivate = true;
            }
        }
    }

    protected override void CastAbilityImpl()
    {
        PBlastAttack();
    }

    private void PBlastAttack()
    {
        Debug.Log("Are we here");
        if (CanActivate)
        {
            spawnedBlastProjectile = Instantiate(blastProjectile, this.transform.position, this.transform.rotation);
            Rigidbody2D blastProjectileRB2D = spawnedBlastProjectile.GetComponent<Rigidbody2D>();
            BlastProjectile blastProjectileScript = spawnedBlastProjectile.GetComponent<BlastProjectile>();

            Attack blastAttack = new Attack(blastDamage, gameObject, blastKnockback);
            blastProjectileScript.attackObject = blastAttack;

            Vector2 fireVel = new Vector2(blastFireVel, 0);
            if (!aEntity.isLookingRight)
            {
                fireVel.x *= -1;
            }

            blastProjectileRB2D.velocity = fireVel;

            blastCooldownCounter = blastCooldownDuration;
            CanActivate = false;
        }
    }
}
