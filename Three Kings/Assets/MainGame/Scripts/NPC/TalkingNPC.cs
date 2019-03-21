using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkingNPC : Interactable
{
    public GameObject interactIndicator;
    public ConversationPacket conversation;
    private Player swathe;
    private bool moveDuringConvo;

    protected override void Start()
    {
        base.Start();
        swathe = Player.instance;
        interactIndicator.SetActive(false);
        if (conversation.isAutomatic)
        {
            moveDuringConvo = true;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (inRange && !interacting)
        {
            interactIndicator.SetActive(true);
        }
        else
        {
            interactIndicator.SetActive(false);
        }
    }

    protected override void Interact()
    {
        base.Interact();

        if (Input.GetAxisRaw("Vertical") > 0 && inRange && !interacting && Player.instance.currControlType == LivingEntity.ControlType.CanControl && Player.instance.gameObject.activeInHierarchy)
        {
            if((swathe.transform.position.x > transform.position.x && swathe.isLookingRight) || (swathe.transform.position.x < transform.position.x && !swathe.isLookingRight))
            {
                swathe.EntityFlip();
            }
            if (!moveDuringConvo)
            {
                swathe.EntityControlTypeSet(LivingEntity.ControlType.CannotControl, true);
            }

            UIManager.instance.textBoxUI.EndConvo += EndConvo;
            UIManager.instance.textBoxUI.LoadConversation(conversation);
            UIManager.instance.textBoxUI.ProgressConversation();
            interacting = true;
        }
    }

    private void EndConvo()
    {
        if (!moveDuringConvo)
        {
            swathe.EntityControlTypeSet(LivingEntity.ControlType.CanControl, true);
        }
        interacting = false;
    }

    
}
