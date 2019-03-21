using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthControl))]
public class GoldReward : MonoBehaviour
{

    [Tooltip("First is base value, second is added range.")]
    public Vector2 goldReward;
    public int chosenGoldValue;
    private int leftToSpawn;
    [Space]
    public bool giveOnHit;
    public bool onHitPercent;
    public GameObject geoPrefab;

    HealthControl healthControl;

    private void Awake()
    {
        healthControl = GetComponent<HealthControl>();
        healthControl.onHitDeath += GiveReward;
        chosenGoldValue = (int)(goldReward.x + Random.Range(0f, goldReward.y));
        leftToSpawn = chosenGoldValue;

        if (giveOnHit)
        {
            healthControl.onHitAlive += GivePercent;
        }
    }

    private void GiveReward(Attack attack)
    {
        for(int i = 0; i < leftToSpawn; i++)
        {
            GameObject spawnedGeo = Instantiate(geoPrefab, transform.position, Quaternion.identity);
            spawnedGeo.GetComponent<Rigidbody2D>().velocity = Random.insideUnitCircle.normalized * Random.Range(13f, 13f);
        }
    }

    private void GivePercent(Attack attack)
    {
        //float value = chosenGoldValue 
    }


}
