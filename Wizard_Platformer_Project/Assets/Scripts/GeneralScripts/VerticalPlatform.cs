using UnityEngine;

public class VerticalPlatform : MonoBehaviour
{
    public Transform topPoint;    // Highest position
    public Transform bottomPoint; // Lowest position
    public float speed = 3f;      // Movement speed

    private Vector3 target;

    void Start()
    {
        // Start moving toward the top point
        target = topPoint.position;
    }

    void Update()
    {
        // Move platform toward target
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Switch direction when reached
        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            target = (target == topPoint.position) ? bottomPoint.position : topPoint.position;
        }
    }

    // Optional: draw gizmos in editor
    void OnDrawGizmos()
    {
        if (topPoint != null && bottomPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(topPoint.position, 0.2f);
            Gizmos.DrawSphere(bottomPoint.position, 0.2f);
            Gizmos.DrawLine(topPoint.position, bottomPoint.position);
        }
    }
}