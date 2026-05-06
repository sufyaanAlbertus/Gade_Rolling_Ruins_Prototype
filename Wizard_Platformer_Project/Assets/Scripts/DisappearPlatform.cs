using UnityEngine;

public class DisappearPlatform : MonoBehaviour
{
    [Header("Settings")]
    public float standTime = 3f;
    public bool autoReactivate = true;
    public float reactivateTime = 5f;

    private float timer;
    private bool playerOnPlatform;
    private bool isDisabled;

    private Renderer platformRenderer;
    private Collider platformCollider;

    private void Start()
    {
        platformRenderer = GetComponent<Renderer>();
        platformCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (isDisabled) return;

        if (playerOnPlatform)
        {
            timer += Time.deltaTime;

            if (timer >= standTime)
            {
                DisablePlatform();
            }
        }
        else
        {
            timer = 0f;
        }
    }

    private void DisablePlatform()
    {
        isDisabled = true;
        playerOnPlatform = false;
        timer = 0f;

        if (platformRenderer != null)
            platformRenderer.enabled = false;

        if (platformCollider != null)
            platformCollider.enabled = false;

        if (autoReactivate)
        {
            CancelInvoke(nameof(ReactivatePlatform));
            Invoke(nameof(ReactivatePlatform), reactivateTime);
        }
    }

    private void ReactivatePlatform()
    {
        if (platformRenderer != null)
            platformRenderer.enabled = true;

        if (platformCollider != null)
            platformCollider.enabled = true;

        isDisabled = false;
        playerOnPlatform = false;
        timer = 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDisabled) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = false;
        }
    }
}