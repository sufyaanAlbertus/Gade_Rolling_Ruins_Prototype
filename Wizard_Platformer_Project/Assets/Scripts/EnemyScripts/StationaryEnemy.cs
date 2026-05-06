using UnityEngine;

public class StationaryEnemy : EnemyBase
{
    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float      fireRate  = 2f;    // Shots per second
    public float      fireRange = 6f;

    private float     _fireCooldown = 0f;
    private Transform _player;

    protected override void OnSpawn()
    {
        base.OnSpawn();
        // Disable NavMeshAgent movement — stationary enemy does not move
        if (_agent != null) _agent.isStopped = true;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;
    }

    // -----------------------------------------------------------------------
    // EnemyBase implementation
    // -----------------------------------------------------------------------

    public override void Move()
    {
        // Stationary — no movement. Just face the player.
        if (_player == null) return;
        Vector3 dir = (_player.position - transform.position);
        dir.y = 0;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 5f);

        // Try to fire
        _fireCooldown -= Time.deltaTime;
        float dist = Vector3.Distance(transform.position, _player.position);
        if (_fireCooldown <= 0f && dist <= fireRange)
        {
            Attack();
            _fireCooldown = 1f / fireRate;
        }
    }

    public override void Attack()
    {
        if (projectilePrefab == null || _player == null) return;
        Vector3 spawnPos = transform.position + transform.forward * 0.6f;
        GameObject proj = Instantiate(projectilePrefab, spawnPos, transform.rotation);

        // Give projectile velocity toward player
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 dir = (_player.position - spawnPos).normalized;
            rb.linearVelocity = dir * 8f;
        }

        Debug.Log($"[StationaryEnemy] {name}: fired projectile.");
    }

    public override void Die()
    {
        Debug.Log($"[StationaryEnemy] {name}: destroyed.");
        Destroy(gameObject);
    }
}
