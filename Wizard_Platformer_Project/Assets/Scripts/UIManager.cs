using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    public GameObject mainMenuPanel;
    public GameObject playerUIPanel;
    public GameObject endGamePanel;

    [Header("Main Menu")]
    public Button playButton;
    public Button quitButtonMain;

    [Header("Player HUD")]
    public TextMeshProUGUI coinText;
    public Image coinIcon;
    public TextMeshProUGUI scoreText;

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

    [Header("End Game Screen")]
    public TextMeshProUGUI endTitleText;
    public TextMeshProUGUI endScoreText;
    public TextMeshProUGUI endCoinsText;
    public TextMeshProUGUI endTimeText;
    public Button restartButton;
    public Button mainMenuButtonEnd;
    public Button quitButtonEnd;

    // -----------------------------------------------------------------------
    // INIT
    // -----------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        playButton?.onClick.AddListener(() => GameManager.Instance?.StartGame());
        quitButtonMain?.onClick.AddListener(() => GameManager.Instance?.QuitGame());

        restartButton?.onClick.AddListener(() => GameManager.Instance?.RestartGame());
        mainMenuButtonEnd?.onClick.AddListener(() => GameManager.Instance?.ReturnToMainMenu());
        quitButtonEnd?.onClick.AddListener(() => GameManager.Instance?.QuitGame());

        dialogueNextButton?.onClick.AddListener(() => DialogueManager.Instance?.ShowNext());

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged += UpdateCoins;
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLivesChanged += UpdateLives;
        }

        ResetAllUI();
        ShowMainMenu();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCoinsChanged -= UpdateCoins;
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
        }
    }

    // -----------------------------------------------------------------------
    // SCREEN CONTROL
    // -----------------------------------------------------------------------

    public void ShowMainMenu()
    {
        SetPanels(true, false, false);
        ResetAllUI();
    }

    public void ShowPlayerUI()
    {
        SetPanels(false, true, false);
        ResetKeyIcon();

        if (GameManager.Instance != null)
        {
            UpdateCoins(GameManager.Instance.GetCoins());
            UpdateScore(GameManager.Instance.GetScore());
            UpdateLives(GameManager.Instance.GetLives());
        }
    }

    public void ShowEndGameScreen(int score, int coins, string formattedTime)
    {
        SetPanels(false, false, true);

        if (endTitleText != null) endTitleText.text = "GAME OVER";
        if (endScoreText != null) endScoreText.text = "Score: " + score;
        if (endCoinsText != null) endCoinsText.text = "Coins: " + coins;
        if (endTimeText != null) endTimeText.text = "Time: " + formattedTime;
    }

    private void SetPanels(bool main, bool hud, bool end)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(main);
        if (playerUIPanel != null) playerUIPanel.SetActive(hud);
        if (endGamePanel != null) endGamePanel.SetActive(end);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // RESET ALL UI (IMPORTANT FOR RESTART FIX)
    // -----------------------------------------------------------------------

    private void ResetAllUI()
    {
        if (coinText != null) coinText.text = "0";
        if (scoreText != null) scoreText.text = "Score: 0";
        if (livesText != null) livesText.text = "3";

        ResetKeyIcon();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // HUD UPDATES
    // -----------------------------------------------------------------------

    private void UpdateCoins(int count)
    {
        if (coinText != null)
            coinText.text = count.ToString();
    }

    private void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    private void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = lives.ToString();

        if (heartIcon != null)
            heartIcon.color = lives > 0 ? Color.red : Color.gray;
    }

    // -----------------------------------------------------------------------
    // KEY SYSTEM
    // -----------------------------------------------------------------------

    public void ShowKeyCollected()
    {
        if (keyIcon != null && keyCollectedSprite != null)
            keyIcon.sprite = keyCollectedSprite;

        SetKeyAlpha(1f);

        if (keyLabel != null)
            keyLabel.text = "KEY";

        if (keySection != null)
            StartCoroutine(PunchScale(keySection.transform));
    }

    private void ResetKeyIcon()
    {
        if (keyIcon != null && keyLockedSprite != null)
            keyIcon.sprite = keyLockedSprite;

        SetKeyAlpha(0.35f);

        if (keyLabel != null)
            keyLabel.text = "KEY";
    }

    private void SetKeyAlpha(float a)
    {
        if (keyIcon == null) return;

        Color c = keyIcon.color;
        c.a = a;
        keyIcon.color = c;
    }

    // -----------------------------------------------------------------------
    // DIALOGUE
    // -----------------------------------------------------------------------

    public void DisplayDialogue(DialogueItem item)
    {
        if (dialoguePanel == null) return;

        if (dialogueAlertNameText != null)
            dialogueAlertNameText.text = item.alertName;

        if (dialogueBodyText != null)
            dialogueBodyText.text = item.dialogue;

        Sprite icon = Resources.Load<Sprite>("DialogueIcons/" + item.iconName);

        if (dialogueIconImage != null)
            dialogueIconImage.sprite = icon != null ? icon : dialogueDefaultIcon;

        dialoguePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public bool IsDialogueVisible =>
        dialoguePanel != null && dialoguePanel.activeSelf;

    // -----------------------------------------------------------------------
    // ANIMATION
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
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        t = 0f;

        while (t < duration)
        {
            target.localScale = Vector3.Lerp(big, original, t / duration);
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        target.localScale = original;
    }
}