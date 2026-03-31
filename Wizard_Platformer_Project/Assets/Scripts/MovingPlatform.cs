using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA;   // Left/start position
    public Transform pointB;   // Right/end position
    public float speed = 3f;   // Movement speed

    private Vector3 target;

    void Start()
    {
        // Start moving toward point B
        target = pointB.position;
    }

    void Update()
    {
        // Move platform toward target
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Switch target when reached
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            target = (target == pointA.position) ? pointB.position : pointA.position;
        }
    }

    // Optional: Draw gizmos for points in editor
    void OnDrawGizmos()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(pointA.position, 0.2f);
            Gizmos.DrawSphere(pointB.position, 0.2f);
            Gizmos.DrawLine(pointA.position, pointB.position);
        }
    }
}