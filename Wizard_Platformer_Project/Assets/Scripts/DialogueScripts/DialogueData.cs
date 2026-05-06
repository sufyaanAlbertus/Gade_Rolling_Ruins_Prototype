using System;
using System.Collections.Generic;


[Serializable]
public class DialogueItem
{
    public string alertName;    // Name shown in the dialogue UI header
    public string iconName;     // Name of the sprite/icon asset to display
    public string dialogue;     // The actual dialogue text
}


[Serializable]
public class NPCDialogue
{
    public string npcID;                        // Unique identifier for the NPC (e.g. "npc_elder")
    public List<DialogueItem> dialogueItems;    // Ordered list of dialogue lines for this NPC
}


[Serializable]
public class LevelDialogueData
{
    public string levelName;                    // e.g. "Beginner", "Advanced", "Expert"
    public List<NPCDialogue> npcDialogues;      // All NPC dialogues for this level
}
