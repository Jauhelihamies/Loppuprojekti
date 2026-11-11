using System.Collections.Generic;
using UnityEngine;

public class TrackSpline : MonoBehaviour
{
    public List<Vector3> controlPoints = new List<Vector3>();

    // Calculates a position along the spline using Bezier Interpolation
    // t is a value between 0.0 and 1.0 representing the progression through the track
    public Vector3 GetPoint(float t)
    {
        if (controlPoints.Count < 2) return Vector3.zero;
        if (controlPoints.Count == 2) return Vector3.Lerp(controlPoints[0], controlPoints[1], t);

        // Simple Catmull-Rom or Bezier interpolation logic for smooth multi-point paths
        int numSections = controlPoints.Count - 1;
        int currPoint = Mathf.Min(Mathf.FloorToInt(t * numSections), numSections - 1);
        float u = (t * numSections) - currPoint;

        Vector3 p0 = controlPoints[Mathf.Max(currPoint - 1, 0)];
        Vector3 p1 = controlPoints[currPoint];
        Vector3 p2 = controlPoints[currPoint + 1];
        Vector3 p3 = controlPoints[Mathf.Min(currPoint + 2, controlPoints.Count - 1)];

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * u +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * u * u +
            (-p0 + 3f * p1 - 3f * p2 + p3) * u * u * u
        );
    }

    // Calculates which direction the track is facing at a specific point
    public Vector3 GetDirection(float t)
    {
        float delta = 0.001f;
        Vector3 p1 = GetPoint(Mathf.Max(0, t - delta));
        Vector3 p2 = GetPoint(Mathf.Min(1, t + delta));
        return (p2 - p1).normalized;
    }
}