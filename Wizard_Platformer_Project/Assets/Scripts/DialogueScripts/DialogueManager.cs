using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DialogueManager: Scene-local — NOT DontDestroyOnLoad.
/// Supports multiple JSON dialogue files per scene.
/// Add as many JSON files as you need in the Inspector — all NPCs from
/// all files are merged into one lookup at runtime.
///
/// SETUP:
///   Beginner scene  → add BeginnerDialogue.json to the list
///   Advanced scene  → add AdvancedDialogue.json + AdvancedNewNPCs.json to the list
///   Expert scene    → add ExpertDialogue.json to the list
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue JSON Files")]
    [Tooltip("Add one or more JSON TextAssets for this scene. All NPCs from all files are merged together.")]
    public List<TextAsset> dialogueJsonFiles = new List<TextAsset>();

    // Runtime queue — holds items for the active NPC conversation
    private DialogueQueue<DialogueItem> _activeQueue = new DialogueQueue<DialogueItem>();

    // Merged lookup from ALL json files: npcID -> list of DialogueItems
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

        LoadAllDialogueFiles();
    }

    private void Start()
    {
        if (UIManager.Instance == null)
            Debug.LogWarning("[DialogueManager] UIManager not found. " +
                             "Make sure a Canvas with UIManager exists in this scene.");
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // -----------------------------------------------------------------------
    // JSON Loading — loads ALL files and merges into one lookup
    // -----------------------------------------------------------------------

    private void LoadAllDialogueFiles()
    {
        if (dialogueJsonFiles == null || dialogueJsonFiles.Count == 0)
        {
            Debug.LogError("[DialogueManager] No JSON files assigned! " +
                           "Add at least one dialogue JSON in the Inspector.");
            return;
        }

        _dialogueLookup.Clear();
        int totalNPCs = 0;

        foreach (TextAsset jsonFile in dialogueJsonFiles)
        {
            if (jsonFile == null)
            {
                Debug.LogWarning("[DialogueManager] One of the JSON slots is empty — skipping.");
                continue;
            }

            LoadSingleFile(jsonFile, ref totalNPCs);
        }

        Debug.Log($"[DialogueManager] Loaded {dialogueJsonFiles.Count} file(s) | " +
                  $"Total NPCs: {totalNPCs} | " +
                  $"IDs: {string.Join(", ", _dialogueLookup.Keys)}");
    }

    private void LoadSingleFile(TextAsset jsonFile, ref int totalNPCs)
    {
        LevelDialogueData data = JsonUtility.FromJson<LevelDialogueData>(jsonFile.text);

        if (data == null || data.npcDialogues == null)
        {
            Debug.LogError($"[DialogueManager] Failed to parse '{jsonFile.name}'. " +
                           "Check the file format matches LevelDialogueData.");
            return;
        }

        foreach (NPCDialogue npc in data.npcDialogues)
        {
            if (string.IsNullOrEmpty(npc.npcID)) continue;

            if (!_dialogueLookup.ContainsKey(npc.npcID))
            {
                _dialogueLookup[npc.npcID] = npc.dialogueItems;
                totalNPCs++;
            }
            else
            {
                // NPC already exists from another file — merge their dialogue items
                _dialogueLookup[npc.npcID].AddRange(npc.dialogueItems);
                Debug.Log($"[DialogueManager] Merged extra dialogue for '{npc.npcID}' from '{jsonFile.name}'.");
            }
        }

        Debug.Log($"[DialogueManager] Parsed '{jsonFile.name}' (level: {data.levelName})");
    }

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Called by NPCInteractable when the player presses the interact button.
    /// Finds the NPC in the merged lookup, loads dialogue into queue, shows first item.
    /// </summary>
    public void StartNPCDialogue(string npcID)
    {
        if (!_dialogueLookup.TryGetValue(npcID, out List<DialogueItem> items))
        {
            Debug.LogWarning($"[DialogueManager] NPC ID '{npcID}' not found. " +
                             $"Valid IDs: {string.Join(", ", _dialogueLookup.Keys)}");
            return;
        }

        _activeQueue.LoadFromList(items);
        ShowNext();
    }

    /// <summary>
    /// Shows the next dialogue item in the queue.
    /// Called by StartNPCDialogue and by the Next button wired in UIManager.
    /// Hides the panel when the queue is empty.
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

    /// <summary>
    /// Returns true if the dialogue panel is currently visible.
    /// Used by NPCInteractable to block new interactions while talking.
    /// </summary>
    public bool IsDialogueActive()
    {
        return UIManager.Instance != null && UIManager.Instance.IsDialogueVisible;
    }
}