using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RedBlueGames.Tools.TextTyper;
using System;

public class TextBoxUI : MonoBehaviour
{
    public GameObject ConversationBox;
    public GameObject endTextBox;
    public TextMeshProUGUI conversationText;
    public TextMeshProUGUI talkerName;
    public TextTyper textTyper;

    private ConversationPacket convoPacket;
    public Action EndConvo;

    float autoWait = 1f;
    float autoTimer;
 
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump") && convoPacket != null && !convoPacket.isAutomatic)
        {
            ProgressConversation();
        }

        if(convoPacket != null && convoPacket.isAutomatic && !textTyper.IsTyping && autoTimer < autoWait)
        {
            autoTimer += Time.deltaTime;
            if (autoTimer >= autoWait)
            {
                ProgressConversation();
                autoTimer = 0;
            }
        }
    }

    public void ProgressConversation()
    {
        if (!textTyper.IsTyping && convoPacket.lineNum <= convoPacket.line.Count - 1)
        {
            if (convoPacket.lineNum == 0)
            {
                ConversationBox.SetActive(true);
            }
            endTextBox.SetActive(false);

            textTyper.TypeText(convoPacket.line[convoPacket.lineNum].speech, 0.02f);
            talkerName.text = convoPacket.line[convoPacket.lineNum].talker;
            convoPacket.lineNum++;
        }
        else
        {
            if (textTyper.IsTyping)
            {
                textTyper.Skip();
            }
            else if(convoPacket.lineNum > convoPacket.line.Count - 1)
            {
                conversationText.text = string.Empty;
                talkerName.text = string.Empty;
                ConversationBox.SetActive(false);
                endTextBox.SetActive(false);

                FinishedConversation();
            }
        }
    }

    public void LoadConversation(ConversationPacket convoPacket)
    {
        this.convoPacket = convoPacket;
    }

    public void FinishedConversation()
    {
        if(convoPacket.lineNum > convoPacket.line.Count - 1)
        {
            EndConvo?.Invoke();
            EndConvo = null;
            convoPacket.lineNum = 0;
            convoPacket = null;
        }
    }

    public void EndLine()
    {
        if (!convoPacket.isAutomatic)
        {
            endTextBox.SetActive(true);
        }
    }
}
