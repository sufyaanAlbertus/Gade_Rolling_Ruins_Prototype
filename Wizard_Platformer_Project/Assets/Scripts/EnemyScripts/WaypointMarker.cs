using UnityEngine;


public class WaypointMarker : MonoBehaviour
{
    [Tooltip("If true this waypoint is a direction-change intersection for the Boss enemy.")]
    public bool isIntersection = false;

    private void Awake()
    {
        // Auto-tag as Waypoint (must also add "Waypoint" to Tags in Project Settings)
        gameObject.tag = isIntersection ? "Intersection" : "Waypoint";
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = isIntersection ? Color.magenta : Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}
