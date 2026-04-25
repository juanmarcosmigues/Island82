using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Trail renderer that builds a procedural tube (cylinder) mesh with a configurable
/// number of sides. Attach to any GameObject; the mesh is rendered in world space
/// regardless of how the GameObject moves.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TubeTrail : MonoBehaviour
{
    public enum TrimMode { ByTime, ByDistance }

    [Header("Tracking")]
    [Tooltip("Transform to follow. If null, follows this GameObject.")]
    public Transform target;
    [Tooltip("How the trail's length is limited.")]
    public TrimMode trimMode = TrimMode.ByDistance;
    [Tooltip("Lifetime of each trail point in seconds (ByTime mode only).")]
    public float time = 1f;
    [Tooltip("Maximum trail length in world units (ByDistance mode only).")]
    public float maxLength = 5f;
    [Tooltip("Minimum distance between recorded points.")]
    public float minVertexDistance = 0.05f;
    public bool emitting = true;

    [Header("Tube Shape")]
    [Tooltip("Number of sides around the tube (3 = triangular prism, 8 = octagonal, etc.)")]
    [Range(3, 64)] public int resolution = 8;
    [Tooltip("Radius at the newest (head) end.")]
    public float startWidth = 0.2f;
    [Tooltip("Radius at the oldest (tail) end.")]
    public float endWidth = 0f;

    [Header("Color")]
    public Gradient colorOverLife = new Gradient();

    private struct Point
    {
        public Vector3 pos;
        public float birth;
        public Vector3 up;   // parallel-transported up vector, frozen at insertion time
    }

    private readonly List<Point> points = new List<Point>();
    private Mesh mesh;

    void Awake()
    {
        mesh = new Mesh { name = "TubeTrail" };
        mesh.MarkDynamic();
        GetComponent<MeshFilter>().mesh = mesh;
        if (target == null) target = transform;
    }

    void LateUpdate()
    {
        // Record new point if we moved far enough
        if (emitting && target != null)
        {
            Vector3 p = target.position;
            if (points.Count == 0 ||
                (points[points.Count - 1].pos - p).sqrMagnitude >= minVertexDistance * minVertexDistance)
            {
                AddPoint(p);
            }
        }

        // Cull old points based on selected mode
        if (trimMode == TrimMode.ByTime)
        {
            float now = Time.time;
            while (points.Count > 0 && now - points[0].birth > time)
                points.RemoveAt(0);
        }
        else // ByDistance
        {
            if (points.Count >= 2 && maxLength > 0f)
            {
                float accum = 0f;
                int keepFrom = 0;
                float overshoot = 0f;
                for (int i = points.Count - 1; i > 0; i--)
                {
                    float seg = Vector3.Distance(points[i].pos, points[i - 1].pos);
                    if (accum + seg >= maxLength)
                    {
                        keepFrom = i - 1;
                        overshoot = (accum + seg) - maxLength;
                        break;
                    }
                    accum += seg;
                    keepFrom = i - 1;
                }

                if (keepFrom > 0)
                    points.RemoveRange(0, keepFrom);

                if (overshoot > 0f && points.Count >= 2)
                {
                    Vector3 a = points[0].pos;
                    Vector3 b = points[1].pos;
                    float seg = Vector3.Distance(a, b);
                    if (seg > 1e-6f)
                    {
                        float t = Mathf.Clamp01(overshoot / seg);
                        var p0 = points[0];
                        p0.pos = Vector3.Lerp(a, b, t);
                        points[0] = p0;
                    }
                }
            }
        }

        BuildMesh();
    }

    /// <summary>
    /// Add a point and compute its up vector via parallel transport from the
    /// previous point's stored up. This is the only place orientation is
    /// computed, so once a point is in the list its frame is frozen.
    /// </summary>
    void AddPoint(Vector3 pos)
    {
        Vector3 up;
        if (points.Count == 0)
        {
            // No previous direction — placeholder up; will be re-orthogonalized
            // against the actual tangent in BuildMesh.
            up = Vector3.up;
        }
        else
        {
            Vector3 lastPos = points[points.Count - 1].pos;
            Vector3 lastUp = points[points.Count - 1].up;
            Vector3 tangent = pos - lastPos;
            float len = tangent.magnitude;
            if (len < 1e-6f)
            {
                up = lastUp;
            }
            else
            {
                tangent /= len;
                // Project last up onto plane perpendicular to tangent
                up = lastUp - Vector3.Dot(lastUp, tangent) * tangent;
                if (up.sqrMagnitude < 1e-6f)
                {
                    // Last up was (almost) parallel to tangent — pick any perpendicular
                    Vector3 alt = Vector3.Cross(tangent, Vector3.right);
                    if (alt.sqrMagnitude < 1e-6f)
                        alt = Vector3.Cross(tangent, Vector3.up);
                    up = alt;
                }
                up.Normalize();
            }
        }

        points.Add(new Point { pos = pos, birth = Time.time, up = up });
    }

    void BuildMesh()
    {
        mesh.Clear();
        if (points.Count < 2) return;

        int ringCount = points.Count;
        int vertCount = ringCount * resolution;
        int triCount = (ringCount - 1) * resolution * 6;

        var vertices = new Vector3[vertCount];
        var uvs = new Vector2[vertCount];
        var colors = new Color32[vertCount];
        var triangles = new int[triCount];

        for (int i = 0; i < ringCount; i++)
        {
            // Tangent: forward direction along the trail (from neighbors)
            Vector3 tangent;
            if (i == 0)
                tangent = (points[1].pos - points[0].pos).normalized;
            else if (i == ringCount - 1)
                tangent = (points[i].pos - points[i - 1].pos).normalized;
            else
                tangent = (points[i + 1].pos - points[i - 1].pos).normalized;

            if (tangent.sqrMagnitude < 1e-8f) tangent = Vector3.forward;

            // Use the point's stored up, re-orthogonalized against the current
            // tangent. The stored up was parallel-transported at insertion time,
            // so it's already nearly perpendicular and stable across frames.
            Vector3 up = points[i].up - Vector3.Dot(points[i].up, tangent) * tangent;
            if (up.sqrMagnitude < 1e-6f)
            {
                up = Vector3.Cross(tangent, Vector3.right);
                if (up.sqrMagnitude < 1e-6f)
                    up = Vector3.Cross(tangent, Vector3.up);
            }
            up.Normalize();
            Vector3 right = Vector3.Cross(tangent, up);

            // 0 at tail (oldest), 1 at head (newest)
            float t01 = (float)i / (ringCount - 1);
            float radius = Mathf.Lerp(endWidth, startWidth, t01) * 0.5f;

            // Evaluate gradient and pack as raw sRGB bytes
            Color c = colorOverLife.Evaluate(1f - t01);
            Color32 c32 = new Color32(
                (byte)(Mathf.Clamp01(c.r) * 255f),
                (byte)(Mathf.Clamp01(c.g) * 255f),
                (byte)(Mathf.Clamp01(c.b) * 255f),
                (byte)(Mathf.Clamp01(c.a) * 255f)
            );

            Vector3 centerLocal = transform.InverseTransformPoint(points[i].pos);
            Vector3 upLocal = transform.InverseTransformDirection(up);
            Vector3 rightLocal = transform.InverseTransformDirection(right);

            int baseIndex = i * resolution;
            for (int j = 0; j < resolution; j++)
            {
                float a = (float)j / resolution * Mathf.PI * 2f;
                Vector3 offset = (Mathf.Cos(a) * rightLocal + Mathf.Sin(a) * upLocal) * radius;
                vertices[baseIndex + j] = centerLocal + offset;
                uvs[baseIndex + j] = new Vector2((float)j / resolution, t01);
                colors[baseIndex + j] = c32;
            }
        }

        // Stitch rings into quads
        int ti = 0;
        for (int i = 0; i < ringCount - 1; i++)
        {
            int rowA = i * resolution;
            int rowB = (i + 1) * resolution;
            for (int j = 0; j < resolution; j++)
            {
                int j2 = (j + 1) % resolution;
                triangles[ti++] = rowA + j;
                triangles[ti++] = rowB + j;
                triangles[ti++] = rowA + j2;

                triangles[ti++] = rowA + j2;
                triangles[ti++] = rowB + j;
                triangles[ti++] = rowB + j2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.colors32 = colors;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public void Clear() => points.Clear();
}