using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private DialogueManager dialogueManager;

    private bool playerDetected = false;

    private void Update()
    {
        if (playerDetected && Input.GetKeyDown(interactKey))
        {
            if (dialogueManager != null)
            {
                dialogueManager.BeginDialogue();
            }
            else
            {
                Debug.LogWarning("NPCSystem has no DialogueManager assigned.", this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerDetected = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerDetected = false;
        }
    }
}
