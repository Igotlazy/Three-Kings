using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkingNPC : Interactable
{
    public GameObject interactIndicator;
    public ConversationPacket conversation;
    private Player swathe;
    private bool moveDuringConvo;

    public StateSetter npcTalkSetter;

    protected override void Start()
    {
        base.Start();
        npcTalkSetter = new StateSetter(this,null, null, Player.instance.BaseActionUpdate, Player.instance.BaseActionFixedUpdate, StateSetter.SetStrength.Medium);
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
        if ((swathe.transform.position.x > transform.position.x && swathe.isLookingRight) || (swathe.transform.position.x < transform.position.x && !swathe.isLookingRight))
        {
            swathe.EntityFlip();
        }
        if (!moveDuringConvo)
        {
            swathe.SetLivingEntityState(npcTalkSetter, false);
            swathe.InputAndPhysicsCleanUp();
        }

        UIManager.instance.textBoxUI.EndConvo += EndConvo;
        UIManager.instance.textBoxUI.LoadConversation(conversation);
        UIManager.instance.textBoxUI.ProgressConversation();
        interacting = true;
    }

    private void EndConvo()
    {
        if (!moveDuringConvo)
        {
            swathe.OriginalStateSet();
        }
        interacting = false;
    }

    
}
