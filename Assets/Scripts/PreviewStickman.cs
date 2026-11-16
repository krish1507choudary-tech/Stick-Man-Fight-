
using UnityEngine;

public class PreviewStickman : MonoBehaviour
{
    [Header("Preview Joint Root")]
    public Transform previewJointRoot;

    [Header("Preview Joints")]
    public Transform[] previewJoints;

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

    private class SegmentData
    {
        public float originalDistance;
        public Vector3 originalScale;
    }

    private SegmentData[] segments = new SegmentData[9];

    private void Awake()
    {
        // Record the original lengths and scales.
        SaveSegmentData(0, bodyVisual, 1, 2);

        SaveSegmentData(1, upperLeftArm, 1, 3);
        SaveSegmentData(2, lowerLeftArm, 3, 5);

        SaveSegmentData(3, upperRightArm, 1, 4);
        SaveSegmentData(4, lowerRightArm, 4, 6);

        SaveSegmentData(5, upperLeftLeg, 2, 7);
        SaveSegmentData(6, lowerLeftLeg, 7, 9);

        SaveSegmentData(7, upperRightLeg, 2, 8);
        SaveSegmentData(8, lowerRightLeg, 8, 10);
    }

    private void SaveSegmentData(
        int index,
        Transform segment,
        int startIndex,
        int endIndex)
    {
        if (segment == null || previewJoints == null ||
            previewJoints.Length != 11)
            return;

        segments[index] = new SegmentData
        {
            originalDistance = Vector3.Distance(
                previewJoints[startIndex].position,
                previewJoints[endIndex].position
            ),

            originalScale = segment.localScale
        };
    }

    public void ApplyPose(Vector3[] savedPose)
    {
        if (savedPose == null ||
            savedPose.Length != 11 ||
            previewJoints == null ||
            previewJoints.Length != 11 ||
            previewJointRoot == null)
        {
            Debug.LogError("PreviewStickman: Missing references!");
            return;
        }

        for (int i = 0; i < 11; i++)
        {
            if (previewJoints[i] == null)
                return;

            previewJoints[i].position =
                previewJointRoot.TransformPoint(savedPose[i]);
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        headVisual.position = previewJoints[0].position;

        Connect(bodyVisual, previewJoints[1], previewJoints[2], 0);

        Connect(upperLeftArm, previewJoints[1], previewJoints[3], 1);
        Connect(lowerLeftArm, previewJoints[3], previewJoints[5], 2);

        Connect(upperRightArm, previewJoints[1], previewJoints[4], 3);
        Connect(lowerRightArm, previewJoints[4], previewJoints[6], 4);

        Connect(upperLeftLeg, previewJoints[2], previewJoints[7], 5);
        Connect(lowerLeftLeg, previewJoints[7], previewJoints[9], 6);

        Connect(upperRightLeg, previewJoints[2], previewJoints[8], 7);
        Connect(lowerRightLeg, previewJoints[8], previewJoints[10], 8);
    }

    private void Connect(
        Transform segment,
        Transform start,
        Transform end,
        int index)
    {
        if (segment == null || start == null || end == null ||
            segments[index] == null)
            return;

        Vector3 direction = end.position - start.position;
        float distance = direction.magnitude;

        if (distance < 0.0001f)
            return;

        segment.position = (start.position + end.position) * 0.5f;

        float angle = Mathf.Atan2(direction.y, direction.x)
                      * Mathf.Rad2Deg - 90f;

        segment.rotation = Quaternion.Euler(0f, 0f, angle);

        SegmentData data = segments[index];

        if (data.originalDistance > 0.0001f)
        {
            Vector3 scale = data.originalScale;

            scale.y *= distance / data.originalDistance;

            segment.localScale = scale;
        }
    }
}
