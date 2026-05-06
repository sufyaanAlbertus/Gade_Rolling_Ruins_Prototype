using UnityEngine;
using UnityEngine.AI;


public class PatrolEnemy : EnemyBase
{
    [Header("Patrol Waypoints")]
    [Tooltip("Drag all waypoint Transforms in patrol order.")]
    public Transform[] waypoints;

    // Our custom LinkedList — built from the waypoints array at runtime
    private WaypointLinkedList _waypointList = new WaypointLinkedList();

    // Index of the waypoint we are currently moving toward
    private int _currentIndex = 0;

    // How close the enemy needs to be to consider a waypoint reached
    private float _reachDistance = 0.5f;

    // -----------------------------------------------------------------------
    // Spawn init
    // -----------------------------------------------------------------------

    protected override void OnSpawn()
    {
        base.OnSpawn();

        // Build the LinkedList from the Inspector array
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"[PatrolEnemy] {name}: No waypoints assigned!");
            return;
        }

        foreach (Transform wp in waypoints)
            _waypointList.Append(wp);

        _waypointList.PrintAll();

        // Start moving to first waypoint
        SetDestination(_currentIndex);
    }

    // -----------------------------------------------------------------------
    // EnemyBase implementation
    // -----------------------------------------------------------------------

    public override void Move()
    {
        if (_waypointList.IsEmpty) return;

        Transform target = _waypointList.GetWaypoint(_currentIndex);
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist <= _reachDistance)
        {
            // Reached this waypoint — advance to next (wraps at end → loops)
            _currentIndex = _waypointList.GetNextIndex(_currentIndex);
            SetDestination(_currentIndex);
        }
    }

    public override void Attack()
    {
        // Basic patrol enemy does not actively attack — kills on contact via EnemyBase trigger
        Debug.Log($"[PatrolEnemy] {name}: contact attack.");
    }

    public override void Die()
    {
        Debug.Log($"[PatrolEnemy] {name}: destroyed.");
        Destroy(gameObject);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private void SetDestination(int index)
    {
        Transform wp = _waypointList.GetWaypoint(index);
        if (wp != null && _agent != null && _agent.isOnNavMesh)
            _agent.SetDestination(wp.position);
    }

    // -----------------------------------------------------------------------
    // Waypoint trigger — when enemy enters waypoint sphere collider
    // switch to next waypoint (smoother than distance check)
    // -----------------------------------------------------------------------

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waypoint"))
        {
            _currentIndex = _waypointList.GetNextIndex(_currentIndex);
            SetDestination(_currentIndex);
        }
        else
        {
            base.OnTriggerEnter(other);  // pass Player contact up to EnemyBase
        }
    }

    // Draw waypoint gizmos in editor
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
    }
}
