using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.8f, 0f);

    [Header("Distance (Fixed)")]
    [SerializeField] private float distance = 6.5f;

    [Header("Rotation")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float controllerSensitivity = 180f;
    [SerializeField] private float verticalMinAngle = -25f;
    [SerializeField] private float verticalMaxAngle = 70f;

    [Header("Smoothing")]
    [SerializeField] private float positionSmoothTime = 0.06f;
    [SerializeField] private float rotationSmoothSpeed = 12f;

    [Header("Input")]
    [SerializeField] private bool invertY = false;

    private PlayerControls controls;
    private Vector2 lookInput;

    private float yaw;
    private float pitch;

    private Vector3 positionVelocity;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("Camera missing target!");
            enabled = false;
            return;
        }

        yaw = target.eulerAngles.y;
        pitch = 20f;
    }

    private void LateUpdate()
    {
        // 🔴 HARD STOP when game not running
        if (GameManager.Instance == null || !GameManager.Instance.IsRunning())
        {
            lookInput = Vector2.zero;
            return;
        }

        HandleInput();
        HandleRotation();
        HandleCamera();
    }

    private void HandleInput()
    {
        lookInput = controls.Player.Look.ReadValue<Vector2>();
    }

    private void HandleRotation()
    {
        bool isGamepad = Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame;
        float sensitivity = isGamepad ? controllerSensitivity : mouseSensitivity;

        float deltaX = lookInput.x * sensitivity * Time.deltaTime;
        float deltaY = lookInput.y * sensitivity * Time.deltaTime;

        if (invertY) deltaY = -deltaY;

        yaw += deltaX;
        pitch -= deltaY;

        pitch = Mathf.Clamp(pitch, verticalMinAngle, verticalMaxAngle);
    }

    private void HandleCamera()
    {
        Vector3 targetPoint = target.position + targetOffset;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 desiredPosition = targetPoint + rotation * new Vector3(0, 0, -distance);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref positionVelocity,
            positionSmoothTime
        );

        Quaternion lookRot = Quaternion.LookRotation(targetPoint - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRot,
            rotationSmoothSpeed * Time.deltaTime
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}