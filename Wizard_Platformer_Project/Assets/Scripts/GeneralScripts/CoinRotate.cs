using UnityEngine;

/// <summary>
/// CoinRotate: Attach to any coin GameObject to make it spin.
/// Works in 2D (Z axis) and 3D (Y axis).
/// </summary>
public class CoinRotate : MonoBehaviour
{
    [Header("Rotation")]
    [Tooltip("Degrees per second. 180 = half turn per second, 360 = full spin per second.")]
    public float rotateSpeed = 180f;

    [Tooltip("Rotate on Z axis for 2D sprites, Y axis for 3D meshes.")]
    public bool is2D = true;

    [Header("Bob Up and Down (optional)")]
    public bool bob = true;
    public float bobHeight = 0.15f;
    public float bobSpeed = 2f;

    private Vector3 _startPos;

    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        // Spin
        Vector3 axis = is2D ? Vector3.forward : Vector3.up;
        transform.Rotate(axis, rotateSpeed * Time.deltaTime);

        // Bob
        if (bob)
        {
            float newY = _startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(_startPos.x, newY, _startPos.z);
        }
    }
}
