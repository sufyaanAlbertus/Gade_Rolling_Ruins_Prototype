using UnityEngine;
using UnityEngine.InputSystem;

public class NPCInteractable : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcID = "npc_01";

    [Header("Prompt")]
    public GameObject interactPrompt;

    private bool _playerInRange;

    private PlayerControls controls;

    // -----------------------------------------------------------------------
    // Setup
    // -----------------------------------------------------------------------

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();

        // 🔥 Use Interact action
        controls.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        controls.Player.Interact.performed -= OnInteract;
        controls.Disable();
    }

    private void Start()
    {
        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // Input
    // -----------------------------------------------------------------------

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!_playerInRange) return;

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueActive())
            return;

        DialogueManager.Instance?.StartNPCDialogue(npcID);
    }

    // -----------------------------------------------------------------------
    // Trigger Detection
    // -----------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = true;

        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    // 3D support
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = true;

        if (interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }
}