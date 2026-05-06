using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 10f;
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

    // -----------------------------------------------------------------------
    // Animation
    // -----------------------------------------------------------------------
    private Animator _animator;

    // Animator parameter hashes — faster than string lookup every frame
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashIsSprinting = Animator.StringToHash("IsSprinting");
    private static readonly int HashIsGrounded = Animator.StringToHash("IsGrounded");
    private static readonly int HashJump = Animator.StringToHash("Jump");
    private static readonly int HashInteract = Animator.StringToHash("Interact");
    private static readonly int HashPickUp = Animator.StringToHash("PickUp");

    // -----------------------------------------------------------------------
    // Private state
    // -----------------------------------------------------------------------
    private CharacterController _controller;
    private PlayerControls _controls;

    private Vector2 _moveInput;
    private bool _jumpPressed;
    private bool _sprintHeld;

    private Vector3 _velocity;
    private float _yVelocity;
    private float _currentSpeed;

    private float _coyoteTimer;
    private float _jumpBufferTimer;

    private bool _isGrounded;
    private bool _wasGrounded;

    private Transform _activePlatform;
    private Transform _lastPlatform;
    private Vector3 _lastPlatformPos;
    private Vector3 _platformDelta;

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _controls = new PlayerControls();

        if (_animator == null)
            Debug.LogWarning("[PlayerMovement] No Animator found on player or children.");
    }

    private void OnEnable()
    {
        _controls.Enable();

        _controls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _controls.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        _controls.Player.Jump.performed += ctx => _jumpPressed = true;
        _controls.Player.Sprint.performed += ctx => _sprintHeld = true;
        _controls.Player.Sprint.canceled += ctx => _sprintHeld = false;

        // Interact and PickUp trigger animations from NPCInteractable / LevelKey
        // but we expose them here so any system can call TriggerInteract() / TriggerPickUp()
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    private void Update()
    {
        // Freeze until game starts
        if (GameManager.Instance == null || !GameManager.Instance.IsRunning())
        {
            _moveInput = Vector2.zero;
            _velocity = Vector3.zero;
            _currentSpeed = 0f;
            _jumpPressed = false;
            UpdateAnimator(0f, false);
            return;
        }

        DetectPlatform();
        ApplyPlatformMovement();

        _wasGrounded = _isGrounded;
        GroundCheck();
        HandleTimers();
        HandleMovement();
        HandleJump();
        ApplyGravity();
        Move();

        UpdateAnimator(_currentSpeed, _sprintHeld);

        _jumpPressed = false;

        if (transform.position.y < -10f)
            GetComponent<PlayerRespawn>()?.Respawn();
    }

    // -----------------------------------------------------------------------
    // Animator — called every frame
    // -----------------------------------------------------------------------

    private void UpdateAnimator(float speed, bool sprinting)
    {
        if (_animator == null) return;

        // Speed (0 = idle, 0–moveSpeed = walk, > moveSpeed = sprint)
        _animator.SetFloat(HashSpeed, speed, 0.1f, Time.deltaTime);

        // Grounded
        _animator.SetBool(HashIsGrounded, _isGrounded);

        // Sprint
        _animator.SetBool(HashIsSprinting, sprinting && speed > 0.1f);

        // Jump trigger — fire once when leaving ground via jump
        if (!_wasGrounded && !_isGrounded && _yVelocity > 0)
            _animator.SetTrigger(HashJump);
    }

    // -----------------------------------------------------------------------
    // Public trigger methods — called by NPCInteractable, LevelKey, etc.
    // -----------------------------------------------------------------------

    /// <summary>Call this when the player presses Interact near an NPC.</summary>
    public void TriggerInteract()
    {
        _animator?.SetTrigger(HashInteract);
    }

    /// <summary>Call this when the player collects a coin or key.</summary>
    public void TriggerPickUp()
    {
        _animator?.SetTrigger(HashPickUp);
    }

    // -----------------------------------------------------------------------
    // Platform detection
    // -----------------------------------------------------------------------

    private void DetectPlatform()
    {
        _activePlatform = null;
        Vector3 checkPoint = transform.position + Vector3.down * 0.2f;

        foreach (Transform p in platforms)
        {
            if (p == null) continue;
            Collider col = p.GetComponent<Collider>();
            if (col == null) continue;
            if (col.bounds.Contains(checkPoint)) { _activePlatform = p; break; }
        }

        if (_activePlatform != null)
        {
            if (_activePlatform != _lastPlatform)
            {
                _lastPlatform = _activePlatform;
                _lastPlatformPos = _activePlatform.position;
            }
            _platformDelta = _activePlatform.position - _lastPlatformPos;
            _lastPlatformPos = _activePlatform.position;
        }
        else
        {
            _platformDelta = Vector3.zero;
            _lastPlatform = null;
        }
    }

    private void ApplyPlatformMovement()
    {
        if (_platformDelta != Vector3.zero)
            _controller.Move(_platformDelta);
    }

    // -----------------------------------------------------------------------
    // Movement
    // -----------------------------------------------------------------------

    private void GroundCheck()
    {
        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _yVelocity < 0)
        {
            _yVelocity = -2f;
            _coyoteTimer = coyoteTime;
        }
    }

    private void HandleTimers()
    {
        _coyoteTimer -= Time.deltaTime;
        _jumpBufferTimer -= Time.deltaTime;
        if (_jumpPressed) _jumpBufferTimer = jumpBufferTime;
    }

    private void HandleMovement()
    {
        Vector3 input = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

        float topSpeed = _sprintHeld ? sprintSpeed : moveSpeed;
        float targetSpeed = input.magnitude * topSpeed;

        float accel = _isGrounded
            ? (targetSpeed > _currentSpeed ? acceleration : deceleration)
            : acceleration * airControl;

        _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, accel * Time.deltaTime);

        if (input.magnitude >= 0.1f)
        {
            float targetAngle =
                Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg +
                cameraTransform.eulerAngles.y;

            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation,
                                                   rotationSpeed * Time.deltaTime);
            _velocity = rotation * Vector3.forward * _currentSpeed;
        }
        else
        {
            _velocity = Vector3.zero;
        }
    }

    private void HandleJump()
    {
        if (_jumpBufferTimer > 0 && _coyoteTimer > 0)
        {
            _yVelocity = jumpForce;
            _jumpBufferTimer = 0;
            _coyoteTimer = 0;
        }
    }

    private void ApplyGravity()
    {
        _yVelocity += gravity * Time.deltaTime;
        if (_yVelocity < 0)
            _yVelocity += gravity * fallMultiplier * Time.deltaTime;
    }

    private void Move()
    {
        Vector3 finalMove = _velocity + Vector3.up * _yVelocity;
        _controller.Move(finalMove * Time.deltaTime);
    }
}