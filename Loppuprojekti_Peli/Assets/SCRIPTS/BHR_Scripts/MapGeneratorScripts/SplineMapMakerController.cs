using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

public class SplineMapMakerController : MonoBehaviour
{
    public TrackSpline spline;
    public TrackMeshGenerator meshGenerator;
    public LayerMask groundLayer;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // Check if Mouse and Keyboard are connected/available
        if (Mouse.current == null || Keyboard.current == null) return;

        // 1. Check for Left Mouse Button Click
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Get mouse position from the new system
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                // Lift the node slightly off the direct terrain dirt to prevent visual clipping
                Vector3 nodePosition = hit.point + (Vector3.up * 0.2f);

                spline.controlPoints.Add(nodePosition);

                // Redraw the geometry in real-time
                meshGenerator.GenerateTrack();
            }
        }

        // 2. Check for Backspace Key Press to undo
        if (Keyboard.current.backspaceKey.wasPressedThisFrame && spline.controlPoints.Count > 0)
        {
            spline.controlPoints.RemoveAt(spline.controlPoints.Count - 1);
            meshGenerator.GenerateTrack();
        }
    }
}