using UnityEngine;


public class DialogueUIController : MonoBehaviour
{
 
    public bool IsVisible => UIManager.Instance != null && UIManager.Instance.IsDialogueVisible;

  
    public void DisplayDialogue(DialogueItem item)
    {
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("[DialogueUIController] UIManager not found in scene.");
            return;
        }
        UIManager.Instance.DisplayDialogue(item);
    }


    public void HideDialogue()
    {
        UIManager.Instance?.HideDialogue();
    }
}
