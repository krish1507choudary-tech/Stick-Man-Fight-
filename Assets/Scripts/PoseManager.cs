
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

public class PoseManager : MonoBehaviour
{
    [Header("Main Stickman")]
    public Transform characterRoot;
    public Transform[] joints;

    [Header("Preview System")]
    public PreviewStickman previewStickman;
    public Camera previewCamera;
    public RenderTexture sharedRenderTexture;

    [Header("Pose Slots")]
    public Button[] slotButtons;
    public RawImage[] slotPreviews;
    public GameObject[] slotPlusIcons;

    private UnityAction[] slotClickActions;

    private int visibleSlotCount = 2;
    private int lastSavedSlotIndex = -1;

    private List<int> saveOrder = new List<int>();

    private void Start()
    {
        if (!ReferencesAreValid())
        {
            enabled = false;
            return;
        }

        previewCamera.targetTexture = sharedRenderTexture;
        previewCamera.enabled = false;

        if (!sharedRenderTexture.IsCreated())
            sharedRenderTexture.Create();

        PoseSessionData.EnsureSlotCount(slotButtons.Length);

        // Show only the first two slots initially.
        visibleSlotCount = Mathf.Min(2, slotButtons.Length);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int slotIndex = i;

            slotButtons[i].gameObject.SetActive(
                i < visibleSlotCount
            );

            if (PoseSessionData.HasPose(i))
                saveOrder.Add(i);

            RefreshSlotDisplay(i);

            slotClickActions ??= new UnityAction[slotButtons.Length];

            slotClickActions[i] =
                () => SavePoseToSlot(slotIndex);

            slotButtons[i].onClick.AddListener(
                slotClickActions[i]
            );
        }

        if (saveOrder.Count > 0)
            lastSavedSlotIndex = saveOrder[saveOrder.Count - 1];

        Debug.Log("Pose Manager Started!");
    }

    private bool ReferencesAreValid()
    {
        if (characterRoot == null ||
            joints == null ||
            joints.Length != 11 ||
            previewStickman == null ||
            previewCamera == null ||
            sharedRenderTexture == null)
        {
            Debug.LogError("PoseManager: Missing main or preview references!");
            return false;
        }

        if (slotButtons == null ||
            slotPreviews == null ||
            slotPlusIcons == null ||
            slotButtons.Length != 4 ||
            slotPreviews.Length != 4 ||
            slotPlusIcons.Length != 4)
        {
            Debug.LogError("PoseManager: Assign exactly four slots!");
            return false;
        }

        for (int i = 0; i < 11; i++)
        {
            if (joints[i] == null)
                return false;
        }

        for (int i = 0; i < 4; i++)
        {
            if (slotButtons[i] == null ||
                slotPreviews[i] == null ||
                slotPlusIcons[i] == null)
                return false;
        }

        return true;
    }

    // CAPTURE POSE INTO A SLOT
    private void SavePoseToSlot(int slotIndex)
    {
        Vector3[] pose = new Vector3[joints.Length];

        for (int i = 0; i < joints.Length; i++)
        {
            pose[i] = characterRoot.InverseTransformPoint(
                joints[i].position
            );
        }

        previewStickman.ApplyPose(pose);

        Texture2D thumbnail = CapturePreview();

        if (thumbnail == null)
            return;

        PoseSessionData.StorePose(
            slotIndex, pose, thumbnail
        );

        // Mark this slot as the latest saved pose.
        saveOrder.Remove(slotIndex);
        saveOrder.Add(slotIndex);

        lastSavedSlotIndex = slotIndex;

        RefreshSlotDisplay(slotIndex);

        Debug.Log("Pose saved in Slot " + (slotIndex + 1));
    }

    // ADD NEXT SLOT
    public void AddNextSlot()
    {
        if (visibleSlotCount >= slotButtons.Length)
        {
            Debug.Log("All four slots are already visible!");
            return;
        }

        int newIndex = visibleSlotCount;

        slotButtons[newIndex].gameObject.SetActive(true);

        visibleSlotCount++;

        RefreshSlotDisplay(newIndex);

        Debug.Log("Slot " + (newIndex + 1) + " added!");
    }

    // DUPLICATE THE LAST SAVED POSE
    public void DuplicateLastPose()
    {
        if (lastSavedSlotIndex < 0 ||
            !PoseSessionData.HasPose(lastSavedSlotIndex))
        {
            Debug.LogWarning("Create a pose before duplicating!");
            return;
        }

        // Find the next empty visible slot.
        int targetIndex = -1;

        for (int i = 0; i < visibleSlotCount; i++)
        {
            if (!PoseSessionData.HasPose(i))
            {
                targetIndex = i;
                break;
            }
        }

        if (targetIndex == -1)
        {
            Debug.LogWarning(
                "No empty visible slot! Press ADD to reveal another slot."
            );
            return;
        }

        Vector3[] sourcePose =
            PoseSessionData.GetSavedPose(lastSavedSlotIndex);

        Texture2D sourceThumbnail =
            PoseSessionData.GetThumbnail(lastSavedSlotIndex);

        if (sourcePose == null || sourceThumbnail == null)
            return;

        // Create an independent copy of the thumbnail.
        Texture2D copiedThumbnail =
            Instantiate(sourceThumbnail);

        PoseSessionData.StorePose(
            targetIndex,
            sourcePose,
            copiedThumbnail
        );

        saveOrder.Remove(targetIndex);
        saveOrder.Add(targetIndex);

        lastSavedSlotIndex = targetIndex;

        RefreshSlotDisplay(targetIndex);

        Debug.Log(
            "Pose duplicated into Slot " + (targetIndex + 1)
        );
    }

    // DELETE THE MOST RECENTLY SAVED POSE
    public void DeleteLastPose()
    {
        if (saveOrder.Count == 0)
        {
            Debug.LogWarning("There are no saved poses to delete!");
            return;
        }

        int deleteIndex = saveOrder[saveOrder.Count - 1];

        saveOrder.RemoveAt(saveOrder.Count - 1);

        // Remove only the pose, not the slot.
        slotPreviews[deleteIndex].texture = null;
        PoseSessionData.ClearPose(deleteIndex);

        slotPreviews[deleteIndex].enabled = false;
        slotPlusIcons[deleteIndex].SetActive(true);

        lastSavedSlotIndex =
            saveOrder.Count > 0
            ? saveOrder[saveOrder.Count - 1]
            : -1;

        Debug.Log(
            "Pose deleted from Slot " + (deleteIndex + 1)
        );
    }

    // SAVE AND CONTINUE VALIDATION
    public bool AreAllSlotsFilled(out int emptySlotIndex)
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (!PoseSessionData.HasPose(i))
            {
                emptySlotIndex = i;

                Debug.LogWarning(
                    "Slot " + (i + 1) + " is empty!"
                );

                return false;
            }
        }

        emptySlotIndex = -1;

        Debug.Log("All four pose slots are filled!");

        return true;
    }

    // CAPTURE SHARED RENDER TEXTURE
    private Texture2D CapturePreview()
    {
        if (!sharedRenderTexture.IsCreated())
            sharedRenderTexture.Create();

        RenderTexture previousRT = RenderTexture.active;

        Texture2D image = null;

        try
        {
            previewCamera.Render();

            RenderTexture.active = sharedRenderTexture;

            image = new Texture2D(
                sharedRenderTexture.width,
                sharedRenderTexture.height,
                TextureFormat.RGBA32,
                false
            );

            image.ReadPixels(
                new Rect(
                    0,
                    0,
                    sharedRenderTexture.width,
                    sharedRenderTexture.height
                ),
                0,
                0,
                false
            );

            image.Apply(false, false);

            return image;
        }
        catch (System.Exception exception)
        {
            if (image != null)
                Destroy(image);

            Debug.LogError(
                "Preview capture failed: " + exception.Message
            );

            return null;
        }
        finally
        {
            RenderTexture.active = previousRT;
        }
    }

    // REFRESH ONE SLOT'S DISPLAY
    private void RefreshSlotDisplay(int index)
    {
        Texture2D thumbnail =
            PoseSessionData.GetThumbnail(index);

        if (PoseSessionData.HasPose(index) &&
            thumbnail != null)
        {
            slotPreviews[index].texture = thumbnail;
            slotPreviews[index].enabled = true;
            slotPlusIcons[index].SetActive(false);
        }
        else
        {
            slotPreviews[index].texture = null;
            slotPreviews[index].enabled = false;
            slotPlusIcons[index].SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if (slotClickActions == null || slotButtons == null)
            return;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null &&
                slotClickActions[i] != null)
            {
                slotButtons[i].onClick.RemoveListener(
                    slotClickActions[i]
                );
            }
        }
    }
}


