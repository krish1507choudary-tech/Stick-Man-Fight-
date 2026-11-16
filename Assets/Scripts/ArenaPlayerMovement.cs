
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class ArenaPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 8f;

    [Header("Ground Detection")]
    public LayerMask groundLayer = 1;

    private Rigidbody2D rb;
    private CapsuleCollider2D playerCollider;

    private float moveInput;
    private bool jumpRequested;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<CapsuleCollider2D>();

        if (rb == null || playerCollider == null)
        {
            Debug.LogError("Player Rigidbody2D or CapsuleCollider2D is missing!");
            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        // LEFT AND RIGHT MOVEMENT
        moveInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            moveInput -= 1f;

        if (Keyboard.current.dKey.isPressed)
            moveInput += 1f;

        // JUMP INPUT
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // CHECK WHETHER THE PLAYER IS TOUCHING THE GROUND.
        isGrounded = playerCollider.IsTouchingLayers(
            groundLayer.value
        );

        // HORIZONTAL MOVEMENT
        Vector2 velocity = rb.linearVelocity;

        velocity.x = moveInput * moveSpeed;

        // JUMP
        if (jumpRequested && isGrounded)
        {
            velocity.y = jumpForce;
            isGrounded = false;

            Debug.Log("Player Jumped!");
        }

        rb.linearVelocity = velocity;

        // Reset the jump request.
        jumpRequested = false;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }
}
