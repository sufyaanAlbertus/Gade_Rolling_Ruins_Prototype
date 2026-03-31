using UnityEngine;

public class DisappearPlatform : MonoBehaviour
{
    [Header("Settings")]
    public float standTime = 3f;         // How long player must stand before disappearing
    public bool autoReactivate = true;    // Should platform come back automatically
    public float reactivateTime = 5f;     // Time before platform reactivates

    private float timer = 0f;
    private bool playerOnPlatform = false;

    private Renderer platformRenderer;
    private Collider platformCollider;

    void Start()
    {
        platformRenderer = GetComponent<Renderer>();
        platformCollider = GetComponent<Collider>();

        if (platformRenderer == null)
            Debug.LogWarning("No Renderer found on this platform!");
        if (platformCollider == null)
            Debug.LogWarning("No Collider found on this platform!");
    }

    void Update()
    {
        if (playerOnPlatform)
        {
            timer += Time.deltaTime;

            if (timer >= standTime)
            {
                // Disable platform
                if (platformRenderer != null)
                    platformRenderer.enabled = false;
                if (platformCollider != null)
                    platformCollider.enabled = false;

                playerOnPlatform = false; // stop counting

                if (autoReactivate)
                    Invoke(nameof(ReactivatePlatform), reactivateTime);
            }
        }
        else
        {
            // Reset timer if player steps off
            timer = 0f;
        }
    }

    private void ReactivatePlatform()
    {
        if (platformRenderer != null)
            platformRenderer.enabled = true;
        if (platformCollider != null)
            platformCollider.enabled = true;
    }

    // Detect player
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            playerOnPlatform = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            playerOnPlatform = false;
    }
}