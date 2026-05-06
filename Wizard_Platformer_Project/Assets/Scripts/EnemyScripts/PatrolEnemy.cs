using UnityEngine;
using UnityEngine.AI;

public class PatrolEnemy : EnemyBase
{
    [Header("Patrol Waypoints")]
    [Tooltip("Drag all waypoint Transforms in patrol order.")]
    public Transform[] waypoints;

    [Header("Patrol Settings")]
    public float reachDistance = 0.8f;
    public float waypointWaitTime = 0f;

    [Header("Chase Settings")]
    [Tooltip("Distance at which the enemy starts AND stops chasing. In range = chase. Out of range = back to patrol immediately.")]
    public float patrolDetectionRange = 5f;

    [Tooltip("Speed multiplier when chasing.")]
    public float chaseSpeedMultiplier = 1.5f;

    // State machine
    private enum State { Patrol, Chase }
    private State _state = State.Patrol;

    // Patrol
    private WaypointLinkedList _waypointList = new WaypointLinkedList();
    private int _currentIndex = 0;
    private float _waitTimer = 0f;
    private bool _isWaiting = false;
    private bool _agentReady = false;

    // Chase
    private Transform _player;
    private float _patrolSpeed; // saved so we can restore it after chase

    // -----------------------------------------------------------------------
    // Spawn init
    // -----------------------------------------------------------------------

    protected override void OnSpawn()
    {
        base.OnSpawn();

        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) _player = playerObj.transform;
        else Debug.LogWarning($"[PatrolEnemy] {name}: No Player tag found.");

        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"[PatrolEnemy] {name}: No waypoints assigned!");
            return;
        }

        foreach (Transform wp in waypoints)
            if (wp != null) _waypointList.Append(wp);

        _waypointList.PrintAll();
        StartCoroutine(InitAfterFrame());
    }

    private System.Collections.IEnumerator InitAfterFrame()
    {
        yield return null;

        if (_agent != null && _agent.isOnNavMesh)
        {
            _agent.speed = speed;
            _patrolSpeed = speed;
            _agentReady = true;
            SetDestination(_currentIndex);
            Debug.Log($"[PatrolEnemy] {name} ready — patrolling.");
        }
        else
        {
            Debug.LogWarning($"[PatrolEnemy] {name}: Agent NOT on NavMesh!");
        }
    }

    // -----------------------------------------------------------------------
    // Move — called every frame by EnemyBase.Update()
    // -----------------------------------------------------------------------

    public override void Move()
    {
        if (!_agentReady || _agent == null || !_agent.isOnNavMesh) return;

        if (_player != null)
        {
            float dist = Vector3.Distance(transform.position, _player.position);

            // Enter chase the moment player is in range
            if (_state == State.Patrol && dist <= patrolDetectionRange)
                EnterChase();

            // Stop chasing THE INSTANT player steps outside range — no buffer
            if (_state == State.Chase && dist > patrolDetectionRange)
                EnterPatrol();
        }

        if (_state == State.Chase)
            UpdateChase();
        else
            UpdatePatrol();
    }

    // -----------------------------------------------------------------------
    // Chase state
    // -----------------------------------------------------------------------

    private void EnterChase()
    {
        _state = State.Chase;
        _isWaiting = false;
        _agent.speed = _patrolSpeed * chaseSpeedMultiplier;
        Debug.Log($"[PatrolEnemy] {name}: spotted player — chasing!");
    }

    private void UpdateChase()
    {
        if (_player == null) { EnterPatrol(); return; }
        _agent.SetDestination(_player.position);
    }

    // -----------------------------------------------------------------------
    // Patrol state
    // -----------------------------------------------------------------------

    private void EnterPatrol()
    {
        _state = State.Patrol;
        _agent.speed = _patrolSpeed;

        // Stop dead immediately — don't coast toward the player's last position
        _agent.ResetPath();
        _agent.velocity = Vector3.zero;

        Debug.Log($"[PatrolEnemy] {name}: player left range — resuming patrol.");
        SetDestination(_currentIndex);
    }

    private void UpdatePatrol()
    {
        if (_waypointList.IsEmpty) return;

        if (_isWaiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f) { _isWaiting = false; AdvanceToNext(); }
            return;
        }

        if (!_agent.pathPending)
        {
            float dist = _agent.remainingDistance;
            if (dist != Mathf.Infinity && dist <= reachDistance)
                OnWaypointReached();
        }
    }

    private void OnWaypointReached()
    {
        if (waypointWaitTime > 0f)
        {
            _isWaiting = true;
            _waitTimer = waypointWaitTime;
            _agent.ResetPath();
        }
        else
        {
            AdvanceToNext();
        }
    }

    private void AdvanceToNext()
    {
        _currentIndex = _waypointList.GetNextIndex(_currentIndex);
        SetDestination(_currentIndex);
    }

    private void SetDestination(int index)
    {
        if (_agent == null || !_agent.isOnNavMesh) return;
        Transform wp = _waypointList.GetWaypoint(index);
        if (wp != null) _agent.SetDestination(wp.position);
    }

    // -----------------------------------------------------------------------
    // EnemyBase abstract
    // -----------------------------------------------------------------------

    public override void Attack()
    {
        Debug.Log($"[PatrolEnemy] {name}: contact with player.");
    }

    public override void Die()
    {
        Debug.Log($"[PatrolEnemy] {name}: destroyed.");
        Destroy(gameObject);
    }

    // -----------------------------------------------------------------------
    // Trigger — waypoint detection only
    // EnemyBase handles all player contact (OnCollisionEnter + OnTriggerEnter)
    // -----------------------------------------------------------------------

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waypoint"))
        {
            // Smooth waypoint switching when enemy physically enters the sphere
            if (_state == State.Patrol && !_isWaiting)
                OnWaypointReached();
        }
        else
        {
            // Player or anything else — let EnemyBase.HitPlayer() handle it
            base.OnTriggerEnter(other);
        }
    }

    // -----------------------------------------------------------------------
    // Gizmos
    // -----------------------------------------------------------------------

    private void OnDrawGizmosSelected()
    {
        // Single range — enemy chases when inside, stops when outside
        Gizmos.color = _state == State.Chase ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, patrolDetectionRange);
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }

        if (Application.isPlaying && _state == State.Chase)
        {
            Gizmos.color = Color.magenta;
            if (_player != null)
                Gizmos.DrawLine(transform.position, _player.position);
        }

        if (Application.isPlaying && !_waypointList.IsEmpty)
        {
            Transform current = _waypointList.GetWaypoint(_currentIndex);
            if (current != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(current.position, 0.35f);
            }
        }
    }
}