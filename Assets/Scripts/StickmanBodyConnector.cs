
using UnityEngine;

public class StickmanBodyConnector : MonoBehaviour
{
    [Header("Joints")]
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

    [Header("Visible Body Parts")]
    public Transform headVisual;
    public Transform bodyVisual;

    public Transform upperLeftArm;
    public Transform lowerLeftArm;

    public Transform upperRightArm;
    public Transform lowerRightArm;

    public Transform upperLeftLeg;
    public Transform lowerLeftLeg;

    public Transform upperRightLeg;
    public Transform lowerRightLeg;

    [Header("Arm Bend Direction")]
    [Tooltip("Use +1 for the left arm's default bend.")]
    public float leftArmBendDirection = 1f;

    [Tooltip("Use -1 for the right arm's default bend.")]
    public float rightArmBendDirection = -1f;

    [Header("Pose Playback")]
    public bool useExternalPose = false;

    private JointDragController dragController;

    private float torsoLength;
    private float headNeckLength;
    private Vector3 headNeckOffset;

    private float leftUpperArmLength;
    private float leftLowerArmLength;
    private float rightUpperArmLength;
    private float rightLowerArmLength;

    private float leftUpperLegLength;
    private float leftLowerLegLength;
    private float rightUpperLegLength;
    private float rightLowerLegLength;

    private float leftArmBend;
    private float rightArmBend;
    private float leftLegBend;
    private float rightLegBend;

    private Vector3 bodyVisualScale;
    private Vector3 upperLeftArmScale;
    private Vector3 lowerLeftArmScale;
    private Vector3 upperRightArmScale;
    private Vector3 lowerRightArmScale;
    private Vector3 upperLeftLegScale;
    private Vector3 lowerLeftLegScale;
    private Vector3 upperRightLegScale;
    private Vector3 lowerRightLegScale;

    private void Start()
    {
        dragController = GetComponent<JointDragController>();

        if (!ReferencesValid())
        {
            Debug.LogError("Missing Stickman references!");
            enabled = false;
            return;
        }

        torsoLength = Distance(neck, body);
        headNeckLength = Distance(head, neck);
        headNeckOffset = head.position - neck.position;

        leftUpperArmLength = Distance(neck, leftElbow);
        leftLowerArmLength = Distance(leftElbow, leftHand);

        rightUpperArmLength = Distance(neck, rightElbow);
        rightLowerArmLength = Distance(rightElbow, rightHand);

        leftUpperLegLength = Distance(body, leftKnee);
        leftLowerLegLength = Distance(leftKnee, leftFoot);

        rightUpperLegLength = Distance(body, rightKnee);
        rightLowerLegLength = Distance(rightKnee, rightFoot);

        // Force opposite default arm bend directions.
        leftArmBend = Mathf.Sign(leftArmBendDirection);
        rightArmBend = Mathf.Sign(rightArmBendDirection);

        // Keep the original leg bending directions.
        leftLegBend = GetBend(body, leftKnee, leftFoot);
        rightLegBend = GetBend(body, rightKnee, rightFoot);

        bodyVisualScale = bodyVisual.localScale;

        upperLeftArmScale = upperLeftArm.localScale;
        lowerLeftArmScale = lowerLeftArm.localScale;

        upperRightArmScale = upperRightArm.localScale;
        lowerRightArmScale = lowerRightArm.localScale;

        upperLeftLegScale = upperLeftLeg.localScale;
        lowerLeftLegScale = lowerLeftLeg.localScale;

        upperRightLegScale = upperRightLeg.localScale;
        lowerRightLegScale = lowerRightLeg.localScale;

        Debug.Log("Stickman IK System Started!");
    }

