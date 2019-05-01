using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyReward : MonoBehaviour
{
    [Header("Rewards:")]
    [Tooltip("First is base value, second is added range.")]
    public Vector2 moneyReward;
    public int chosenMoneyValue;

    [Header("OnHitRewards:")]
    public bool giveOnHit;
    [Range(0, 100)]
    public float onHitPercent;
    public int percentMoney;

    [Header("Feedback:")]
    public int remainingMoney;
    protected int divideNum;

    [Header("References:")]
    public GameObject moneyPrefab;

    protected void Start()
    {
        SetUpHooks();

        chosenMoneyValue = (int)(moneyReward.x + Random.Range(0f, moneyReward.y));
        remainingMoney = chosenMoneyValue;

        if (giveOnHit)
        {
            percentMoney = (int)(chosenMoneyValue * (onHitPercent / 100f));
            if(percentMoney <= 0)
            {
                percentMoney = 1;
            }
        }
    }

    protected virtual void SetUpHooks()
    {

    }

    protected void GiveReward(Attack attack)
    {
        for(int i = 0; i < remainingMoney; i++)
        {
            SpawnMoney();
        }
    }

    protected void GivePercent(Attack attack)
    {
        int toSpawn = (int)(percentMoney / divideNum);
        if(toSpawn <= 0 && remainingMoney > 0)
        {
            toSpawn = 1;
        }
        remainingMoney -= toSpawn;
        remainingMoney = Mathf.Clamp(remainingMoney, 0, chosenMoneyValue);

        for (int i = 0; i < toSpawn; i++)
        {
            SpawnMoney();
        }
    }

    private void SpawnMoney()
    {
        GameObject spawnedGeo = Instantiate(moneyPrefab, transform.position, Quaternion.identity);
        spawnedGeo.GetComponent<Rigidbody2D>().velocity = Random.insideUnitCircle.normalized * Random.Range(12f, 12f);
    }


}
