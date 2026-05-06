using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float speed      = 3.5f;
    public float size       = 1.0f;   // Used to scale the GameObject
    public Color enemyColor = Color.red;

    [Header("Detection")]
    public float detectionRange = 5f;

    // Cached NavMeshAgent
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
        // Apply stats from factory
        _agent.speed = speed;
        transform.localScale = Vector3.one * size;

        // Apply colour to renderer if present
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
        Move();
    }

    // -----------------------------------------------------------------------
    // Abstract methods — MUST be implemented by each enemy subclass
    // -----------------------------------------------------------------------

    
    public abstract void Move();


    public abstract void Attack();


    public abstract void Die();

    protected virtual void OnSpawn()
    {
        Debug.Log($"[EnemyBase] {gameObject.name} spawned. Speed={speed} Size={size}");
    }

 
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[EnemyBase] {name} contacted player — player dies.");
            GameManager.Instance?.LoseLife();
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[EnemyBase] {name} contacted player — player dies.");
            GameManager.Instance?.LoseLife();
        }
    }
}
