using UnityEngine;
using UnityEngine.InputSystem;

public class LevelKey : MonoBehaviour
{
    [Header("Audio (optional)")]
    public AudioClip pickupSound;

    [Header("References")]
    public LevelExit levelExit;

    [Header("Prompt (optional)")]
    public GameObject pickupPrompt;

    private bool _playerInRange;
    private bool _collected;

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

        // 🔥 Changed to "Pick"
        controls.Player.Pick.performed += OnPick;
    }

    private void OnDisable()
    {
        controls.Player.Pick.performed -= OnPick;
        controls.Disable();
    }

    private void Start()
    {
        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // Input
    // -----------------------------------------------------------------------

    private void OnPick(InputAction.CallbackContext ctx)
    {
        if (_collected || !_playerInRange) return;

        CollectKey();
    }

    // -----------------------------------------------------------------------
    // Trigger Detection
    // -----------------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (_collected || !other.CompareTag("Player")) return;

        _playerInRange = true;

        if (pickupPrompt != null)
            pickupPrompt.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected || !other.CompareTag("Player")) return;

        _playerInRange = true;

        if (pickupPrompt != null)
            pickupPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // Collection Logic
    // -----------------------------------------------------------------------

    private void CollectKey()
    {
        _collected = true;

        if (pickupPrompt != null)
            pickupPrompt.SetActive(false);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        UIManager.Instance?.ShowKeyCollected();

        if (levelExit != null)
            levelExit.Unlock();
        else
            FindObjectOfType<LevelExit>()?.Unlock();

        Destroy(gameObject);
    }
}