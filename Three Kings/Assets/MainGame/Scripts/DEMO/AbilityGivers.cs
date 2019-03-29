using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AbilityGivers : MonoBehaviour
{
    public Ability ability;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision == Player.instance.hurtBox)
        {
            ability.AbilityActivated = true;
            gameObject.SetActive(false);
        }

        try //This is fucking garbage. Change after Demo.
        {
            Jump doubleJump = (Jump)ability;
            doubleJump.maxExtraJumps = 1;
            doubleJump.currentExtraJumps = doubleJump.maxExtraJumps;
        }
        catch (InvalidCastException)
        {

        }
    }

}
