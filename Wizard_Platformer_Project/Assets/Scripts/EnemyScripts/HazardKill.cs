using UnityEngine;


public class HazardKill : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Seconds before this hazard can kill the player again. Prevents instant multi-hit on respawn.")]
    public float hitCooldown = 2f;

    private float _cooldownTimer = 0f;

    private void Update()
    {
        if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
    }

    // 3D trigger
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        TryKill(other.GetComponent<PlayerRespawn>());
    }

    // 2D trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        TryKill(other.GetComponent<PlayerRespawn>());
    }

    private void TryKill(PlayerRespawn playerRespawn)
    {
        // Cooldown — don't fire multiple times immediately after respawn
        if (_cooldownTimer > 0f) return;
        _cooldownTimer = hitCooldown;

        if (GameManager.Instance == null || !GameManager.Instance.IsRunning()) return;

        // Lose one life via GameManager
        // GameManager.LoseLife() already calls PlayerRespawn.Respawn() if lives > 0
        // and TriggerGameOver() if lives reach 0
        GameManager.Instance.LoseLife();
    }
}