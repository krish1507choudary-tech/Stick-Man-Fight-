
using UnityEngine;

public class BotAIController : MonoBehaviour
{
    public enum BotDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    [Header("Opponent")]
    public Transform player;
    public BotMovement botMovement;
    public Rigidbody2D playerRigidbody;

    [Header("Difficulty")]
    public BotDifficulty difficulty = BotDifficulty.Normal;

    [Header("Facing")]
    public bool facePlayer = true;

    [Header("Movement Distance")]
    public float easyPreferredDistance = 2.5f;
    public float normalPreferredDistance = 2f;
    public float hardPreferredDistance = 1.6f;

    [Header("Movement Tolerance")]
    public float stoppingTolerance = 0.2f;

    [Header("Reaction Settings")]
    public float easyThinkInterval = 1f;
    public float normalThinkInterval = 0.5f;
    public float hardThinkInterval = 0.25f;

    [Header("Jump Behaviour")]
    [Range(0f, 1f)]
    public float easyJumpChance = 0.1f;

    [Range(0f, 1f)]
    public float normalJumpChance = 0.25f;

    [Range(0f, 1f)]
    public float hardJumpChance = 0.4f;

    public float jumpReactionDistance = 3f;

    [Header("Defensive Behaviour")]
    [Range(0f, 1f)]
    public float easyRetreatChance = 0.1f;

    [Range(0f, 1f)]
    public float normalRetreatChance = 0.25f;

    [Range(0f, 1f)]
    public float hardRetreatChance = 0.35f;

    private float nextDecisionTime;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;

        if (botMovement == null)
            botMovement = GetComponent<BotMovement>();

        if (player != null && playerRigidbody == null)
            playerRigidbody = player.GetComponent<Rigidbody2D>();

        if (botMovement == null)
        {
            Debug.LogError("BotMovement is missing!");
            enabled = false;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("BotAIController: Player reference is missing!");
            enabled = false;
            return;
        }

        if (playerRigidbody == null)
            playerRigidbody = player.GetComponent<Rigidbody2D>();

        nextDecisionTime = Time.time;
    }

    private void Update()
    {
        if (player == null || botMovement == null)
            return;

        if (facePlayer)
            UpdateFacing();

        // Movement is checked every frame.
        UpdateMovement();

        // Jump and retreat decisions remain on a timer.
        if (Time.time >= nextDecisionTime)
        {
            MakeReactionDecision();
            nextDecisionTime = Time.time + GetThinkInterval();
        }
    }

    private void UpdateFacing()
    {
        float difference = player.position.x - transform.position.x;

        if (Mathf.Abs(difference) < 0.05f)
            return;

        Vector3 scale = transform.localScale;

        scale.x = Mathf.Abs(originalScale.x) *
                  (difference > 0f ? 1f : -1f);

        transform.localScale = scale;
    }

    private void UpdateMovement()
    {
        float difference = player.position.x - transform.position.x;
        float distance = Mathf.Abs(difference);

        int directionToPlayer = difference >= 0f ? 1 : -1;

        float preferredDistance = GetPreferredDistance();

        // Walk towards the Player while too far away.
        if (distance > preferredDistance + stoppingTolerance)
        {
            botMovement.SetMoveDirection(directionToPlayer);
        }
        // Stop when close enough.
        else
        {
            botMovement.StopMovement();
        }
    }

    private void MakeReactionDecision()
    {
        float difference = player.position.x - transform.position.x;
        float distance = Mathf.Abs(difference);

        int directionToPlayer = difference >= 0f ? 1 : -1;

        bool playerAdvancing = false;

        if (playerRigidbody != null)
        {
            float playerVelocityX = playerRigidbody.linearVelocity.x;

            playerAdvancing =
                Mathf.Abs(playerVelocityX) > 0.2f &&
                Mathf.Sign(playerVelocityX) == -directionToPlayer;
        }

        // Retreat only if the Player gets extremely close.
        if (distance < GetPreferredDistance() - 0.6f &&
            playerAdvancing &&
            Random.value < GetRetreatChance())
        {
            botMovement.SetMoveDirection(-directionToPlayer);
        }

        // React to Player jumping.
        bool playerIsJumping =
            playerRigidbody != null &&
            playerRigidbody.linearVelocity.y > 0.5f;

        if (botMovement.IsGrounded &&
            (distance <= jumpReactionDistance || playerIsJumping))
        {
            if (Random.value < GetJumpChance())
                botMovement.RequestJump();
        }
    }

    private float GetThinkInterval()
    {
        switch (difficulty)
        {
            case BotDifficulty.Easy:
                return easyThinkInterval;

            case BotDifficulty.Hard:
                return hardThinkInterval;

            default:
                return normalThinkInterval;
        }
    }

    private float GetPreferredDistance()
    {
        switch (difficulty)
        {
            case BotDifficulty.Easy:
                return easyPreferredDistance;

            case BotDifficulty.Hard:
                return hardPreferredDistance;

            default:
                return normalPreferredDistance;
        }
    }

    private float GetJumpChance()
    {
        switch (difficulty)
        {
            case BotDifficulty.Easy:
                return easyJumpChance;

            case BotDifficulty.Hard:
                return hardJumpChance;

            default:
                return normalJumpChance;
        }
    }

    private float GetRetreatChance()
    {
        switch (difficulty)
        {
            case BotDifficulty.Easy:
                return easyRetreatChance;

            case BotDifficulty.Hard:
                return hardRetreatChance;

            default:
                return normalRetreatChance;
        }
    }

    private void OnDisable()
    {
        if (botMovement != null)
            botMovement.StopMovement();
    }
}
