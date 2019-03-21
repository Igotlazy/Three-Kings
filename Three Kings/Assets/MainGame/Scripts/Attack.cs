using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack
{

    public float damageValue;
    public bool doesKnockback;
    public Vector2 knockback;
    public GameObject damageSource;

    public bool isInstaKill;
    public bool ignoreInvincibility;

    public static Vector2 standardPlayerKnockBack = new Vector2(9f, 9f);

    public Attack(float _damageValue, GameObject _damageSource)
    {
        damageValue = _damageValue;
        damageSource = _damageSource;

        knockback = Vector2.zero;

    }

    public Attack(float _damageValue, GameObject _damageSource, Vector2 _knockback)
    {
        damageValue = _damageValue;
        damageSource = _damageSource;

        doesKnockback = true;
        knockback = _knockback;

    }

    public Attack(bool _isInstaKill, GameObject _damageSource)
    {
        isInstaKill = _isInstaKill;
        damageSource = _damageSource;
    }
}

