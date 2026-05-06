using UnityEngine;

/// <summary>
/// StationaryEnemy: Stands in place and fires projectiles at the player.
/// Projectile spawns at the tip of a configurable barrel point or
/// automatically calculated from the capsule collider size.
/// </summary>
public class StationaryEnemy : EnemyBase
{
    [Header("Projectile")]
    [Tooltip("Prefab must have Rigidbody + Collider + EnemyProjectile script.")]
    public GameObject projectilePrefab;

    [Tooltip("Shots per second.")]
    public float fireRate = 1.5f;

    [Tooltip("Maximum distance to shoot.")]
    public float fireRange = 6f;

    [Tooltip("Speed the projectile travels.")]
    public float projectileSpeed = 10f;

    [Tooltip("If true, only shoots when player is visible (no walls in the way).")]
    public bool requireLineOfSight = true;

    [Tooltip("Layers that block line of sight (your walls and platforms).")]
    public LayerMask sightBlockLayers;

    [Header("Spawn Point")]
    [Tooltip("Optional: assign an empty child Transform as the exact barrel/spawn point." +
             " If left empty the script calculates it automatically from the capsule size.")]
    public Transform projectileSpawnPoint;

    [Tooltip("Extra forward offset added on top of the capsule radius. Increase if projectile still clips.")]
    public float spawnForwardOffset = 0.2f;

    [Tooltip("Height offset from the enemy's centre. 0 = centre, positive = higher.")]
    public float spawnHeightOffset = 0f;

    private float _fireCooldown = 0f;
    private Transform _player;
    private float _capsuleRadius = 0.5f;

    // -----------------------------------------------------------------------
    // Spawn init
    // -----------------------------------------------------------------------

    protected override void OnSpawn()
    {
        base.OnSpawn();

        // Stop movement
        if (_agent != null && _agent.isOnNavMesh)
            _agent.isStopped = true;
        else if (_agent != null)
            _agent.enabled = false;

        // Get capsule radius so we can spawn OUTSIDE it
        CapsuleCollider cap = GetComponent<CapsuleCollider>();
        if (cap != null) _capsuleRadius = cap.radius * transform.localScale.x;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
        else
            Debug.LogWarning("[StationaryEnemy] No Player tag found.");
    }

    // -----------------------------------------------------------------------
    // Move — called every frame by EnemyBase.Update()
    // -----------------------------------------------------------------------

    public override void Move()
    {
        if (_player == null) return;

        float dist = Vector3.Distance(transform.position, _player.position);
        if (dist > fireRange) return;

        if (requireLineOfSight && !HasLineOfSight()) return;

        // Face player
        Vector3 dir = _player.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), Time.deltaTime * 5f);

        _fireCooldown -= Time.deltaTime;
        if (_fireCooldown <= 0f)
        {
            Attack();
            _fireCooldown = 1f / fireRate;
        }
    }

    // -----------------------------------------------------------------------
    // Attack — spawn projectile OUTSIDE the capsule
    // -----------------------------------------------------------------------

    public override void Attack()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[StationaryEnemy] No projectilePrefab assigned!");
            return;
        }
        if (_player == null) return;

        // ── Use manual spawn point if assigned
        Vector3 spawnPos;
        if (projectileSpawnPoint != null)
        {
            spawnPos = projectileSpawnPoint.position;
        }
        else
        {
            // Auto-calculate: start from enemy centre + height offset
            // then push forward by capsule radius + extra offset
            // so the projectile appears right at the surface of the capsule
            Vector3 centre = transform.position + Vector3.up * spawnHeightOffset;
            spawnPos = centre
                     + transform.forward * (_capsuleRadius + spawnForwardOffset);
        }

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        Vector3 direction = (_player.position + Vector3.up * 0.5f - spawnPos).normalized;

        // 3D
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
            return;
        }

        // 2D
        Rigidbody2D rb2d = proj.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = new Vector2(direction.x, direction.y) * projectileSpeed;
            return;
        }

        Debug.LogWarning("[StationaryEnemy] Projectile prefab has no Rigidbody!");
    }

    public override void Die()
    {
        Destroy(gameObject);
    }

    // -----------------------------------------------------------------------
    // Line of sight
    // -----------------------------------------------------------------------

    private bool HasLineOfSight()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 target = _player.position + Vector3.up * 0.5f;
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, sightBlockLayers))
            return hit.collider.CompareTag("Player");

        return true;
    }

    // -----------------------------------------------------------------------
    // Gizmos
    // -----------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        // Fire range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fireRange);

        // Spawn point preview
        Gizmos.color = Color.cyan;
        if (projectileSpawnPoint != null)
        {
            Gizmos.DrawSphere(projectileSpawnPoint.position, 0.08f);
        }
        else
        {
            CapsuleCollider cap = GetComponent<CapsuleCollider>();
            float r = cap != null ? cap.radius * transform.localScale.x : 0.5f;
            Vector3 sp = transform.position
                       + Vector3.up * spawnHeightOffset
                       + transform.forward * (r + spawnForwardOffset);
            Gizmos.DrawSphere(sp, 0.08f);
        }
    }
}