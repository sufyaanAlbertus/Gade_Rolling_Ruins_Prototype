using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float speed = 3.5f;
    public float size = 1.0f;
    public Color enemyColor = Color.red;

    [Header("Detection")]
    public float detectionRange = 5f;

    // Cooldown prevents multiple LoseLife() calls from one contact
    private float _hitCooldown = 0f;
    private const float HIT_COOLDOWN_TIME = 1.5f;

    protected NavMeshAgent _agent;

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    protected virtual void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Start()
    {
        if (_agent != null) _agent.speed = speed;
        transform.localScale = Vector3.one * size;

        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material = new Material(rend.material);
            rend.material.color = enemyColor;
        }

        OnSpawn();
    }

    protected virtual void Update()
    {
        if (_hitCooldown > 0f) _hitCooldown -= Time.deltaTime;
        Move();
    }

    // -----------------------------------------------------------------------
    // Abstract
    // -----------------------------------------------------------------------

    public abstract void Move();
    public abstract void Attack();
    public abstract void Die();

    protected virtual void OnSpawn()
    {
        Debug.Log($"[EnemyBase] {gameObject.name} spawned. Speed={speed} Size={size}");
    }

    // -----------------------------------------------------------------------
    // Player contact — all four collision/trigger types covered
    // -----------------------------------------------------------------------

    private void HitPlayer()
    {
        if (_hitCooldown > 0f) return;
        _hitCooldown = HIT_COOLDOWN_TIME;
        Debug.Log($"[EnemyBase] {name} hit player — losing life.");
        GameManager.Instance?.LoseLife();
    }

    // 3D trigger (Is Trigger = ON)
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) HitPlayer();
    }

    // 3D solid collider (Is Trigger = OFF) — this is what NavMeshAgent enemies use
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player")) HitPlayer();
    }

    // 2D trigger
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) HitPlayer();
    }

    // 2D solid collider
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player")) HitPlayer();
    }
}