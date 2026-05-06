using UnityEngine;


public class EnemyProjectile : MonoBehaviour
{
    [Tooltip("Seconds before the projectile destroys itself if it hits nothing.")]
    public float lifetime = 4f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // -----------------------------------------------------------------------
    // Trigger (Is Trigger = ON)
    // -----------------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyProjectile")) return;

        if (other.CompareTag("Player"))
            GameManager.Instance?.LoseLife();

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("EnemyProjectile")) return;

        if (other.CompareTag("Player"))
            GameManager.Instance?.LoseLife();

        Destroy(gameObject);
    }

    // -----------------------------------------------------------------------
    // Collision (Is Trigger = OFF)
    // -----------------------------------------------------------------------

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy") ||
            collision.collider.CompareTag("EnemyProjectile")) return;

        if (collision.collider.CompareTag("Player"))
            GameManager.Instance?.LoseLife();

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Enemy") ||
            collision.collider.CompareTag("EnemyProjectile")) return;

        if (collision.collider.CompareTag("Player"))
            GameManager.Instance?.LoseLife();

        Destroy(gameObject);
    }
}