using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("=== Movement ===")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 12f;

    [Header("=== Jump & Gravity ===")]
    public float jumpForce = 8f;
    public float gravity = -9.81f;

    [Header("=== References ===")]
    public Transform cameraTransform;

    private CharacterController controller;
    private float yVelocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        HandleGravityAndJump();
        if (transform.position.y < -10f) // Example: falling off map
        {
            GetComponent<PlayerRespawn>().Respawn();
        }
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical).normalized;

        if (input.magnitude >= 0.1f)
        {
            // Camera-relative direction
            float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);

            // Smooth player rotation
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                rotation,
                rotationSpeed * Time.deltaTime
            );

            Vector3 moveDir = rotation * Vector3.forward;
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        }
    }

    private void HandleGravityAndJump()
    {
        if (controller.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            yVelocity = jumpForce;
        }

        yVelocity += gravity * Time.deltaTime;
        Vector3 verticalMove = new Vector3(0, yVelocity, 0);
        controller.Move(verticalMove * Time.deltaTime);
    }
}