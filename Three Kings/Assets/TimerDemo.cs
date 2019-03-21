using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class TimerDemo : MonoBehaviour
{
    public Collider2D collide;
    public GameObject sprite;
    public TextMeshProUGUI timer;
    public bool isActive;
    public bool counting;
    public Image stop;



    public List<AbilityGivers> givers = new List<AbilityGivers>();

    Coroutine co;
    // Start is called before the first frame update
    void Start()
    {
        givers[0].ability = Player.instance.dashAbility;
        givers[1].ability = Player.instance.wallJumpAbility;
        givers[2].ability = Player.instance.jumpAbility;
        givers[3].ability = Player.instance.glideAbility;
        givers[4].ability = Player.instance.vaultAbility;

        stop.gameObject.SetActive(false);
        timer.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && Player.instance.currControlType == LivingEntity.ControlType.CanControl)
        {
            if (isActive)
            {
                Debug.Log("GIVE2");
                collide.enabled = true;
                sprite.SetActive(true);
                StopCoroutine(co);
                timer.text = string.Empty;
                isActive = false;
                counting = false;
                stop.gameObject.SetActive(false);
            }

            foreach (AbilityGivers give in givers)
            {
                Debug.Log("GIVE");
                give.gameObject.SetActive(true);
                try
                {
                    Jump doubleJump = (Jump)give.ability;
                    doubleJump.maxExtraJumps = 0;
                    doubleJump.currentExtraJumps = 0;
                }
                catch (InvalidCastException)
                {
                    give.ability.AbilityActivated = false;
                }
            }
        }
    }

    public void StopCounting()
    {
        if(co != null)
        {
            StopCoroutine(co);
        }
        stop.gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision == Player.instance.hurtBox)
        {
            collide.enabled = false;
            sprite.SetActive(false);
            timer.text = string.Empty;
            isActive = true;
            counting = true;
            co = StartCoroutine(Timer());
        }
    }

    IEnumerator Timer()
    {
        float count = 0;

        while (counting)
        {
            int minutes = (int)count / 60;
            int seconds = (int)count - 60 * minutes;
            int milliseconds = (int)(1000 * (count - minutes * 60 - seconds));

            timer.text = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);
            count += Time.unscaledDeltaTime;

            yield return null;
        }

    }
}
