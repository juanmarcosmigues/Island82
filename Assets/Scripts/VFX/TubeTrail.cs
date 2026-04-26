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

    [Header("Offset")]
    [Tooltip("Slides the tube along the recorded curve in arc-length units. " +
             "0 = aligned with the curve. -1 = tube has slid forward by the full " +
             "curve length (the tail now sits where the head was; the head extends " +
             "straight past the recorded end). +1 = tube has slid backward by the " +
             "full curve length (head sits where the tail was).")]
    [Range(-1f, 1f)] public float offset = 0f;

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

        // --- Arc-length parameterization of the recorded polyline ---
        // arcLen[i] is the cumulative distance from points[0] to points[i].
        var arcLen = new float[ringCount];
        for (int i = 1; i < ringCount; i++)
            arcLen[i] = arcLen[i - 1] + Vector3.Distance(points[i - 1].pos, points[i].pos);
        float totalLen = arcLen[ringCount - 1];

        // Endpoint tangents used to extrapolate when a ring lands past either end.
        Vector3 startTangent = points[1].pos - points[0].pos;
        startTangent = startTangent.sqrMagnitude > 1e-12f ? startTangent.normalized : Vector3.forward;
        Vector3 endTangent = points[ringCount - 1].pos - points[ringCount - 2].pos;
        endTangent = endTangent.sqrMagnitude > 1e-12f ? endTangent.normalized : Vector3.forward;

        // offset = -1 ? shift each ring forward along the curve by totalLen
        // offset = +1 ? shift each ring backward along the curve by totalLen
        float shift = -offset * totalLen;

        var vertices = new Vector3[vertCount];
        var uvs = new Vector2[vertCount];
        var colors = new Color32[vertCount];
        var triangles = new int[triCount];

        // Marching segment index — both arcLen[i] and (arcLen[i] + shift) are
        // monotonic in i, so we never have to walk backward.
        int segIdx = 0;

        for (int i = 0; i < ringCount; i++)
        {
            // Target arc length for this ring on the (extended) curve.
            float s = arcLen[i] + shift;

            Vector3 ringPos;
            Vector3 ringTangent;
            Vector3 ringUp;

            if (s <= 0f)
            {
                // Before the recorded start — extrapolate backward along the start tangent.
                ringPos = points[0].pos + startTangent * s; // s is negative
                ringTangent = startTangent;
                ringUp = points[0].up;
            }
            else if (s >= totalLen)
            {
                // Past the recorded end — extrapolate forward along the end tangent.
                ringPos = points[ringCount - 1].pos + endTangent * (s - totalLen);
                ringTangent = endTangent;
                ringUp = points[ringCount - 1].up;
            }
            else
            {
                // Inside the polyline — find segment [k, k+1] containing s.
                while (segIdx < ringCount - 2 && arcLen[segIdx + 1] < s) segIdx++;
                int k = segIdx;
                float segLen = arcLen[k + 1] - arcLen[k];
                float u = segLen > 1e-6f ? (s - arcLen[k]) / segLen : 0f;

                ringPos = Vector3.Lerp(points[k].pos, points[k + 1].pos, u);
                Vector3 seg = points[k + 1].pos - points[k].pos;
                ringTangent = seg.sqrMagnitude > 1e-12f ? seg.normalized : startTangent;
                // Lerp the parallel-transported ups; they're nearly parallel so a
                // linear lerp + re-orthogonalization is plenty.
                ringUp = Vector3.Lerp(points[k].up, points[k + 1].up, u);
            }

            // Re-orthogonalize the up against the tangent.
            ringUp = ringUp - Vector3.Dot(ringUp, ringTangent) * ringTangent;
            if (ringUp.sqrMagnitude < 1e-6f)
            {
                ringUp = Vector3.Cross(ringTangent, Vector3.right);
                if (ringUp.sqrMagnitude < 1e-6f)
                    ringUp = Vector3.Cross(ringTangent, Vector3.up);
            }
            ringUp.Normalize();
            Vector3 ringRight = Vector3.Cross(ringTangent, ringUp);

            // Radius and color follow the ring's index (its identity within the
            // tube), not its shifted position. So the head end keeps its head
            // color/radius wherever the tube ends up sliding to.
            float t01 = (float)i / (ringCount - 1);
            float radius = Mathf.Lerp(endWidth, startWidth, t01) * 0.5f;

            Color c = colorOverLife.Evaluate(1f - t01);
            Color32 c32 = new Color32(
                (byte)(Mathf.Clamp01(c.r) * 255f),
                (byte)(Mathf.Clamp01(c.g) * 255f),
                (byte)(Mathf.Clamp01(c.b) * 255f),
                (byte)(Mathf.Clamp01(c.a) * 255f)
            );

            Vector3 centerLocal = transform.InverseTransformPoint(ringPos);
            Vector3 upLocal = transform.InverseTransformDirection(ringUp);
            Vector3 rightLocal = transform.InverseTransformDirection(ringRight);

            int baseIndex = i * resolution;
            for (int j = 0; j < resolution; j++)
            {
                float a = (float)j / resolution * Mathf.PI * 2f;
                Vector3 offsetVec = (Mathf.Cos(a) * rightLocal + Mathf.Sin(a) * upLocal) * radius;
                vertices[baseIndex + j] = centerLocal + offsetVec;
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

    /// <summary>
    /// Returns the world-space position and forward direction of the tube's tip
    /// (head ring) for a given offset value, using the same semantics as the
    /// <c>offset</c> field. Does not modify the mesh.
    ///
    /// At offset = 0  ? tip is at the most recently recorded point.
    /// At offset = -1 ? tip is one full curve length past the recorded head
    ///                  (extrapolated along the end tangent).
    /// At offset = +1 ? tip sits at the recorded tail.
    /// At offset = 0.7 ? tip is 30% of the way from tail to head along the curve.
    ///
    /// Returns false if the trail has fewer than 2 points (not enough info to
    /// define a direction); in that case <paramref name="position"/> falls back
    /// to the single recorded point or the target's position, and
    /// <paramref name="direction"/> falls back to <c>Vector3.forward</c>.
    /// </summary>
    public bool GetTipAtOffset(float offsetValue, out Vector3 position, out Vector3 direction)
    {
        int n = points.Count;
        if (n < 2)
        {
            if (n == 1) position = points[0].pos;
            else position = target != null ? target.position : transform.position;
            direction = Vector3.forward;
            return false;
        }

        // Total arc length of the recorded polyline.
        float totalLen = 0f;
        for (int i = 1; i < n; i++)
            totalLen += Vector3.Distance(points[i - 1].pos, points[i].pos);

        // The tip's original arc length is totalLen; with shift = -offset*totalLen,
        // the tip's target arc length is:
        float s = totalLen - offsetValue * totalLen; // = (1 - offsetValue) * totalLen

        SamplePolylineAtArcLength(s, totalLen, out position, out direction);
        return true;
    }

    /// <summary>
    /// Samples the recorded polyline (extended by linear extrapolation past
    /// either end) at a given arc length. Caller must ensure points.Count >= 2.
    /// </summary>
    private void SamplePolylineAtArcLength(float s, float totalLen,
                                           out Vector3 pos, out Vector3 tangent)
    {
        int n = points.Count;

        Vector3 startTangent = points[1].pos - points[0].pos;
        startTangent = startTangent.sqrMagnitude > 1e-12f ? startTangent.normalized : Vector3.forward;
        Vector3 endTangent = points[n - 1].pos - points[n - 2].pos;
        endTangent = endTangent.sqrMagnitude > 1e-12f ? endTangent.normalized : Vector3.forward;

        if (s <= 0f)
        {
            pos = points[0].pos + startTangent * s; // s negative
            tangent = startTangent;
            return;
        }
        if (s >= totalLen)
        {
            pos = points[n - 1].pos + endTangent * (s - totalLen);
            tangent = endTangent;
            return;
        }

        // Walk segments to find the one containing arc length s.
        float accum = 0f;
        for (int i = 1; i < n; i++)
        {
            float seg = Vector3.Distance(points[i - 1].pos, points[i].pos);
            if (accum + seg >= s)
            {
                float u = seg > 1e-6f ? (s - accum) / seg : 0f;
                pos = Vector3.Lerp(points[i - 1].pos, points[i].pos, u);
                Vector3 dir = points[i].pos - points[i - 1].pos;
                tangent = dir.sqrMagnitude > 1e-12f ? dir.normalized : startTangent;
                return;
            }
            accum += seg;
        }

        // Numerical fallback — shouldn't be reachable given the bounds checks above.
        pos = points[n - 1].pos;
        tangent = endTangent;
    }

    public void Clear() => points.Clear();
}