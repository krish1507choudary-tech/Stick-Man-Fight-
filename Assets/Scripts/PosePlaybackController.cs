
using System.Collections;
using UnityEngine;

public class PosePlaybackController : MonoBehaviour
{
    [Header("Arena Stickman")]
    public Transform characterRoot;
    public Transform[] joints;
    public StickmanBodyConnector bodyConnector;

    [Header("Playback Settings")]
    public float transitionDuration = 0.5f;
    public float pauseBetweenPoses = 0.2f;
    public bool playOnStart = true;

    private void Awake()
    {
        // Find the body connector automatically if not assigned.
        if (bodyConnector == null && characterRoot != null)
        {
            bodyConnector =
                characterRoot.GetComponentInChildren<StickmanBodyConnector>();
        }

        // Prevent IK from changing the saved joint positions.
        if (bodyConnector != null)
        {
            bodyConnector.useExternalPose = true;
        }
    }

    private void Start()
    {
        if (!ValidateSetup())
            return;

        if (playOnStart)
        {
            StartCoroutine(PlayPosesLoop());
        }
    }

    private bool ValidateSetup()
    {
        if (characterRoot == null ||
            joints == null ||
            joints.Length != 11 ||
            bodyConnector == null)
        {
            Debug.LogError(
                "PosePlaybackController: Missing Arena references!"
            );

            return false;
        }

        for (int i = 0; i < joints.Length; i++)
        {
            if (joints[i] == null)
            {
                Debug.LogError(
                    "Missing Arena joint at index " + i
                );

                return false;
            }
        }

        // Check that all four poses exist.
        for (int i = 0; i < 4; i++)
        {
            if (!PoseSessionData.HasPose(i))
            {
                Debug.LogError(
                    "Pose " + (i + 1) + " is missing!"
                );

                return false;
            }
        }

        Debug.Log("All four poses loaded successfully!");

        return true;
    }

    private IEnumerator PlayPosesLoop()
    {
        // Repeat all four poses continuously.
        while (true)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3[] targetPose =
                    PoseSessionData.GetSavedPose(i);

                Debug.Log("Playing Pose " + (i + 1));

                yield return StartCoroutine(
                    TransitionToPose(targetPose)
                );

                if (pauseBetweenPoses > 0f)
                {
                    yield return new WaitForSeconds(
                        pauseBetweenPoses
                    );
                }
            }
        }
    }

    private IEnumerator TransitionToPose(Vector3[] targetPose)
    {
        if (targetPose == null || targetPose.Length != 11)
            yield break;

        // Capture the current joint positions.
        Vector3[] startPose = new Vector3[11];

        for (int i = 0; i < 11; i++)
        {
            startPose[i] =
                characterRoot.InverseTransformPoint(
                    joints[i].position
                );
        }

        float elapsed = 0f;

        // Smoothly move from the current pose to the next pose.
        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;

            float progress = transitionDuration > 0f
                ? Mathf.Clamp01(elapsed / transitionDuration)
                : 1f;

            progress = Mathf.SmoothStep(0f, 1f, progress);

            for (int i = 0; i < 11; i++)
            {
                Vector3 interpolatedPosition =
                    Vector3.Lerp(
                        startPose[i],
                        targetPose[i],
                        progress
                    );

                joints[i].position =
                    characterRoot.TransformPoint(
                        interpolatedPosition
                    );
            }

            yield return null;
        }

        // Apply the exact final saved positions.
        for (int i = 0; i < 11; i++)
        {
            joints[i].position =
                characterRoot.TransformPoint(targetPose[i]);
        }
    }
}