// SESSION STORAGE
public static class PoseSessionData
{
    private static Vector3[][] savedPoses = new Vector3[0][];
    private static Texture2D[] savedThumbnails = new Texture2D[0];

    public static void EnsureSlotCount(int count)
    {
        if (savedPoses.Length == count)
            return;

        for (int i = count; i < savedThumbnails.Length; i++)
        {
            if (savedThumbnails[i] != null)
                Object.Destroy(savedThumbnails[i]);
        }

        System.Array.Resize(ref savedPoses, count);
        System.Array.Resize(ref savedThumbnails, count);
    }

    public static void StorePose(
        int index,
        Vector3[] pose,
        Texture2D thumbnail)
    {
        if (index < 0 ||
            index >= savedPoses.Length ||
            pose == null ||
            thumbnail == null)
            return;

        Texture2D oldThumbnail = savedThumbnails[index];

        savedPoses[index] = (Vector3[])pose.Clone();
        savedThumbnails[index] = thumbnail;

        if (oldThumbnail != null &&
            oldThumbnail != thumbnail)
        {
            Object.Destroy(oldThumbnail);
        }
    }

    public static void ClearPose(int index)
    {
        if (index < 0 || index >= savedPoses.Length)
            return;

        savedPoses[index] = null;

        if (savedThumbnails[index] != null)
        {
            Object.Destroy(savedThumbnails[index]);
            savedThumbnails[index] = null;
        }
    }

    public static bool HasPose(int index)
    {
        return index >= 0 &&
               index < savedPoses.Length &&
               savedPoses[index] != null;
    }

    public static Vector3[] GetSavedPose(int index)
    {
        if (!HasPose(index))
            return null;

        return (Vector3[])savedPoses[index].Clone();
    }

    public static Texture2D GetThumbnail(int index)
    {
        if (index < 0 || index >= savedThumbnails.Length)
            return null;

        return savedThumbnails[index];
    }
}
