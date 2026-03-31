using System.Collections.Generic;

[System.Serializable]
public class DialogueEntry
{
    public string alertName;   
    public string icon;        
    public string text;       
}
[System.Serializable]
public class DialogueList
{
    public List<DialogueEntry> dialogues = new List<DialogueEntry>();
}