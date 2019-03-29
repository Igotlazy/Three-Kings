using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool inRange;
    public bool interacting;

    protected virtual void Start()
    {
        GameController.instance.respawnManager.playerRespawn += onRespawn;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        Interact();
    }

    private void onRespawn()
    {
        if (inRange)
        {
            inRange = false;
        }
    }

    protected virtual void Interact()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && Player.instance.currControlType == LivingEntity.ControlType.CanControl)
        {
            inRange = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            inRange = false;
        }
    }
}
