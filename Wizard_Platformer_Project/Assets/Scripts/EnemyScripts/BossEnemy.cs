using UnityEngine;

public class BossEnemy : EnemyBase
{
    [Header("Boss Patrol")]
    public Transform[] waypoints;

    [Header("Boss Settings")]
    [Tooltip("Boss only changes direction at Intersection-tagged waypoints.")]
    public bool intersectionOnly = true;

    private WaypointLinkedList _waypointList = new WaypointLinkedList();
    private int   _currentIndex  = 0;
    private float _reachDistance = 0.8f;

    protected override void OnSpawn()
    {
        base.OnSpawn();
        if (waypoints == null || waypoints.Length == 0) return;
        foreach (Transform wp in waypoints) _waypointList.Append(wp);
        SetDestination(_currentIndex);
    }

    public override void Move()
    {
        if (_waypointList.IsEmpty) return;
        Transform target = _waypointList.GetWaypoint(_currentIndex);
        if (target == null) return;

        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= _reachDistance)
        {
            
            bool canAdvance = !intersectionOnly || target.CompareTag("Intersection");
            if (canAdvance)
            {
                _currentIndex = _waypointList.GetNextIndex(_currentIndex);
                SetDestination(_currentIndex);
            }
        }
    }

    public override void Attack()
    {
        Debug.Log($"[BossEnemy] {name}: contact — player loses life.");
        GameManager.Instance?.LoseLife();
    }

    public override void Die()
    {
        Debug.Log($"[BossEnemy] {name}: boss defeated.");
        Destroy(gameObject);
    }

    private void SetDestination(int index)
    {
        Transform wp = _waypointList.GetWaypoint(index);
        if (wp != null && _agent != null && _agent.isOnNavMesh)
            _agent.SetDestination(wp.position);
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.magenta;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.35f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}
