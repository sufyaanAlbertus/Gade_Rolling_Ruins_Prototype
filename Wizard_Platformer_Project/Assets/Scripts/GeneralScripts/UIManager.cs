using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    public GameObject mainMenuPanel;
    public GameObject playerUIPanel;
    public GameObject endGamePanel;

    [Header("Main Menu (Beginner scene only)")]
    public Button playButton;
    public Button quitButtonMain;

    [Header("Player HUD")]
    public TextMeshProUGUI coinText;
    public Image coinIcon;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("Lives UI")]
    public Image heartIcon;
    public TextMeshProUGUI livesText;

    [Header("Key HUD")]
    public GameObject keySection;
    public Image keyIcon;
    public TextMeshProUGUI keyLabel;
    public Sprite keyLockedSprite;
    public Sprite keyCollectedSprite;

    [Header("Dialogue UI")]
    public GameObject dialoguePanel;
    public Image dialogueIconImage;
    public TextMeshProUGUI dialogueAlertNameText;
    public TextMeshProUGUI dialogueBodyText;
    public Button dialogueNextButton;
    public Sprite dialogueDefaultIcon;

    [Header("Controls UI")]
    [Tooltip("Your existing controls panel — toggled by the Show input action (I key / D-pad down)")]
    public GameObject controlsPanel;
    [Tooltip("Optional close button on the controls panel")]
    public Button closeControlsButton;

    [Header("End Game Screen")]
    public TextMeshProUGUI endTitleText;
    public TextMeshProUGUI endScoreText;
    public TextMeshProUGUI endCoinsText;
    public TextMeshProUGUI endTimeText;
    public Button restartButton;
    public Button mainMenuButtonEnd;
    public Button quitButtonEnd;

    // -----------------------------------------------------------------------
    // Private
    // -----------------------------------------------------------------------
    private PlayerControls _controls;
    private bool _controlsVisible = false;

    // -----------------------------------------------------------------------
    // Awake
    // -----------------------------------------------------------------------

    private void Awake()
    {
        Instance = this;
        HideAllPanels();
        _controls = new PlayerControls();
    }

    private void OnEnable()
    {
        _controls.Enable();
        // Uses the "Show" action already in your PlayerControls asset
        _controls.Player.Show.performed += OnToggleControls;
    }

    private void OnDisable()
    {
        _controls.Player.Show.performed -= OnToggleControls;
        _controls.Disable();
    }

    private void Start()
    {
        // Wire buttons
        playButton?.onClick.AddListener(() => GameManager.Instance?.StartGame());
        quitButtonMain?.onClick.AddListener(() => GameManager.Instance?.QuitGame());
        restartButton?.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        mainMenuButtonEnd?.onClick.AddListener(() => GameManager.Instance?.ReturnToMainMenu());
        quitButtonEnd?.onClick.AddListener(() => GameManager.Instance?.QuitGame());
        dialogueNextButton?.onClick.AddListener(() => DialogueManager.Instance?.ShowNext());
        closeControlsButton?.onClick.AddListener(() => HideControls());

        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged += UpdateCoins;
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnTimeChanged += UpdateTimer;
        }

        ResetTextValues();

        // Controls panel starts hidden
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= UpdateCoins;
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnTimeChanged -= UpdateTimer;
        }
        if (Instance == this) Instance = null;
    }

    // -----------------------------------------------------------------------
    // Controls panel toggle — bound to Player.Show action
    // -----------------------------------------------------------------------

    private void OnToggleControls(InputAction.CallbackContext ctx)
    {
        // Only works during gameplay
        if (GameManager.Instance == null || !GameManager.Instance.IsRunning()) return;

        // Don't open while dialogue is open
        if (IsDialogueVisible) return;

        if (_controlsVisible)
            HideControls();
        else
            ShowControls();
    }

    public void ShowControls()
    {
        if (controlsPanel == null) return;
        controlsPanel.SetActive(true);
        _controlsVisible = true;
        Time.timeScale = 0f;
    }

    public void HideControls()
    {
        if (controlsPanel == null) return;
        controlsPanel.SetActive(false);
        _controlsVisible = false;

        // Only resume if dialogue is also not open
        if (!IsDialogueVisible)
            Time.timeScale = 1f;
    }

    // -----------------------------------------------------------------------
    // Screen control
    // -----------------------------------------------------------------------

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        ResetTextValues();
        ResetKeyIcon();
    }

    public void ShowPlayerUI()
    {
        HideAllPanels();
        if (playerUIPanel != null) playerUIPanel.SetActive(true);
        ResetKeyIcon();

        if (GameManager.Instance != null)
        {
            UpdateCoins(GameManager.Instance.GetCoins());
            UpdateScore(GameManager.Instance.GetScore());
            UpdateLives(GameManager.Instance.GetLives());
            UpdateTimer(GameManager.Instance.GetTime());
        }
    }

    public void ShowEndGameScreen(int score, int coins, string formattedTime)
    {
        HideAllPanels();
        if (endGamePanel != null) endGamePanel.SetActive(true);

        if (endTitleText != null) endTitleText.text = "GAME OVER";
        if (endScoreText != null) endScoreText.text = "Score: " + score;
        if (endCoinsText != null) endCoinsText.text = "Coins: " + coins;
        if (endTimeText != null) endTimeText.text = "Time: " + formattedTime;
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (playerUIPanel != null) playerUIPanel.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Always close controls and resume time when switching screens
        if (controlsPanel != null) controlsPanel.SetActive(false);
        _controlsVisible = false;
        Time.timeScale = 1f;
    }

    private void ResetTextValues()
    {
        if (coinText != null) coinText.text = "0";
        if (scoreText != null) scoreText.text = "Score: 0";
        if (timerText != null) timerText.text = "00:00";
        if (livesText != null) livesText.text = "3";
    }

    // -----------------------------------------------------------------------
    // HUD updates
    // -----------------------------------------------------------------------

    private void UpdateCoins(int count)
    {
        if (coinText != null) coinText.text = count.ToString();
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    private void UpdateLives(int lives)
    {
        if (livesText != null) livesText.text = lives.ToString();
        if (heartIcon != null) heartIcon.color = lives > 0 ? Color.red : Color.gray;
    }

    private void UpdateTimer(float time)
    {
        if (timerText != null && GameManager.Instance != null)
            timerText.text = GameManager.Instance.FormatTime(time);
    }

    // -----------------------------------------------------------------------
    // Key system
    // -----------------------------------------------------------------------

    public void ShowKeyCollected()
    {
        if (keyIcon != null && keyCollectedSprite != null)
            keyIcon.sprite = keyCollectedSprite;
        SetKeyAlpha(1f);
        if (keyLabel != null) keyLabel.text = "KEY";
        if (keySection != null) StartCoroutine(PunchScale(keySection.transform));
    }

    private void ResetKeyIcon()
    {
        if (keyIcon != null && keyLockedSprite != null)
            keyIcon.sprite = keyLockedSprite;
        SetKeyAlpha(0.35f);
        if (keyLabel != null) keyLabel.text = "KEY";
    }

    private void SetKeyAlpha(float a)
    {
        if (keyIcon == null) return;
        Color c = keyIcon.color; c.a = a; keyIcon.color = c;
    }

    // -----------------------------------------------------------------------
    // Dialogue
    // -----------------------------------------------------------------------

    public void DisplayDialogue(DialogueItem item)
    {
        if (dialoguePanel == null) return;
        if (dialogueAlertNameText != null) dialogueAlertNameText.text = item.alertName;
        if (dialogueBodyText != null) dialogueBodyText.text = item.dialogue;

        Sprite icon = Resources.Load<Sprite>("DialogueIcons/" + item.iconName);
        if (dialogueIconImage != null)
            dialogueIconImage.sprite = icon != null ? icon : dialogueDefaultIcon;

        dialoguePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // Only resume time if controls panel is also closed
        if (!_controlsVisible)
            Time.timeScale = 1f;
    }

    public bool IsDialogueVisible =>
        dialoguePanel != null && dialoguePanel.activeSelf;

    // -----------------------------------------------------------------------
    // Punch scale animation
    // -----------------------------------------------------------------------

    private System.Collections.IEnumerator PunchScale(Transform target)
    {
        Vector3 original = target.localScale;
        Vector3 big = original * 1.35f;
        float duration = 0.12f;
        float t = 0f;

        while (t < duration)
        {
            target.localScale = Vector3.Lerp(original, big, t / duration);
            t += Time.unscaledDeltaTime; yield return null;
        }
        t = 0f;
        while (t < duration)
        {
            target.localScale = Vector3.Lerp(big, original, t / duration);
            t += Time.unscaledDeltaTime; yield return null;
        }
        target.localScale = original;
    }
}