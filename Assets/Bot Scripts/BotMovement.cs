
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class BotMovement : MonoBehaviour
{
    public float moveSpeed = 4.5f;
    public float jumpForce = 8f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private CapsuleCollider2D botCollider;

    private float moveDirection;
    private bool jumpRequested;

    private string debugStatus = "SCRIPT NOT STARTED";

    public bool IsGrounded { get; private set; }

    private void Awake()
    {
        debugStatus = "BOT MOVEMENT AWAKE";

        rb = GetComponent<Rigidbody2D>();
        botCollider = GetComponent<CapsuleCollider2D>();

        if (rb == null || botCollider == null)
        {
            debugStatus = "MISSING PHYSICS COMPONENTS";
            enabled = false;
            return;
        }

        debugStatus = "BOT MOVEMENT READY";
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        IsGrounded = botCollider.IsTouchingLayers(groundLayer);

        Vector2 velocity = rb.linearVelocity;

        velocity.x = moveDirection * moveSpeed;

        if (jumpRequested && IsGrounded)
        {
            velocity.y = jumpForce;
            IsGrounded = false;
        }

        rb.linearVelocity = velocity;
        jumpRequested = false;
    }

    public void SetMoveDirection(float direction)
    {
        moveDirection = Mathf.Clamp(direction, -1f, 1f);

        debugStatus = "MOVEMENT COMMAND: " + moveDirection;
    }

    public void StopMovement()
    {
        moveDirection = 0f;
        debugStatus = "MOVEMENT STOPPED";
    }

    public void RequestJump()
    {
        jumpRequested = true;
    }

    private void OnGUI()
    {
        GUI.color = Color.yellow;

        GUI.Label(
            new Rect(20, 20, 600, 30),
            "BOT DIAGNOSTIC: " + debugStatus
        );

        GUI.Label(
            new Rect(20, 50, 600, 30),
            "Move Direction: " + moveDirection +
            " | Rigidbody: " + (rb != null ? rb.bodyType.ToString() : "NULL")
        );
    }
}
