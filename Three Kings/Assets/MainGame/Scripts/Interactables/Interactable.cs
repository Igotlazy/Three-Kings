using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Interactable : MonoBehaviour
{
    public bool inRange;
    public bool interacting;

    protected virtual void Start()
    {

    }

    // Update is called once per frame
    protected virtual void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == Player.instance.entityBC2D)
        {
            inRange = true;
            Player.instance.interactableMethod = Interact;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == Player.instance.entityBC2D)
        {
            inRange = false;
            Player.instance.interactableMethod -= Interact;
        }
    }

    protected abstract void Interact();

    private void OnDestroy()
    {
        if(Player.instance != null)
        {
            Player.instance.interactableMethod -= Interact;
        }
    }
}
