using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    GameObject parent;

    void Start()
    {
        parent = transform.parent.gameObject;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject == Player.instance.gameObject)
        {
            OnPickup();
        }
    }

    protected virtual void OnPickup()
    {
        Destroy(parent);
    }

}
