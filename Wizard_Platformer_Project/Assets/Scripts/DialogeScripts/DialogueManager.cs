using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    [Header("Data")]
    public string levelDialogueFileName = "BeginnerDialogues";
    public bool autoStartOnSceneLoad = true;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Image iconImage;
    public TextMeshProUGUI alertNameText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;

    private readonly MyQueue<DialogueEntry> dialogueQueue = new MyQueue<DialogueEntry>();

    private void Awake()
    {
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextClicked);
        }
    }

    private void Start()
    {
        LoadDialogueFile(levelDialogueFileName);

        if (autoStartOnSceneLoad)
        {
            ShowNextDialogue();
        }
        else
        {
            SetPanelVisible(false);
        }
    }

    private void OnDestroy()
    {
        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(OnNextClicked);
        }
    }

    public void LoadDialogueFile(string fileName)
    {
        dialogueQueue.Clear();

        TextAsset dialogueJson = Resources.Load<TextAsset>("Dialogues/" + fileName);
        if (dialogueJson == null)
        {
            Debug.LogError("Dialogue JSON not found at Resources/Dialogues/" + fileName);
            SetPanelVisible(false);
            return;
        }

        DialogueList loadedList = JsonUtility.FromJson<DialogueList>(dialogueJson.text);
        if (loadedList == null || loadedList.dialogues == null || loadedList.dialogues.Count == 0)
        {
            Debug.LogWarning("No dialogues found in file: " + fileName);
            SetPanelVisible(false);
            return;
        }

        for (int i = 0; i < loadedList.dialogues.Count; i++)
        {
            dialogueQueue.Enqueue(loadedList.dialogues[i]);
        }
    }

    public void BeginDialogue()
    {
        ShowNextDialogue();
    }

    public void OnNextClicked()
    {
        ShowNextDialogue();
    }

    private void ShowNextDialogue()
    {
        if (dialogueQueue.IsEmpty())
        {
            SetPanelVisible(false);
            return;
        }

        DialogueEntry entry = dialogueQueue.Dequeue();
        UpdateUI(entry);
        SetPanelVisible(true);
    }

    private void UpdateUI(DialogueEntry entry)
    {
        if (alertNameText != null)
        {
            alertNameText.text = entry.alertName;
        }

        if (dialogueText != null)
        {
            dialogueText.text = entry.text;
        }

        if (iconImage != null)
        {
            Sprite loadedIcon = null;

            if (!string.IsNullOrWhiteSpace(entry.icon))
            {
                loadedIcon = Resources.Load<Sprite>(entry.icon);
            }

            iconImage.sprite = loadedIcon;
            iconImage.enabled = loadedIcon != null;
        }
    }

    private void SetPanelVisible(bool isVisible)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(isVisible);
        }
    }
}
