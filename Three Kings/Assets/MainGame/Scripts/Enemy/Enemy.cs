using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : LivingEntity {

    // Use this for initialization

    [Header("Enemy")]
    public LayerMask playerLayerMask;
    public bool isImmuneToHazards;

    protected override void Awake()
    {
        base.Awake();
    }


    protected override void Start ()
    {
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update ()
    {
        base.Update();

        EnemyContactAttack();
	}

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    private void EnemyContactAttack()
    {      
       Collider2D[] playersToDamage = Physics2D.OverlapBoxAll(entityBC2D.bounds.center, new Vector2(entityBC2D.bounds.size.x, entityBC2D.bounds.size.y), 0, playerLayerMask);
        foreach (Collider2D col in playersToDamage)
        {
            if(col == Player.instance.hurtBox)
            {
                Attack contactAttack = new Attack(1f, gameObject, Attack.standardPlayerKnockBack);
                Player.instance.healthControl.DealDamage(contactAttack);
            }
            else
            {
                LivingEntity livingScript = col.gameObject.GetComponent<LivingEntity>();
                if(livingScript != null && livingScript != Player.instance)
                {
                    Attack contactAttack = new Attack(1f, gameObject, Attack.standardPlayerKnockBack);
                    livingScript.healthControl.DealDamage(contactAttack);
                }
            }
        }

    }

    /*
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(entityBC2D.bounds.center, new Vector2(entityBC2D.bounds.size.x, entityBC2D.bounds.size.y));
    }
    */
}
