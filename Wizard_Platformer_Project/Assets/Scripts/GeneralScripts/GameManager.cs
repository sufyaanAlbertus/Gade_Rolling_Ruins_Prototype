using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int maxLives = 3;

    [Header("Scene Names")]
    public string mainMenuScene = "BeginnerLevel";
    public string beginnerScene = "BeginnerLevel";

    [Header("Spawn Settings")]
    public Transform defaultSpawnPoint;

    private int currentLives;
    private int currentCoins;
    private int currentScore;
    private float elapsedTime;

    private bool isGameRunning = false;
    private bool isGameOver = false;

    public System.Action<int> OnLivesChanged;
    public System.Action<int> OnCoinsChanged;
    public System.Action<int> OnScoreChanged;
    public System.Action<float> OnTimeChanged;
    public System.Action OnGameOver;
    public System.Action OnGameStart;

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // First time only — show main menu
        StartCoroutine(ShowMainMenuNextFrame());
    }

    private void Update()
    {
        if (!isGameRunning || isGameOver) return;
        elapsedTime += Time.deltaTime;
        currentScore = Mathf.FloorToInt(elapsedTime);
        OnScoreChanged?.Invoke(currentScore);
        OnTimeChanged?.Invoke(elapsedTime);
    }

    // -----------------------------------------------------------------------
    // Scene loaded — wait one frame so UIManager.Start() runs first
    // -----------------------------------------------------------------------

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Re-find spawn point by tag
        GameObject sp = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (sp != null) defaultSpawnPoint = sp.transform;

        // Wait a frame — UIManager.Awake() has run but Start() hasn't yet
        StartCoroutine(OnSceneLoadedNextFrame());
    }

    private IEnumerator OnSceneLoadedNextFrame()
    {
        // Wait for UIManager.Start() to finish wiring up buttons/events
        yield return null;

        if (isGameRunning && !isGameOver)
        {
            UIManager.Instance?.ShowPlayerUI();
            NotifyAllUI();
            SpawnPlayerAtStart();
        }
        else
        {
            UIManager.Instance?.ShowMainMenu();
        }
    }

    private IEnumerator ShowMainMenuNextFrame()
    {
        yield return null;
        UIManager.Instance?.ShowMainMenu();
    }

    // -----------------------------------------------------------------------
    // Game flow
    // -----------------------------------------------------------------------

    public void StartGame()
    {
        ResetGameState();
        isGameOver = false;
        isGameRunning = true;

        NotifyAllUI();
        OnGameStart?.Invoke();

        UIManager.Instance?.ShowPlayerUI();
        SpawnPlayerAtStart();
    }

    public void RestartGame()
    {
        ResetGameState();
        isGameOver = false;
        isGameRunning = true;

        // Reload current scene — OnSceneLoadedNextFrame handles ShowPlayerUI
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMainMenu()
    {
        isGameRunning = false;
        isGameOver = false;
        SceneManager.LoadScene(mainMenuScene);
    }

    // -----------------------------------------------------------------------
    // Spawn
    // -----------------------------------------------------------------------

    private void SpawnPlayerAtStart()
    {
        PlayerRespawn player = FindObjectOfType<PlayerRespawn>();
        if (player == null) return;

        if (defaultSpawnPoint != null)
        {
            player.SetCheckpoint(defaultSpawnPoint);
            player.Respawn();
        }
        else
        {
            Debug.LogWarning("[GameManager] SpawnPoint not found! Tag an empty GO as 'SpawnPoint'.");
        }
    }

    // -----------------------------------------------------------------------
    // Gameplay
    // -----------------------------------------------------------------------

    public void AddCoin(int amount = 1)
    {
        if (!isGameRunning || isGameOver) return;
        currentCoins += amount;
        OnCoinsChanged?.Invoke(currentCoins);
    }

    public void LoseLife()
    {
        if (!isGameRunning || isGameOver) return;
        currentLives = Mathf.Max(0, currentLives - 1);
        OnLivesChanged?.Invoke(currentLives);

        if (currentLives <= 0)
            TriggerGameOver();
        else
            FindObjectOfType<PlayerRespawn>()?.Respawn();
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;
        isGameOver = true;
        isGameRunning = false;

        OnGameOver?.Invoke();
        UIManager.Instance?.ShowEndGameScreen(
            currentScore, currentCoins, FormatTime(elapsedTime));
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private void ResetGameState()
    {
        currentLives = maxLives;
        currentCoins = 0;
        currentScore = 0;
        elapsedTime = 0f;
    }

    private void NotifyAllUI()
    {
        OnLivesChanged?.Invoke(currentLives);
        OnCoinsChanged?.Invoke(currentCoins);
        OnScoreChanged?.Invoke(currentScore);
        OnTimeChanged?.Invoke(elapsedTime);
    }

    public bool IsRunning() => isGameRunning;
    public int GetLives() => currentLives;
    public int GetCoins() => currentCoins;
    public int GetScore() => currentScore;
    public float GetTime() => elapsedTime;

    public string FormatTime(float time)
    {
        int m = Mathf.FloorToInt(time / 60f);
        int s = Mathf.FloorToInt(time % 60f);
        return $"{m:00}:{s:00}";
    }
}