using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DialogueManager: Singleton that loads dialogue from a JSON file,
/// builds a DialogueQueue at runtime, and calls UIManager directly to show/hide dialogue.
/// DialogueUIController is no longer needed in the scene.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("JSON Dialogue File")]
    [Tooltip("Drag the JSON TextAsset for THIS level into this slot.")]
    public TextAsset dialogueJsonFile;

    // Runtime queue — holds dialogue items for the active NPC
    private DialogueQueue<DialogueItem> _activeQueue = new DialogueQueue<DialogueItem>();

    // Lookup: npcID -> list of DialogueItems (built once at Awake)
    private Dictionary<string, List<DialogueItem>> _dialogueLookup
        = new Dictionary<string, List<DialogueItem>>();

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        LoadDialogueFromJSON();
    }

    private void Start()
    {
        // UIManager owns all dialogue UI — warn early if it's missing
        if (UIManager.Instance == null)
            Debug.LogWarning("[DialogueManager] UIManager not found in scene. " +
                             "Dialogue panel will not display.");
    }

    // -----------------------------------------------------------------------
    // JSON Loading
    // -----------------------------------------------------------------------

    private void LoadDialogueFromJSON()
    {
        if (dialogueJsonFile == null)
        {
            Debug.LogError("[DialogueManager] No JSON file assigned in the Inspector!");
            return;
        }

        LevelDialogueData data = JsonUtility.FromJson<LevelDialogueData>(dialogueJsonFile.text);

        if (data == null || data.npcDialogues == null)
        {
            Debug.LogError("[DialogueManager] Failed to parse JSON. Check file format.");
            return;
        }

        _dialogueLookup.Clear();
        foreach (NPCDialogue npc in data.npcDialogues)
        {
            if (!_dialogueLookup.ContainsKey(npc.npcID))
                _dialogueLookup[npc.npcID] = npc.dialogueItems;
        }

        Debug.Log($"[DialogueManager] Loaded: {data.levelName} | NPCs: {_dialogueLookup.Count} " +
                  $"| IDs: {string.Join(", ", _dialogueLookup.Keys)}");
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called by NPCInteractable when the player presses the interact key.
    /// Finds the NPC's dialogue in the lookup, loads it into the queue, shows first item.
    /// </summary>
    public void StartNPCDialogue(string npcID)
    {
        if (!_dialogueLookup.TryGetValue(npcID, out List<DialogueItem> items))
        {
            Debug.LogWarning($"[DialogueManager] No dialogue found for NPC ID: '{npcID}' " +
                             $"| Valid IDs: {string.Join(", ", _dialogueLookup.Keys)}");
            return;
        }

        _activeQueue.LoadFromList(items);
        ShowNext();
    }

    /// <summary>
    /// Dequeues the next dialogue item and sends it to UIManager to display.
    /// If queue is empty, hides the dialogue panel.
    /// Called by the Next button (wired in UIManager.Start).
    /// </summary>
    public void ShowNext()
    {
        if (_activeQueue.IsEmpty)
        {
            UIManager.Instance?.HideDialogue();
            return;
        }

        DialogueItem item = _activeQueue.Dequeue();
        UIManager.Instance?.DisplayDialogue(item);
    }

    public bool IsDialogueActive()
    {
        return UIManager.Instance != null && UIManager.Instance.IsDialogueVisible;
    }
}
