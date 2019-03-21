using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobilityRefresher : Interactable
{
    public GameObject sprite;
    private Collider2D col;

    public float respawnTimer = 2f;

    protected override void Start()
    {
        base.Start();
        col = GetComponent<Collider2D>();
    }


    protected override void Update()
    {
        base.Update();
        if (inRange)
        {
            StartCoroutine(Time());
        }
    }

    private IEnumerator Time()
    {
        Player.instance.ResetMobility();
        col.enabled = false;
        sprite.SetActive(false);
        inRange = false;

        yield return new WaitForSeconds(respawnTimer);

        col.enabled = true;
        sprite.SetActive(true);

    }

    
}
