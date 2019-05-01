using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastProjectile : MonoBehaviour {

    public float projectileLifeTime;
    public Attack attackObject;
    public GameObject onDeathParticles;

    void Start()
    {
        Invoke("DestroySelf", projectileLifeTime);
    }


    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D hitCollider)
    {
        if (hitCollider.tag == "Enemy" && hitCollider.gameObject.activeInHierarchy)
        {
            GameObject hitObject = hitCollider.gameObject;

            LivingEntity enemyScript = hitObject.GetComponent<LivingEntity>();

            if (enemyScript != null)
            {
                enemyScript.healthControl.TakeDamage(attackObject);
                Instantiate(onDeathParticles, transform.position, Quaternion.identity);
            }

            //Destroy(gameObject);
        }
    }

    private void DestroySelf()
    {
        if (onDeathParticles != null)
        {
            Instantiate(onDeathParticles, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
