
using UnityEngine;
using UnityEngine.UI;

public class PoseSlotControls : MonoBehaviour
{
    [Header("Pose Manager")]
    public PoseManager poseManager;

    [Header("Buttons")]
    public Button addSlotButton;
    public Button duplicatePoseButton;
    public Button deletePoseButton;

    private void OnEnable()
    {
        if (addSlotButton != null)
            addSlotButton.onClick.AddListener(AddSlot);

        if (duplicatePoseButton != null)
            duplicatePoseButton.onClick.AddListener(DuplicatePose);

        if (deletePoseButton != null)
            deletePoseButton.onClick.AddListener(DeletePose);
    }

    private void OnDisable()
    {
        if (addSlotButton != null)
            addSlotButton.onClick.RemoveListener(AddSlot);

        if (duplicatePoseButton != null)
            duplicatePoseButton.onClick.RemoveListener(DuplicatePose);

        if (deletePoseButton != null)
            deletePoseButton.onClick.RemoveListener(DeletePose);
    }

    private void AddSlot()
    {
        if (poseManager != null)
            poseManager.AddNextSlot();
    }

    private void DuplicatePose()
    {
        if (poseManager != null)
            poseManager.DuplicateLastPose();
    }

    private void DeletePose()
    {
        if (poseManager != null)
            poseManager.DeleteLastPose();
    }
}
