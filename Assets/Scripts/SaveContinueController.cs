
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SaveContinueController : MonoBehaviour
{
    [Header("References")]
    public Button saveContinueButton;
    public PoseManager poseManager;

    [Header("Arena Settings")]
    public string arenaSceneName = "Arena";

    [Header("Optional Feedback")]
    public TMP_Text feedbackText;

    private void Awake()
    {
        // Find the button automatically if attached to the button.
        if (saveContinueButton == null)
            saveContinueButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (saveContinueButton != null)
            saveContinueButton.onClick.AddListener(SaveAndContinue);
    }

    private void OnDisable()
    {
        if (saveContinueButton != null)
            saveContinueButton.onClick.RemoveListener(SaveAndContinue);
    }

    public void SaveAndContinue()
    {
        if (poseManager == null)
        {
            Debug.LogError("SaveContinueController: PoseManager is missing!");
            return;
        }

        // Check whether all four slots contain poses.
        if (!poseManager.AreAllSlotsFilled(out int emptySlotIndex))
        {
            string message =
                "Please create a pose in Slot " +
                (emptySlotIndex + 1) + "!";

            ShowMessage(message);

            return;
        }

        // Check whether the Arena scene is available.
        if (string.IsNullOrWhiteSpace(arenaSceneName) ||
            !Application.CanStreamedLevelBeLoaded(arenaSceneName))
        {
            ShowMessage("Arena scene is missing from Build Settings!");

            Debug.LogError(
                "Cannot load Arena scene: " + arenaSceneName
            );

            return;
        }

        Debug.Log("All poses ready! Loading Arena...");

        ShowMessage("All poses saved! Entering Arena...");

        SceneManager.LoadScene(arenaSceneName);
    }

    private void ShowMessage(string message)
    {
        Debug.Log(message);

        if (feedbackText != null)
            feedbackText.text = message;
    }
}
