
using UnityEngine;
using UnityEngine.InputSystem;

public class JointDragController : MonoBehaviour
{
    [Header("Camera")]
    public Camera mainCamera;

    [Header("Body Movement")]
    public Transform bodyJoint;
    public Transform characterRoot;

    [Header("Joint Detection")]
    public LayerMask jointLayer;

    private Transform selectedJoint;
    public Transform SelectedJoint => selectedJoint;

    private Vector3 lastMouseWorldPosition;

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            Debug.LogError("MAIN CAMERA NOT FOUND!");

        if (characterRoot == null)
            Debug.LogError("CHARACTER ROOT NOT ASSIGNED!");

        Debug.Log("Joint Drag Controller Started!");
    }

    private void Update()
    {
        if (Mouse.current == null || mainCamera == null)
            return;

        Vector3 mouseWorldPosition = GetMouseWorldPosition();

        // MOUSE CLICK
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Collider2D hit = Physics2D.OverlapPoint(
                mouseWorldPosition,
                jointLayer
            );

            if (hit != null)
            {
                selectedJoint = hit.transform;

                lastMouseWorldPosition = mouseWorldPosition;

                Debug.Log("Joint Selected: " + selectedJoint.name);
            }
            else
            {
                selectedJoint = null;
                Debug.Log("No Joint Detected!");
            }
        }

        // MOUSE RELEASE
        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (selectedJoint != null)
                Debug.Log("Joint Released: " + selectedJoint.name);

            selectedJoint = null;
        }

        // DRAG SELECTED JOINT
        if (selectedJoint != null &&
            Mouse.current.leftButton.isPressed)
        {
            Vector3 mouseDelta =
                mouseWorldPosition - lastMouseWorldPosition;

            // BODY JOINT MOVES THE ENTIRE STICKMAN
            if (selectedJoint == bodyJoint)
            {
                if (characterRoot != null)
                {
                    characterRoot.position += mouseDelta;
                }
            }
            else
            {
                // MOVE INDIVIDUAL JOINT
                selectedJoint.position += mouseDelta;
            }

            lastMouseWorldPosition = mouseWorldPosition;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        float distance =
            -mainCamera.transform.position.z;

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    mousePosition.x,
                    mousePosition.y,
                    distance
                )
            );

        return new Vector3(
            worldPosition.x,
            worldPosition.y,
            0f
        );
    }
}