    private void LateUpdate()
    {
        if (!ReferencesValid())
            return;

        // Arena playback mode: update visuals without changing joints.
        if (useExternalPose)
        {
            UpdateVisualsOnly();
            return;
        }

        Transform selected = dragController != null
            ? dragController.SelectedJoint
            : null;

        // HEAD
        if (selected == neck)
        {
            head.position = neck.position + headNeckOffset;
        }
        else if (selected == head)
        {
            ConstrainPair(neck, head, headNeckLength);
        }

        headVisual.position = head.position;

        // TORSO
        if (selected == neck)
        {
            ConstrainPair(neck, body, torsoLength);
        }
        else if (selected == body)
        {
            ConstrainPair(body, neck, torsoLength);
        }

        Connect(bodyVisual, neck, body);

        // LEFT ARM
        if (selected == leftElbow)
        {
            FollowJoint(leftElbow, leftHand, leftLowerArmLength);

            UpdateBendDirection(
                neck, leftElbow, leftHand, ref leftArmBend
            );
        }
        else
        {
            SolveIK(
                neck, leftElbow, leftHand,
                leftUpperArmLength, leftLowerArmLength,
                leftArmBend
            );
        }

        // RIGHT ARM
        if (selected == rightElbow)
        {
            FollowJoint(rightElbow, rightHand, rightLowerArmLength);

            UpdateBendDirection(
                neck, rightElbow, rightHand, ref rightArmBend
            );
        }
        else
        {
            SolveIK(
                neck, rightElbow, rightHand,
                rightUpperArmLength, rightLowerArmLength,
                rightArmBend
            );
        }

        // LEFT LEG
        if (selected == leftKnee)
        {
            FollowJoint(leftKnee, leftFoot, leftLowerLegLength);

            UpdateBendDirection(
                body, leftKnee, leftFoot, ref leftLegBend
            );
        }
        else
        {
            SolveIK(
                body, leftKnee, leftFoot,
                leftUpperLegLength, leftLowerLegLength,
                leftLegBend
            );
        }

        // RIGHT LEG
        if (selected == rightKnee)
        {
            FollowJoint(rightKnee, rightFoot, rightLowerLegLength);

            UpdateBendDirection(
                body, rightKnee, rightFoot, ref rightLegBend
            );
        }
        else
        {
            SolveIK(
                body, rightKnee, rightFoot,
                rightUpperLegLength, rightLowerLegLength,
                rightLegBend
            );
        }

        // UPDATE BODY PART VISUALS
        Connect(upperLeftArm, neck, leftElbow);
        Connect(lowerLeftArm, leftElbow, leftHand);

        Connect(upperRightArm, neck, rightElbow);
        Connect(lowerRightArm, rightElbow, rightHand);

        Connect(upperLeftLeg, body, leftKnee);
        Connect(lowerLeftLeg, leftKnee, leftFoot);

        Connect(upperRightLeg, body, rightKnee);
        Connect(lowerRightLeg, rightKnee, rightFoot);

        // RESTORE ORIGINAL SCALES
        RestoreScale(bodyVisual, bodyVisualScale);

        RestoreScale(upperLeftArm, upperLeftArmScale);
        RestoreScale(lowerLeftArm, lowerLeftArmScale);

        RestoreScale(upperRightArm, upperRightArmScale);
        RestoreScale(lowerRightArm, lowerRightArmScale);

        RestoreScale(upperLeftLeg, upperLeftLegScale);
        RestoreScale(lowerLeftLeg, lowerLeftLegScale);

        RestoreScale(upperRightLeg, upperRightLegScale);
        RestoreScale(lowerRightLeg, lowerRightLegScale);
    }

    // Updates body parts without changing joint positions.
    private void UpdateVisualsOnly()
    {
        headVisual.position = head.position;

        Connect(bodyVisual, neck, body);

        Connect(upperLeftArm, neck, leftElbow);
        Connect(lowerLeftArm, leftElbow, leftHand);

        Connect(upperRightArm, neck, rightElbow);
        Connect(lowerRightArm, rightElbow, rightHand);

        Connect(upperLeftLeg, body, leftKnee);
        Connect(lowerLeftLeg, leftKnee, leftFoot);

        Connect(upperRightLeg, body, rightKnee);
        Connect(lowerRightLeg, rightKnee, rightFoot);

        RestoreScale(bodyVisual, bodyVisualScale);

        RestoreScale(upperLeftArm, upperLeftArmScale);
        RestoreScale(lowerLeftArm, lowerLeftArmScale);

        RestoreScale(upperRightArm, upperRightArmScale);
        RestoreScale(lowerRightArm, lowerRightArmScale);

        RestoreScale(upperLeftLeg, upperLeftLegScale);
        RestoreScale(lowerLeftLeg, lowerLeftLegScale);

        RestoreScale(upperRightLeg, upperRightLegScale);
        RestoreScale(lowerRightLeg, lowerRightLegScale);
    }

