using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobilityRefresher : MonoBehaviour
{
    public GameObject sprite;
    private Collider2D col;

    public float respawnTimer = 2f;

    void Start()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision == Player.instance.EntityBC2D)
        {
            StartCoroutine(Time());
        }
    }

    private IEnumerator Time()
    {
        Player.instance.ResetMobility();
        col.enabled = false;
        sprite.SetActive(false);

        yield return new WaitForSeconds(respawnTimer);

        col.enabled = true;
        sprite.SetActive(true);

    }

    
}
