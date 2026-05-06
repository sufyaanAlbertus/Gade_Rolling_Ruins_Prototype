using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 12f;
    public float deceleration = 16f;
    public float airControl = 0.5f;
    public float rotationSpeed = 12f;

    [Header("Jump")]
    public float jumpForce = 8f;
    public float gravity = -20f;
    public float fallMultiplier = 2.5f;

    [Header("Jump Assist")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    [Header("Platforms")]
    public Transform[] platforms;

    [Header("References")]
    public Transform cameraTransform;

    private CharacterController controller;
    private PlayerControls controls;

    private Vector2 moveInput;
    private bool jumpPressed;

    private Vector3 velocity;
    private float yVelocity;
    private float currentSpeed;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool isGrounded;

    private Transform activePlatform;
    private Transform lastPlatform;

    private Vector3 lastPlatformPos;
    private Vector3 platformDelta;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Enable();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpPressed = true;
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        // STOP EVERYTHING until game starts
        if (GameManager.Instance == null || !GameManager.Instance.IsRunning())
        {
            moveInput = Vector2.zero;
            velocity = Vector3.zero;
            currentSpeed = 0f;
            jumpPressed = false;
            return;
        }

        DetectPlatform();
        ApplyPlatformMovement();

        GroundCheck();
        HandleTimers();
        HandleMovement();
        HandleJump();
        ApplyGravity();

        Move();

        jumpPressed = false;

        if (transform.position.y < -10f)
            GetComponent<PlayerRespawn>()?.Respawn();
    }

    private void DetectPlatform()
    {
        activePlatform = null;

        Vector3 checkPoint = transform.position + Vector3.down * 0.2f;

        foreach (Transform p in platforms)
        {
            if (p == null) continue;

            Collider col = p.GetComponent<Collider>();
            if (col == null) continue;

            if (col.bounds.Contains(checkPoint))
            {
                activePlatform = p;
                break;
            }
        }

        if (activePlatform != null)
        {
            if (activePlatform != lastPlatform)
            {
                lastPlatform = activePlatform;
                lastPlatformPos = activePlatform.position;
            }

            platformDelta = activePlatform.position - lastPlatformPos;
            lastPlatformPos = activePlatform.position;
        }
        else
        {
            platformDelta = Vector3.zero;
            lastPlatform = null;
        }
    }

    private void ApplyPlatformMovement()
    {
        if (platformDelta != Vector3.zero)
            controller.Move(platformDelta);
    }

    private void GroundCheck()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
            coyoteTimer = coyoteTime;
        }
    }

    private void HandleTimers()
    {
        coyoteTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;

        if (jumpPressed)
            jumpBufferTimer = jumpBufferTime;
    }

    private void HandleMovement()
    {
        Vector3 input = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        float targetSpeed = input.magnitude * moveSpeed;

        if (isGrounded)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                (targetSpeed > currentSpeed ? acceleration : deceleration) * Time.deltaTime
            );
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                acceleration * airControl * Time.deltaTime
            );
        }

        if (input.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg +
                cameraTransform.eulerAngles.y;

            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);

            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                rotation,
                rotationSpeed * Time.deltaTime
            );

            Vector3 moveDir = rotation * Vector3.forward;
            velocity = moveDir * currentSpeed;
        }
        else
        {
            velocity = Vector3.zero;
        }
    }

    private void HandleJump()
    {
        if (jumpBufferTimer > 0 && coyoteTimer > 0)
        {
            yVelocity = jumpForce;
            jumpBufferTimer = 0;
            coyoteTimer = 0;
        }
    }

    private void ApplyGravity()
    {
        yVelocity += gravity * Time.deltaTime;

        if (yVelocity < 0)
            yVelocity += gravity * fallMultiplier * Time.deltaTime;
    }

    private void Move()
    {
        Vector3 finalMove = velocity + Vector3.up * yVelocity;
        controller.Move(finalMove * Time.deltaTime);
    }
}