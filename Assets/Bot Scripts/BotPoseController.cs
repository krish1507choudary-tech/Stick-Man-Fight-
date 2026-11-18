
using System.Collections;
using UnityEngine;

public class BotPoseController : MonoBehaviour
{
    private enum BotAction
    {
        Punch,
        Kick,
        Defence
    }

    [Header("References")]
    public Transform player;
    public Transform[] joints;
    public StickmanBodyConnector bodyConnector;
    public BotAIController botAI;

    [Header("Fight Stance")]
    public float fightStanceRange = 3.5f;
    public float guardTransitionDuration = 0.35f;

    [Header("Attack Settings")]
    public float attackRange = 3f;
    public float punchRange = 1.5f;

    [Header("Attack Cooldown")]
    public float easyAttackCooldown = 2f;
    public float normalAttackCooldown = 1.2f;
    public float hardAttackCooldown = 0.6f;

    [Header("Pose Animation")]
    public float transitionDuration = 0.25f;
    public float poseHoldDuration = 0.7f;

    [Header("Defence")]
    [Range(0f, 1f)]
    public float defenceChance = 0.15f;

    private Vector3[] originalPose;
    private Vector3[] guardPose;

    private float nextActionTime;
    private bool isPlayingPose;
    private bool guardActive;

    private bool useLeftArm = true;
    private bool useLeftLeg = true;

