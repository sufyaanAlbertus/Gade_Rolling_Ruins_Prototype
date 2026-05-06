using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public int maxLives = 3;

    [Header("Spawn Settings")] // ✅ NEW
    public Transform defaultSpawnPoint;

    private int currentLives;
    private int currentCoins;

    private float elapsedTime;
    private int currentScore;

    private bool isGameRunning = false;
    private bool isGameOver = false;

    // Events
    public System.Action<int> OnLivesChanged;
    public System.Action<int> OnCoinsChanged;
    public System.Action<int> OnScoreChanged;
    public System.Action<float> OnTimeChanged;
    public System.Action OnGameOver;
    public System.Action OnGameStart;

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
        UIManager.Instance?.ShowMainMenu();
    }

    private void Update()
    {
        if (!isGameRunning || isGameOver) return;

        elapsedTime += Time.deltaTime;

        currentScore = Mathf.FloorToInt(elapsedTime);

        OnScoreChanged?.Invoke(currentScore);
        OnTimeChanged?.Invoke(elapsedTime);
    }

    // ------------------------------------------------------------------
    // GAME START / RESTART
    // ------------------------------------------------------------------

    public void StartGame()
    {
        ResetGameState();

        isGameOver = false;
        isGameRunning = true;

        NotifyAllUI();

        OnGameStart?.Invoke();

        UIManager.Instance?.ShowPlayerUI();

        // ✅ MOVE PLAYER TO DEFAULT SPAWN
        SpawnPlayerAtStart();
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void ReturnToMainMenu()
    {
        isGameRunning = false;
        isGameOver = false;

        UIManager.Instance?.ShowMainMenu();
    }

    // ------------------------------------------------------------------
    // SPAWN SYSTEM (NEW)
    // ------------------------------------------------------------------

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
            Debug.LogWarning("Default Spawn Point not assigned in GameManager!");
        }
    }

    // ------------------------------------------------------------------
    // RESET LOGIC
    // ------------------------------------------------------------------

    private void ResetGameState()
    {
        currentLives = maxLives;
        currentCoins = 0;

        elapsedTime = 0f;
        currentScore = 0;
    }

    private void NotifyAllUI()
    {
        OnLivesChanged?.Invoke(currentLives);
        OnCoinsChanged?.Invoke(currentCoins);
        OnScoreChanged?.Invoke(currentScore);
        OnTimeChanged?.Invoke(elapsedTime);
    }

    // ------------------------------------------------------------------
    // GAMEPLAY
    // ------------------------------------------------------------------

    public void AddCoin(int amount = 1)
    {
        if (!isGameRunning || isGameOver) return;

        currentCoins += amount;
        OnCoinsChanged?.Invoke(currentCoins);
    }

    public void LoseLife()
    {
        if (!isGameRunning || isGameOver) return;

        currentLives--;
        currentLives = Mathf.Max(0, currentLives);

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
            currentScore,
            currentCoins,
            FormatTime(elapsedTime)
        );
    }

    // ------------------------------------------------------------------
    // QUIT
    // ------------------------------------------------------------------

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ------------------------------------------------------------------
    // GETTERS
    // ------------------------------------------------------------------

    public bool IsRunning() => isGameRunning;
    public int GetLives() => currentLives;
    public int GetCoins() => currentCoins;
    public int GetScore() => currentScore;
    public float GetTime() => elapsedTime;

    // ------------------------------------------------------------------
    // TIME FORMAT
    // ------------------------------------------------------------------

    public string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}