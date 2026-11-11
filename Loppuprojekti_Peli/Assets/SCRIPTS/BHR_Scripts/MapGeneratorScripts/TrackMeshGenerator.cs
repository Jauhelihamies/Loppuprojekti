
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class TrackMeshGenerator : MonoBehaviour
{
    public TrackSpline spline;
    public float trackWidth = 8f;
    public int resolution = 50; // Total segments along the track

    [ContextMenu("Generate Track")]
    public void GenerateTrack()
    {
        if (spline == null || spline.controlPoints.Count < 2) return;

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        Mesh mesh = new Mesh();

        int vertexCount = (resolution + 1) * 2;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[resolution * 6];

        int vIndex = 0;
        int tIndex = 0;

        for (int i = 0; i <= resolution; i++)
        {
            float t = (float)i / resolution;
            Vector3 position = spline.GetPoint(t);
            Vector3 direction = spline.GetDirection(t);

            // Calculate side vectors to stretch the track width outwards
            Vector3 left = Vector3.Cross(direction, Vector3.up).normalized;

            // Local coordinates relative to this GameObject
            Vector3 localLeft = transform.InverseTransformPoint(position + (left * (trackWidth / 2f)));
            Vector3 localRight = transform.InverseTransformPoint(position - (left * (trackWidth / 2f)));

            vertices[vIndex] = localLeft;
            vertices[vIndex + 1] = localRight;

            // Generate simple tiled texture coordinates
            uvs[vIndex] = new Vector2(0, t * resolution);
            uvs[vIndex + 1] = new Vector2(1, t * resolution);

            // Stitch the quad faces together using triangles
            if (i < resolution)
            {
                triangles[tIndex] = vIndex;
                triangles[tIndex + 1] = vIndex + 2;
                triangles[tIndex + 2] = vIndex + 1;

                triangles[tIndex + 3] = vIndex + 1;
                triangles[tIndex + 4] = vIndex + 2;
                triangles[tIndex + 5] = vIndex + 3;

                tIndex += 6;
            }

            vIndex += 2;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh; // Instantly updates physics for the wheels
    }
}