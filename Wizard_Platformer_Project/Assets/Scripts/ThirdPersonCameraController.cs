using UnityEngine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("=== Target ===")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.8f, 0f);

    [Header("=== Distance & Zoom ===")]
    [SerializeField] private float defaultDistance = 7.5f;
    [SerializeField] private float minDistance = 2.5f;
    [SerializeField] private float maxDistance = 14f;
    [SerializeField] private float zoomSpeed = 8f;

    [Header("=== Rotation ===")]
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float verticalMinAngle = -20f;
    [SerializeField] private float verticalMaxAngle = 65f;
    [SerializeField] private float smoothTime = 0.12f;

    [Header("=== Collision ===")]
    [SerializeField] private bool enableCollision = true;
    [SerializeField] private LayerMask collisionLayers = ~0;
    [SerializeField] private float collisionBuffer = 0.3f;

    [Header("=== Input ===")]
    [SerializeField] private bool invertY = false;

    private float currentYaw;
    private float currentPitch;
    private float currentDistance;

    private Vector3 velocity;

    private void Awake()
    {
        currentDistance = defaultDistance;
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("ThirdPersonCameraController: No target assigned!");
            enabled = false;
            return;
        }

        currentYaw = target.eulerAngles.y;
        currentPitch = 20f;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        HandleInput();
        UpdateCameraPosition();
    }

    private void HandleInput()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (invertY) mouseY = -mouseY;

        currentYaw += mouseX * rotationSpeed * Time.deltaTime;
        currentPitch -= mouseY * rotationSpeed * Time.deltaTime;

        currentPitch = Mathf.Clamp(currentPitch, verticalMinAngle, verticalMaxAngle);

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
    }

    private void UpdateCameraPosition()
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);

        Vector3 targetPoint = target.position + targetOffset;

        Vector3 desiredPosition = targetPoint + rotation * new Vector3(0, 0, -currentDistance);

        if (enableCollision)
        {
            Vector3 direction = (desiredPosition - targetPoint).normalized;

            if (Physics.SphereCast(targetPoint, 0.4f, direction, out RaycastHit hit, currentDistance, collisionLayers))
            {
                float correctedDistance = hit.distance - collisionBuffer;
                correctedDistance = Mathf.Clamp(correctedDistance, minDistance, maxDistance);

                desiredPosition = targetPoint + rotation * new Vector3(0, 0, -correctedDistance);
            }
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        transform.LookAt(targetPoint);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void ResetToBehindPlayer(float yawOffset = 0f)
    {
        currentYaw = target.eulerAngles.y + yawOffset;
        currentPitch = 18f;
    }
}