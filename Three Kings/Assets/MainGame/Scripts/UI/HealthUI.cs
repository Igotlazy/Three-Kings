using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public GameObject heartPrefab;
    public float heartDis = 55f;
    public List<GameObject> healthHearts = new List<GameObject>();

    public Image energySlider;

    private void Awake()
    {
        Player.instance.swatheHealth.HealthSetEvent += UpdateHealth;
        Player.instance.swatheEnergy.EnergySetEvent += UpdateEnergy;
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if(maxHealth > healthHearts.Count)
        {
            for (int i = healthHearts.Count; i < maxHealth; i++)
            {
                GameObject spawnedHeart = Instantiate(heartPrefab, new Vector3(this.transform.position.x + (heartDis * i), transform.position.y, transform.position.x), Quaternion.identity);
                spawnedHeart.transform.SetParent(transform);
                healthHearts.Add(spawnedHeart);
            }
        }

        if (maxHealth < healthHearts.Count)
        {
            for (int i = healthHearts.Count -1; i >= maxHealth; i--)
            {
                Destroy(healthHearts[i]);
                healthHearts.RemoveAt(i);
            }
        }

        for (int i = 0; i < healthHearts.Count; i++)
        {
            Image heartImage = healthHearts[i].GetComponent<Image>();
            if (i < currentHealth)
            {
                heartImage.color = new Color32(255, 255, 255, 255);
            }
            else
            {
                heartImage.color = new Color32(255, 255, 255, 40);
            }
        }

        Debug.Log("Update Health");
    }

    public void UpdateEnergy(float current, float max)
    {
        float set = current / max;
        energySlider.fillAmount = set;
    }

    private void OnDestroy()
    {
        Player.instance.swatheHealth.HealthSetEvent -= UpdateHealth;
        Player.instance.swatheEnergy.EnergySetEvent -= UpdateEnergy;
    }

}