    private void SolveIK(
        Transform start,
        Transform middle,
        Transform end,
        float length1,
        float length2,
        float bendDirection)
    {
        Vector2 a = start.position;
        Vector2 target = end.position;

        Vector2 direction = target - a;
        float distance = direction.magnitude;

        if (distance < 0.001f)
            return;

        direction.Normalize();

        float minDistance = Mathf.Abs(length1 - length2) + 0.001f;
        float maxDistance = length1 + length2 - 0.001f;

        float clampedDistance = Mathf.Clamp(
            distance, minDistance, maxDistance
        );

        Vector2 finalTarget = a + direction * clampedDistance;

        end.position = new Vector3(
            finalTarget.x,
            finalTarget.y,
            end.position.z
        );

        float along = (
            length1 * length1 -
            length2 * length2 +
            clampedDistance * clampedDistance
        ) / (2f * clampedDistance);

        float height = Mathf.Sqrt(
            Mathf.Max(0f, length1 * length1 - along * along)
        );

        Vector2 perpendicular = new Vector2(
            -direction.y,
            direction.x
        );

        Vector2 middlePosition =
            a + direction * along +
            perpendicular * height * bendDirection;

        middle.position = new Vector3(
            middlePosition.x,
            middlePosition.y,
            middle.position.z
        );
    }

    private void FollowJoint(
        Transform middle,
        Transform end,
        float fixedLength)
    {
        Vector2 direction = end.position - middle.position;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.down;

        direction.Normalize();

        Vector2 newPosition =
            (Vector2)middle.position + direction * fixedLength;

        end.position = new Vector3(
            newPosition.x,
            newPosition.y,
            end.position.z
        );
    }

    private void UpdateBendDirection(
        Transform start,
        Transform middle,
        Transform end,
        ref float bendDirection)
    {
        Vector2 a = end.position - start.position;
        Vector2 b = middle.position - start.position;

        float cross = a.x * b.y - a.y * b.x;

        if (Mathf.Abs(cross) < 0.001f)
            return;

        bendDirection = cross > 0f ? 1f : -1f;
    }

    private void ConstrainPair(
        Transform anchor,
        Transform other,
        float fixedLength)
    {
        Vector2 direction = other.position - anchor.position;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.down;

        direction.Normalize();

        Vector2 newPosition =
            (Vector2)anchor.position + direction * fixedLength;

        other.position = new Vector3(
            newPosition.x,
            newPosition.y,
            other.position.z
        );
    }

    private void Connect(
        Transform segment,
        Transform start,
        Transform end)
    {
        if (segment == null || start == null || end == null)
            return;

        Vector3 direction = end.position - start.position;

        segment.position = (start.position + end.position) / 2f;

        float angle = Mathf.Atan2(
            direction.y, direction.x
        ) * Mathf.Rad2Deg - 90f;

        segment.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void RestoreScale(
        Transform segment,
        Vector3 originalScale)
    {
        if (segment != null)
            segment.localScale = originalScale;
    }

    private float Distance(Transform a, Transform b)
    {
        return Vector2.Distance(a.position, b.position);
    }

    private float GetBend(
        Transform start,
        Transform middle,
        Transform end)
    {
        Vector2 a = end.position - start.position;
        Vector2 b = middle.position - start.position;

        float cross = a.x * b.y - a.y * b.x;

        return cross >= 0f ? 1f : -1f;
    }

    private bool ReferencesValid()
    {
        return head != null &&
               neck != null &&
               body != null &&
               leftElbow != null &&
               rightElbow != null &&
               leftHand != null &&
               rightHand != null &&
               leftKnee != null &&
               rightKnee != null &&
               leftFoot != null &&
               rightFoot != null &&
               headVisual != null &&
               bodyVisual != null &&
               upperLeftArm != null &&
               lowerLeftArm != null &&
               upperRightArm != null &&
               lowerRightArm != null &&
               upperLeftLeg != null &&
               lowerLeftLeg != null &&
               upperRightLeg != null &&
               lowerRightLeg != null;
    }
}
