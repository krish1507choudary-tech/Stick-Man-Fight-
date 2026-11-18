
using System.Collections;
using UnityEngine;

public class FighterHitDetection : MonoBehaviour
{
    [Header("Opponent")]
    public FighterHealth opponentHealth;
    public FighterHitDetection opponentHitDetection;

    [Header("My Body Parts - Joint Order")]
    public Transform head;
    public Transform neck;
    public Transform body;
    public Transform leftElbow;
    public Transform rightElbow;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftKnee;
    public Transform rightKnee;
    public Transform leftFoot;
    public Transform rightFoot;

    [Header("Individual Damage")]
    [Min(0f)] public float headDamage = 15f;
    [Min(0f)] public float neckDamage = 10f;
    [Min(0f)] public float bodyDamage = 10f;
    [Min(0f)] public float leftElbowDamage = 5f;
    [Min(0f)] public float rightElbowDamage = 5f;
    [Min(0f)] public float leftHandDamage = 8f;
    [Min(0f)] public float rightHandDamage = 8f;
    [Min(0f)] public float leftKneeDamage = 7f;
    [Min(0f)] public float rightKneeDamage = 7f;
    [Min(0f)] public float leftFootDamage = 12f;
    [Min(0f)] public float rightFootDamage = 12f;

    [Header("Arm Defence")]
    [Range(0f, 100f)]
    public float armDamageReduction = 50f;

    [Header("Hit Detection")]
    [Min(0.01f)] public float contactDistance = 0.4f;
    [Min(0f)] public float hitCooldown = 0.6f;

    [Header("Hit Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    [Range(0f, 1f)]
    public float hitVolume = 1f;

    [Header("Hit Visual Effect")]
    [Tooltip("Assign actual body artwork SpriteRenderers, NOT joint markers.")]
    public SpriteRenderer[] bodyPartRenderers;

    public Color hitColor = Color.red;
    [Min(0.01f)] public float hitFlashDuration = 0.2f;

    [Header("Debug")]
    public bool showHitLogs = true;
    public bool showHitGizmos = true;

    private Transform[] bodyParts;
    private float[] damageValues;

    private bool[,] wasTouching = new bool[11, 11];
    private float[] nextHitTime = new float[11];

    private Color[] originalColors;
    private Coroutine flashCoroutine;

    public Transform[] BodyParts => bodyParts;

    private readonly string[] partNames =
    {
        "Head", "Neck", "Body",
        "Left Elbow", "Right Elbow",
        "Left Hand", "Right Hand",
        "Left Knee", "Right Knee",
        "Left Foot", "Right Foot"
    };

    private void Awake()
    {
        BuildBodyPartArrays();
        CacheOriginalColors();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (opponentHitDetection == null && opponentHealth != null)
        {
            opponentHitDetection =
                opponentHealth.GetComponent<FighterHitDetection>();
        }

        if (opponentHealth == null)
            Debug.LogError(gameObject.name + ": Opponent Health is missing!");

        if (opponentHitDetection == null)
            Debug.LogError(gameObject.name + ": Opponent Hit Detection is missing!");
    }

    private void BuildBodyPartArrays()
    {
        bodyParts = new Transform[]
        {
            head, neck, body,
            leftElbow, rightElbow,
            leftHand, rightHand,
            leftKnee, rightKnee,
            leftFoot, rightFoot
        };

        damageValues = new float[]
        {
            headDamage, neckDamage, bodyDamage,
            leftElbowDamage, rightElbowDamage,
            leftHandDamage, rightHandDamage,
            leftKneeDamage, rightKneeDamage,
            leftFootDamage, rightFootDamage
        };
    }

    private void CacheOriginalColors()
    {
        if (bodyPartRenderers == null)
            return;

        originalColors = new Color[bodyPartRenderers.Length];

        for (int i = 0; i < bodyPartRenderers.Length; i++)
        {
            if (bodyPartRenderers[i] != null)
                originalColors[i] = bodyPartRenderers[i].color;
        }
    }

    private void Update()
    {
        if (opponentHealth == null ||
            opponentHitDetection == null ||
            opponentHealth.IsDead)
            return;

        Transform[] opponentParts = opponentHitDetection.BodyParts;

        if (opponentParts == null)
            return;

        Physics2D.SyncTransforms();

        for (int attacker = 0; attacker < 11; attacker++)
        {
            if (bodyParts[attacker] == null)
                continue;

            for (int defender = 0; defender < 11; defender++)
            {
                if (opponentParts[defender] == null)
                    continue;

                float distance = Vector2.Distance(
                    bodyParts[attacker].position,
                    opponentParts[defender].position
                );

                bool touching = distance <= contactDistance;

                if (touching &&
                    !wasTouching[attacker, defender] &&
                    Time.time >= nextHitTime[attacker])
                {
                    DealDamage(attacker, defender);
                }

                wasTouching[attacker, defender] = touching;
            }
        }
    }

    private void DealDamage(int attacker, int defender)
    {
        float baseDamage = damageValues[attacker];
        float finalDamage = baseDamage;

        bool hitArm =
            defender == 3 ||
            defender == 4 ||
            defender == 5 ||
            defender == 6;

        if (hitArm)
        {
            finalDamage *= 1f - armDamageReduction / 100f;
        }

        if (finalDamage <= 0f)
            return;

        opponentHealth.TakeDamage(finalDamage);

        // Play sound and flash the fighter who received the hit.
        opponentHitDetection.PlayHitFeedback();

        nextHitTime[attacker] = Time.time + hitCooldown;

        if (showHitLogs)
        {
            Debug.Log(
                "\n========== FIGHT HIT ==========" +
                "\nAttacker: " + gameObject.name +
                "\nOpponent: " + opponentHealth.gameObject.name +
                "\nAttacking Part: " + partNames[attacker] +
                "\nDefending Part: " + partNames[defender] +
                "\nBase Damage: " + baseDamage +
                "\nArm Hit: " + (hitArm ? "YES" : "NO") +
                "\nDamage Reduction: " +
                (hitArm ? armDamageReduction + "%" : "0%") +
                "\nFINAL DAMAGE: " + finalDamage +
                "\n==============================="
            );
        }
    }

    public void PlayHitFeedback()
    {
        // Hit sound.
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound, hitVolume);
        }

        // Red flash on actual body artwork.
        if (bodyPartRenderers == null ||
            bodyPartRenderers.Length == 0)
            return;

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        for (int i = 0; i < bodyPartRenderers.Length; i++)
        {
            if (bodyPartRenderers[i] != null)
                bodyPartRenderers[i].color = hitColor;
        }

        yield return new WaitForSeconds(hitFlashDuration);

        RestoreOriginalColors();

        flashCoroutine = null;
    }

    private void RestoreOriginalColors()
    {
        if (bodyPartRenderers == null || originalColors == null)
            return;

        for (int i = 0; i < bodyPartRenderers.Length; i++)
        {
            if (bodyPartRenderers[i] != null)
                bodyPartRenderers[i].color = originalColors[i];
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showHitGizmos)
            return;

        Gizmos.color = Color.yellow;

        Transform[] parts =
        {
            head, neck, body,
            leftElbow, rightElbow,
            leftHand, rightHand,
            leftKnee, rightKnee,
            leftFoot, rightFoot
        };

        foreach (Transform part in parts)
        {
            if (part != null)
            {
                Gizmos.DrawWireSphere(
                    part.position,
                    contactDistance * 0.5f
                );
            }
        }
    }
}
