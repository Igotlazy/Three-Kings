using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ConversationPacket
{
    [System.Serializable]
    public struct Line
    {
        public string talker;
        [TextArea] public string speech;
    }

    public List<Line> line = new List<Line>();
    [HideInInspector] public int lineNum;
    public bool isAutomatic;

    public ConversationPacket(List<Line> conversation)
    {
        this.line = conversation;
    }

}