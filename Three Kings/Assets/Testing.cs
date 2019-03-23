using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    public FloatModifier controller;

    private void Awake()
    {
        controller = new FloatModifier(3, FloatModifier.FloatModType.Flat, 4f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other == Player.instance.entityBC2D)
        {
            Player.instance.outsideSourceSpeed.AddSingleModifier(controller);
        }
    }
}
