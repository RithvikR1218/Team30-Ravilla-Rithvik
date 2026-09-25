using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    //Player Movement
    public float moveSpeed = 8f;
    public float jumpForce = 8f;

    //Ground check to avoid infinite looping
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;


    // 1 = facing right, -1 = facing left
    public float FacingDirection { get; private set; } = 1f;

    //
    private Rigidbody2D rb;
    private PlayerDash dash;
    private float horizontalInput;
    private bool isGrounded;
    private MovingPlatform currentPlatform;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        dash = GetComponent<PlayerDash>();
    }

    void Update()
    {
        horizontalInput = 0f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) horizontalInput -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) horizontalInput += 1f;
        if (horizontalInput != 0f) FacingDirection = horizontalInput;

        // Reads W or Up Arrow key press for Jump
        bool jumpPressed = Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame;

        if (jumpPressed && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        // Ground detection circle cast at feet position
        if (groundCheck != null)
        {
            Collider2D ground = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            isGrounded = ground != null;
            currentPlatform = isGrounded ? ground.GetComponentInParent<MovingPlatform>() : null;
        }

        // PlayerDash controls velocity while dashing
        if (dash != null && dash.IsDashing) return;

        // Ride along with a moving platform we're standing on
        Vector2 platformVelocity = currentPlatform != null ? currentPlatform.Velocity : Vector2.zero;
        float yVelocity = rb.linearVelocity.y;

        // Match the platform's vertical motion unless we're jumping off it
        if (currentPlatform != null && yVelocity <= platformVelocity.y + 0.1f)
        {
            yVelocity = platformVelocity.y;
        }

        // Apply physics velocity for horizontal movement
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed + platformVelocity.x, yVelocity);
    }
}