    private void Start()
    {
        if (player == null ||
            joints == null ||
            joints.Length != 11 ||
            bodyConnector == null ||
            botAI == null)
        {
            Debug.LogError("BotPoseController: Missing references!");
            enabled = false;
            return;
        }

        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] == null)
            {
                Debug.LogError("BotPoseController: Missing joint " + i);
                enabled = false;
                return;
            }
        }

        bodyConnector.useExternalPose = true;

        // Capture the original rig only to calculate limb lengths.
        originalPose = CaptureCurrentPose();

        // Create the proper fighting stance.
        guardPose = CreateGuardPose();

        nextActionTime = Time.time + 1f;

        Debug.Log("Bot Pose System Started!");
    }

    private void Update()
    {
        if (isPlayingPose || player == null)
            return;

        float distance = Mathf.Abs(
            player.position.x - transform.position.x
        );

        // Enter fighting stance when close to the player.
        if (!guardActive && distance <= fightStanceRange)
        {
            StartCoroutine(EnterGuardStance());
            return;
        }

        // Attack only after entering the guard stance.
        if (guardActive &&
            distance <= attackRange &&
            Time.time >= nextActionTime)
        {
            StartCoroutine(PerformAction(distance));
        }
    }

    private IEnumerator EnterGuardStance()
    {
        isPlayingPose = true;

        Debug.Log("Bot entered fighting stance!");

        yield return StartCoroutine(
            AnimatePose(guardPose, guardTransitionDuration)
        );

        guardActive = true;
        nextActionTime = Time.time + 0.5f;

        isPlayingPose = false;
    }

    private IEnumerator PerformAction(float distance)
    {
        isPlayingPose = true;

        BotAction selectedAction;

        if (Random.value < defenceChance)
        {
            selectedAction = BotAction.Defence;
        }
        else if (distance <= punchRange)
        {
            selectedAction = BotAction.Punch;
        }
        else
        {
            selectedAction = BotAction.Kick;
        }

        Vector3[] targetPose = CreatePose(selectedAction);

        Debug.Log("Bot Action: " + selectedAction);

        // Move from guard stance into the attack.
        yield return StartCoroutine(
            AnimatePose(targetPose, transitionDuration)
        );

        // Hold the attack briefly.
        yield return new WaitForSeconds(poseHoldDuration);

        // Return to fighting guard stance, NOT the T-pose.
        yield return StartCoroutine(
            AnimatePose(guardPose, transitionDuration)
        );

        nextActionTime = Time.time + GetAttackCooldown();

        isPlayingPose = false;
    }

    private Vector3[] CreateGuardPose()
    {
        Vector3[] pose = (Vector3[])originalPose.Clone();

        Vector2 neck = pose[1];

        // Both hands raised near the face.
        SetArmGuardPose(pose, 3, 5,
            neck + new Vector2(0.20f, 0.35f));

        SetArmGuardPose(pose, 4, 6,
            neck + new Vector2(0.38f, 0.25f));

        return pose;
    }

    private void SetArmGuardPose(
        Vector3[] pose,
        int elbowIndex,
        int handIndex,
        Vector2 handTarget)
    {
        Vector2 neck = pose[1];

        float upperLength = Vector2.Distance(
            originalPose[1],
            originalPose[elbowIndex]
        );

        float lowerLength = Vector2.Distance(
            originalPose[elbowIndex],
            originalPose[handIndex]
        );

        handTarget = GetReachableTarget(
            neck,
            handTarget,
            upperLength,
            lowerLength
        );

        pose[handIndex] = new Vector3(
            handTarget.x,
            handTarget.y,
            pose[handIndex].z
        );

        Vector2 elbow = SolveElbow(
            neck,
            handTarget,
            upperLength,
            lowerLength,
            GetOriginalBend(1, elbowIndex, handIndex)
        );

        pose[elbowIndex] = new Vector3(
            elbow.x,
            elbow.y,
            pose[elbowIndex].z
        );
    }

    private Vector3[] CreatePose(BotAction action)
    {
        Vector3[] pose = (Vector3[])guardPose.Clone();

        switch (action)
        {
            case BotAction.Punch:
                CreatePunchPose(pose);
                break;

            case BotAction.Kick:
                CreateKickPose(pose);
                break;

            case BotAction.Defence:
                CreateDefencePose(pose);
                break;
        }

        return pose;
    }

    private void CreatePunchPose(Vector3[] pose)
    {
        int elbowIndex = useLeftArm ? 3 : 4;
        int handIndex = useLeftArm ? 5 : 6;

        Vector2 neck = pose[1];

        float upperLength = Vector2.Distance(
            guardPose[1], guardPose[elbowIndex]
        );

        float lowerLength = Vector2.Distance(
            guardPose[elbowIndex], guardPose[handIndex]
        );

        float reach = (upperLength + lowerLength) * 0.90f;

        Vector2 handTarget = neck + new Vector2(reach, 0.03f);

        handTarget = GetReachableTarget(
            neck, handTarget, upperLength, lowerLength
        );

        pose[handIndex] = new Vector3(
            handTarget.x,
            handTarget.y,
            pose[handIndex].z
        );

        Vector2 elbow = SolveElbow(
            neck,
            handTarget,
            upperLength,
            lowerLength,
            GetOriginalBend(1, elbowIndex, handIndex)
        );

        pose[elbowIndex] = new Vector3(
            elbow.x,
            elbow.y,
            pose[elbowIndex].z
        );

        useLeftArm = !useLeftArm;
    }

    private void CreateKickPose(Vector3[] pose)
    {
        int kneeIndex = useLeftLeg ? 7 : 8;
        int footIndex = useLeftLeg ? 9 : 10;

        Vector2 body = pose[2];

        float upperLength = Vector2.Distance(
            guardPose[2], guardPose[kneeIndex]
        );

        float lowerLength = Vector2.Distance(
            guardPose[kneeIndex], guardPose[footIndex]
        );

        float reach = (upperLength + lowerLength) * 0.90f;

        Vector2 kickDirection =
            new Vector2(1f, 0.35f).normalized;

        Vector2 footTarget = body + kickDirection * reach;

        footTarget = GetReachableTarget(
            body, footTarget, upperLength, lowerLength
        );

        pose[footIndex] = new Vector3(
            footTarget.x,
            footTarget.y,
            pose[footIndex].z
        );

        Vector2 knee = SolveElbow(
            body,
            footTarget,
            upperLength,
            lowerLength,
            GetOriginalBend(2, kneeIndex, footIndex)
        );

        pose[kneeIndex] = new Vector3(
            knee.x,
            knee.y,
            pose[kneeIndex].z
        );

        useLeftLeg = !useLeftLeg;
    }

    private void CreateDefencePose(Vector3[] pose)
    {
        // A tighter guard: bring both hands slightly higher.
        Vector2 neck = pose[1];

        SetArmGuardPose(
            pose, 3, 5,
            neck + new Vector2(0.18f, 0.42f)
        );

        SetArmGuardPose(
            pose, 4, 6,
            neck + new Vector2(0.34f, 0.36f)
        );
    }

    private Vector2 GetReachableTarget(
        Vector2 start,
        Vector2 target,
        float length1,
        float length2)
    {
        Vector2 direction = target - start;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        direction.Normalize();

        float minimum = Mathf.Abs(length1 - length2) + 0.01f;
        float maximum = length1 + length2 - 0.01f;

        float desiredDistance = Vector2.Distance(start, target);

        float finalDistance = Mathf.Clamp(
            desiredDistance,
            minimum,
            maximum
        );

        return start + direction * finalDistance;
    }

    private Vector2 SolveElbow(
        Vector2 start,
        Vector2 end,
        float length1,
        float length2,
        float bendDirection)
    {
        Vector2 direction = end - start;

        float distance = direction.magnitude;

        if (distance < 0.001f)
            return start;

        direction.Normalize();

        float minimum = Mathf.Abs(length1 - length2) + 0.001f;
        float maximum = length1 + length2 - 0.001f;

        float finalDistance = Mathf.Clamp(
            distance, minimum, maximum
        );

        float along =
            (length1 * length1 -
             length2 * length2 +
             finalDistance * finalDistance) /
            (2f * finalDistance);

        float height = Mathf.Sqrt(
            Mathf.Max(0f, length1 * length1 - along * along)
        );

        Vector2 perpendicular =
            new Vector2(-direction.y, direction.x);

        return start +
               direction * along +
               perpendicular * height * bendDirection;
    }

    private float GetOriginalBend(
        int startIndex,
        int middleIndex,
        int endIndex)
    {
        Vector2 a =
            originalPose[endIndex] - originalPose[startIndex];

        Vector2 b =
            originalPose[middleIndex] - originalPose[startIndex];

        float cross = a.x * b.y - a.y * b.x;

        return cross >= 0f ? 1f : -1f;
    }

    private Vector3[] CaptureCurrentPose()
    {
        Vector3[] pose = new Vector3[joints.Length];

        for (int i = 0; i < joints.Length; i++)
        {
            pose[i] = transform.InverseTransformPoint(
                joints[i].position
            );
        }

        return pose;
    }

    private IEnumerator AnimatePose(
        Vector3[] targetPose,
        float duration)
    {
        Vector3[] startingPose = CaptureCurrentPose();

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float progress = duration > 0f
                ? Mathf.Clamp01(elapsed / duration)
                : 1f;

            progress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < joints.Length; i++)
            {
                Vector3 position = Vector3.Lerp(
                    startingPose[i],
                    targetPose[i],
                    progress
                );

                joints[i].position =
                    transform.TransformPoint(position);
            }

            yield return null;
        }

        for (int i = 0; i < joints.Length; i++)
        {
            joints[i].position =
                transform.TransformPoint(targetPose[i]);
        }
    }

    private float GetAttackCooldown()
    {
        switch (botAI.difficulty)
        {
            case BotAIController.BotDifficulty.Easy:
                return easyAttackCooldown;

            case BotAIController.BotDifficulty.Hard:
                return hardAttackCooldown;

            default:
                return normalAttackCooldown;
        }
    }
}
