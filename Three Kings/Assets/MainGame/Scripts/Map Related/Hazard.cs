using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Did hit " + collision.gameObject.name);
        if(collision == Player.instance.hurtBox)
        {
            Attack swatheHit = new Attack(1, gameObject)
            {
                ignoreInvincibility = true
            };
            Player.instance.healthControl.DealDamage(swatheHit);
        }
        else if(collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
            if(enemyScript != null && !enemyScript.isImmuneToHazards)
            {
                Attack enemyHit = new Attack(true, gameObject);
                enemyScript.healthControl.DealDamage(enemyHit);
            }
        }
    }
}
