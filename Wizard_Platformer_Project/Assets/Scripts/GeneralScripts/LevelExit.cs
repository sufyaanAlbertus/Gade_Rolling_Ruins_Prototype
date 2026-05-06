using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// LevelExit: Starts LOCKED. Once the key is collected, player walks into the
/// trigger zone and presses the Interact button (same as NPC interact) to load
/// the next scene.
/// </summary>
public class LevelExit : MonoBehaviour
{
    [Header("Scene Transition")]
    [Tooltip("Exact name of the scene to load (must be in Build Settings).")]
    public string nextSceneName;

    [Header("Visual Feedback")]
    [Tooltip("Shown while locked.")]
    public GameObject lockedVisual;
    [Tooltip("Shown when unlocked.")]
    public GameObject unlockedVisual;

    [Header("Interact Prompt")]
    [Tooltip("'Press E to Enter' UI — shown when unlocked AND player is in range.")]
    public GameObject interactPrompt;

    [Header("Audio (optional)")]
    public AudioClip unlockSound;
    public AudioClip exitSound;

    // -----------------------------------------------------------------------
    // Private state
    // -----------------------------------------------------------------------
    private bool _isUnlocked    = false;
    private bool _playerInRange = false;

    private PlayerControls _controls;

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        _controls = new PlayerControls();
    }

    private void OnEnable()
    {
        _controls.Enable();
        _controls.Player.Interact.performed += OnInteract;
    }

    private void OnDisable()
    {
        _controls.Player.Interact.performed -= OnInteract;
        _controls.Disable();
    }

    private void Start()
    {
        if (lockedVisual   != null) lockedVisual.SetActive(true);
        if (unlockedVisual != null) unlockedVisual.SetActive(false);
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // Input — fires when Interact is pressed (E / gamepad button)
    // -----------------------------------------------------------------------

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        // Both conditions must be true: key collected AND standing in zone
        if (!_isUnlocked || !_playerInRange) return;

        GoToNextLevel();
    }

    // -----------------------------------------------------------------------
    // Public API — called by LevelKey when the player collects the key
    // -----------------------------------------------------------------------

    public void Unlock()
    {
        if (_isUnlocked) return;
        _isUnlocked = true;

        if (lockedVisual   != null) lockedVisual.SetActive(false);
        if (unlockedVisual != null) unlockedVisual.SetActive(true);

        // If player is already standing here, show prompt right away
        if (_playerInRange && interactPrompt != null)
            interactPrompt.SetActive(true);

        if (unlockSound != null)
            AudioSource.PlayClipAtPoint(unlockSound, transform.position);

        Debug.Log("[LevelExit] Unlocked — press Interact to enter.");
    }

    // -----------------------------------------------------------------------
    // Trigger — track whether player is inside the zone
    // -----------------------------------------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;

        // Only show prompt once unlocked
        if (_isUnlocked && interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    // 3D versions
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;

        if (_isUnlocked && interactPrompt != null)
            interactPrompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;

        if (interactPrompt != null)
            interactPrompt.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // Scene Load
    // -----------------------------------------------------------------------

    private void GoToNextLevel()
    {
        if (exitSound != null)
            AudioSource.PlayClipAtPoint(exitSound, transform.position);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next < SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(next);
            else
                Debug.LogWarning("[LevelExit] No next scene. Set nextSceneName or add scenes to Build Settings.");
        }
    }
}
